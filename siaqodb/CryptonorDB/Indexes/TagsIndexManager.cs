using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sqo.Queries;
using System.Collections;
using Cryptonor;
using Sqo.Indexes;
using Sqo;

namespace Cryptonor.Indexes
{
    class TagsIndexManager
    {
        Dictionary<string, IBTree> cache = new Dictionary<string, IBTree>();
        Siaqodb siaqodb;
        public TagsIndexManager(Siaqodb siaqodb)
        {
            this.siaqodb = siaqodb;
        }
        public IBTree GetIndex(string indexName, Type indexType)
        {
            indexName = "Tag|" + indexName;
            if (!cache.ContainsKey(indexName))
            {
                IBTree index = CreateIndex(indexName, indexType);
                cache[indexName] = index;
            }
            return cache[indexName]; 
        }
        private IBTree CreateIndex(string indexName,Type indexType)
        {
            Type t = typeof(BTree<>).MakeGenericType(indexType);
            ConstructorInfo ctor = t.GetConstructor(new Type[] { typeof(Siaqodb) });
            IBTree index = (IBTree)ctor.Invoke(new object[] { this.siaqodb });
            IndexInfo2 indexInfo = null;
            
            foreach (IndexInfo2 ii in StoredIndexes)
            {
                if (indexName==ii.IndexName)
                {
                    indexInfo = ii;
                    break;
                }
            }
            bool indexExists = false;
            if (indexInfo == null)
            {
                indexInfo = this.BuildIndexInfo(indexName, index);
            }
            else
            {
                indexExists = true;
            }
            index.SetIndexInfo(indexInfo);
            if (!indexExists)
            {
                index.Persist();
            }
            Type nodeType = typeof(BTreeNode<>).MakeGenericType(indexType);
            if (indexInfo.RootOID > 0 && indexExists)
            {
                object rootP = siaqodb.LoadObjectByOID(nodeType, indexInfo.RootOID);
                if (rootP != null)
                {
                    index.SetRoot(rootP);
                }
            }

            return index;
        }
        private IndexInfo2 BuildIndexInfo(string indexName, IBTree index)
        {
            
            IndexInfo2 ii = new IndexInfo2();
            ii.IndexName = indexName;
            ii.RootOID = index.GetRootOid();
            siaqodb.StoreObject(ii);
            storedIndexes.Add(ii);
            return ii;
        }
        private IList<IndexInfo2> storedIndexes;
        public IList<IndexInfo2> StoredIndexes
        {
            get
            {
                if (storedIndexes == null)
                {
                    storedIndexes = siaqodb.LoadAll<IndexInfo2>();
                }
                return storedIndexes;
            }
        }
        public Dictionary<string, object> PrepareUpdateIndexes(int oid)
        {
            Sqo.Meta.SqoTypeInfo ti = siaqodb.CheckDBAndGetSqoTypeInfo<CryptonorObject>();
            Dictionary<string, object> tags = new Dictionary<string, object>();
            if (oid > 0 && oid <= ti.Header.numberOfRecords)
            {

                object intTagsObj = siaqodb.LoadValue(oid, "tags_Int", ti.Type);
                this.CopyDictionary(tags, intTagsObj as IDictionary);

                object dtTagsObj = siaqodb.LoadValue(oid, "tags_DateTime", ti.Type);
                this.CopyDictionary(tags, dtTagsObj as IDictionary);

                object strTagsObj = siaqodb.LoadValue(oid, "tags_String", ti.Type);
                this.CopyDictionary(tags, strTagsObj as IDictionary);

                object dblTagsObj = siaqodb.LoadValue(oid, "tags_Double", ti.Type);
                this.CopyDictionary(tags, dblTagsObj as IDictionary);

                object boolTagsObj = siaqodb.LoadValue(oid, "tags_Bool", ti.Type);
                this.CopyDictionary(tags, boolTagsObj as IDictionary);
                



            }

            return tags;

        }
        private void CopyDictionary(Dictionary<string, object> tags, IDictionary dict_to_copy)
        {
            if (dict_to_copy != null )
            {

                foreach (string key in dict_to_copy.Keys)
                {
                    tags.Add(key, dict_to_copy[key]);
                }

            }
        }
       
        public void UpdateIndexes(int oid, Dictionary<string, object> oldTags, Dictionary<string, object> newTags)
        {
           
            if (oldTags != null && oldTags.Count > 0)
            {
                foreach (string key in oldTags.Keys)
                {
                    
                    if (newTags!=null && newTags.ContainsKey(key))
                    {

                        int c = ((IComparable)newTags[key]).CompareTo(oldTags[key]);
                        if (c != 0)
                        {
                            IBTree index = this.GetIndex(key, newTags[key].GetType());
                            index.RemoveOid(oldTags[key], oid);
                            //add new value(updated)
                            index.AddItem(newTags[key], new int[] { oid });
                            index.AllowPersistance(this.allowPersistence);
                            index.Persist();
                        }
                    }
                    else//tag is removed
                    {
                        IBTree index = this.GetIndex(key, oldTags[key].GetType());
                        index.RemoveOid(oldTags[key], oid);
                        index.AllowPersistance(this.allowPersistence);
                        index.Persist();
                       
                    }
                }
                if (newTags != null)
                {
                    foreach (string key in newTags.Keys)
                    {
                        if (!oldTags.ContainsKey(key))
                        {
                            IBTree index = this.GetIndex(key, newTags[key].GetType());
                            index.AddItem(newTags[key], new int[] { oid });
                            index.AllowPersistance(this.allowPersistence);
                            index.Persist();
                        }
                    }
                }

            }
            else//add
            {
                if (newTags != null)
                {
                    foreach (string key in newTags.Keys)
                    {
                        IBTree index = this.GetIndex(key, newTags[key].GetType());
                        index.AddItem(newTags[key], new int[] { oid });
                        index.AllowPersistance(this.allowPersistence);
                        index.Persist();
                    }
                }
            }
            
        }
        public void UpdateIndexesAfterDelete(int oid, Dictionary<string, object> oldTags)
        {
            if (oldTags != null && oldTags.Count > 0)
            {
                foreach (string key in oldTags.Keys)
                {
                    IBTree index = this.GetIndex(key, oldTags[key].GetType());
                    index.RemoveOid(oldTags[key], oid);
                    index.AllowPersistance(this.allowPersistence);
                    index.Persist();
                }
            }
        }
        
        public bool ExistsIndex(string indexName)
        {
            indexName = "Tag|" + indexName;
            foreach (IndexInfo2 ii in StoredIndexes)
            {
                if (ii.IndexName == indexName)
                    return true;
            }
            return false;
        }

        internal void LoadOidsByIndex(Cryptonor.Queries.CryptonorQuery query, List<int> oids)
        {
            IBTree index = this.GetIndex(query.TagName, query.GetTagType());
            IndexQueryFinder.FindOids(index, query, oids);
           
        }
        internal void Persist()
        {
            if (cache != null)
            {
                foreach (string keyIndex in cache.Keys)
                {
                    IBTree index = cache[keyIndex];
                    index.AllowPersistance(true);
                    if (index != null)
                    {
                        index.Persist();
                    }
                }

            }
        }
        private bool allowPersistence;
        public void AllowPersistence(bool allow)
        {
            this.allowPersistence = allow;
        }
       
    }
}
