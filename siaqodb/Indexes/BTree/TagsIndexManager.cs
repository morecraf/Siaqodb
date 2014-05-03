using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sqo.Queries;

namespace Sqo.Indexes
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
        public Dictionary<string, int> PrepareUpdateIntIndexes(int oid)
        {
            Sqo.Meta.SqoTypeInfo ti = siaqodb.CheckDBAndGetSqoTypeInfo<DotissiObject>();
            if (oid > 0 && oid <= ti.Header.numberOfRecords)
            {

                object intTagsObj = siaqodb.LoadValue(oid, "intTags", ti.Type);
                if (intTagsObj != null)
                {
                    return (Dictionary<string, int>)intTagsObj;
                }

            }

            return null;

        }
        public Dictionary<string, string> PrepareUpdateStrIndexes(int oid)
        {
            Sqo.Meta.SqoTypeInfo ti = siaqodb.CheckDBAndGetSqoTypeInfo<DotissiObject>();
            if (oid > 0 && oid <= ti.Header.numberOfRecords)
            {

                object strTagsObj = siaqodb.LoadValue(oid, "strTags", ti.Type);
                if (strTagsObj != null)
                {
                    return (Dictionary<string, string>)strTagsObj;
                }

            }

            return null;

        }
        public void UpdateIndexes(int oid, Dictionary<string, int> oldTags, Dictionary<string, int> newTags)
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
                            index.Persist();
                        }
                    }
                    else//tag is removed
                    {
                        IBTree index = this.GetIndex(key, oldTags[key].GetType());
                        index.RemoveOid(oldTags[key], oid);
                       
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
                    }
                }
            }
            
        }
        public void UpdateIndexes(int oid, Dictionary<string, string> oldTags, Dictionary<string, string> newTags)
        {
            if (oldTags != null && oldTags.Count > 0)
            {
                foreach (string key in oldTags.Keys)
                {

                    if (newTags != null && newTags.ContainsKey(key))
                    {

                        int c = ((IComparable)newTags[key]).CompareTo(oldTags[key]);
                        if (c != 0)
                        {
                            IBTree index = this.GetIndex(key, newTags[key].GetType());
                            index.RemoveOid(oldTags[key], oid);
                            //add new value(updated)
                            index.AddItem(newTags[key], new int[] { oid });
                            index.Persist();
                        }
                    }
                    else//tag is removed
                    {
                        IBTree index = this.GetIndex(key, oldTags[key].GetType());
                        index.RemoveOid(oldTags[key], oid);

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
                        index.Persist();
                    }
                    
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

        internal void LoadOidsByIndex(Queries.Where where, List<int> oids)
        {
            IBTree index = this.GetIndex(where.Value2.ToString(), where.Value.GetType());
            if (where.OperationType == OperationType.Equal)
            {
                int[] oidsFound = index.FindItem(where.Value);
                if (oidsFound != null)
                {
                    oids.AddRange(oidsFound);

                }
            }

            else if (where.OperationType == OperationType.GreaterThan)
            {
                List<int> oidsFound = index.FindItemsBiggerThan(where.Value);
                if (oidsFound != null)
                {
                    oidsFound.Reverse();
                    oids.AddRange(oidsFound);

                }

            }
            else if (where.OperationType == OperationType.GreaterThanOrEqual)
            {
                List<int> oidsFound = index.FindItemsBiggerThanOrEqual(where.Value);
                if (oidsFound != null)
                {
                    oidsFound.Reverse();
                    oids.AddRange(oidsFound);

                }

            }
            else if (where.OperationType == OperationType.LessThan)
            {
                List<int> oidsFound = index.FindItemsLessThan(where.Value);
                if (oidsFound != null)
                {
                    oids.AddRange(oidsFound);

                }
            }
            else if (where.OperationType == OperationType.LessThanOrEqual)
            {
                List<int> oidsFound = index.FindItemsLessThanOrEqual(where.Value);
                if (oidsFound != null)
                {
                    oids.AddRange(oidsFound);

                }
            }
            else if (where.OperationType == OperationType.StartWith)
            {
                List<int> oidsFound = null;
                if (where.Value2 != null && where.Value2 is StringComparison)
                {
                    oidsFound = index.FindItemsStartsWith(where.Value, false, (StringComparison)where.Value2);
                }
                else
                {
                    oidsFound = index.FindItemsStartsWith(where.Value, true, StringComparison.Ordinal);
                }
                if (oidsFound != null)
                {
                    oids.AddRange(oidsFound);

                }

            }
        }
    }
}
