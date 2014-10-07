using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sqo.Queries;
using System.Collections;
using Cryptonor;
using Sqo.Indexes;
using Sqo;
#if ASYNC
using System.Threading.Tasks;
#endif

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
#if ASYNC
        public async Task<IBTree> GetIndexAsync(string indexName, Type indexType)
        {
            indexName = "Tag|" + indexName;
            if (!cache.ContainsKey(indexName))
            {
                IBTree index = await CreateIndexAsync(indexName, indexType).LibAwait();
                cache[indexName] = index;
            }
            return cache[indexName];
        }
#endif
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
#if ASYNC
        private async Task<IBTree> CreateIndexAsync(string indexName, Type indexType)
        {
            Type t = typeof(BTree<>).MakeGenericType(indexType);
            ConstructorInfo ctor = t.GetConstructor(new Type[] { typeof(Siaqodb) });
            IBTree index = (IBTree)ctor.Invoke(new object[] { this.siaqodb });
            IndexInfo2 indexInfo = null;
            IList<IndexInfo2> stIndexes = await this.GetStoredIndexesAsync().LibAwait();

            foreach (IndexInfo2 ii in stIndexes)
            {
                if (indexName == ii.IndexName)
                {
                    indexInfo = ii;
                    break;
                }
            }
            bool indexExists = false;
            if (indexInfo == null)
            {
                indexInfo = await this.BuildIndexInfoAsync(indexName, index).LibAwait();
            }
            else
            {
                indexExists = true;
            }
            index.SetIndexInfo(indexInfo);
            if (!indexExists)
            {
                await index.PersistAsync().LibAwait();
            }
            Type nodeType = typeof(BTreeNode<>).MakeGenericType(indexType);
            if (indexInfo.RootOID > 0 && indexExists)
            {
                object rootP = await siaqodb.LoadObjectByOIDAsync(nodeType, indexInfo.RootOID).LibAwait();
                if (rootP != null)
                {
                    index.SetRoot(rootP);
                }
            }

            return index;
        }
#endif
        private IndexInfo2 BuildIndexInfo(string indexName, IBTree index)
        {
            
            IndexInfo2 ii = new IndexInfo2();
            ii.IndexName = indexName;
            ii.RootOID = index.GetRootOid();
            siaqodb.StoreObject(ii);
            storedIndexes.Add(ii);
            return ii;
        }
#if ASYNC
        private async Task<IndexInfo2> BuildIndexInfoAsync(string indexName, IBTree index)
        {

            IndexInfo2 ii = new IndexInfo2();
            ii.IndexName = indexName;
            ii.RootOID = index.GetRootOid();
            await siaqodb.StoreObjectAsync(ii).LibAwait();
            storedIndexes.Add(ii);
            return ii;
        }
#endif
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
#if ASYNC
        public async Task<IList<IndexInfo2>> GetStoredIndexesAsync()
        {
           
                if (storedIndexes == null)
                {
                    storedIndexes = await siaqodb.LoadAllAsync<IndexInfo2>().LibAwait();
                }
                return storedIndexes;
            
        }
#endif
        public Dictionary<string, object> PrepareUpdateIndexes(int oid)
        {
            Sqo.Meta.SqoTypeInfo ti = siaqodb.CheckDBAndGetSqoTypeInfo<CryptonorObject>();
            if (oid > 0 && oid <= ti.Header.numberOfRecords)
            {

                byte[] tagsBytes = (byte[])siaqodb.LoadValue(oid, "tagsSerialized", typeof(CryptonorObject));
                return TagsSerializer.GetDictionary(tagsBytes);

            }

            return null;

        }
#if ASYNC
        public async Task<Dictionary<string, object>> PrepareUpdateIndexesAsync(int oid)
        {
            Sqo.Meta.SqoTypeInfo ti = siaqodb.CheckDBAndGetSqoTypeInfo<CryptonorObject>();
            if (oid > 0 && oid <= ti.Header.numberOfRecords)
            {

                byte[] tagsBytes = (byte[])(await siaqodb.LoadValueAsync(oid, "tagsSerialized", typeof(CryptonorObject)).LibAwait());
                return TagsSerializer.GetDictionary(tagsBytes);

            }

            return null;

        }
#endif
       
        public void UpdateIndexes(int oid, Dictionary<string, object> oldTags, Dictionary<string, object> newTags)
        {
           
            if (oldTags != null && oldTags.Count > 0)
            {
                foreach (string key in oldTags.Keys)
                {
                    
                    if (newTags!=null && newTags.ContainsKey(key))
                    {
                        object oldVal=oldTags[key];
                        if (newTags[key].GetType() != oldVal.GetType())
                        {
                            oldVal = Sqo.Utilities.Convertor.ChangeType(oldTags[key], newTags[key].GetType());
                        }
                        int c = ((IComparable)newTags[key]).CompareTo(oldVal);
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
#if ASYNC
        public async Task UpdateIndexesAsync(int oid, Dictionary<string, object> oldTags, Dictionary<string, object> newTags)
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
                            IBTree index = await this.GetIndexAsync(key, newTags[key].GetType()).LibAwait();
                            await index.RemoveOidAsync(oldTags[key], oid).LibAwait();
                            //add new value(updated)
                            await index.AddItemAsync(newTags[key], new int[] { oid }).LibAwait();
                            index.AllowPersistance(this.allowPersistence);
                            await index.PersistAsync().LibAwait();
                        }
                    }
                    else//tag is removed
                    {
                        IBTree index = await this.GetIndexAsync(key, oldTags[key].GetType()).LibAwait();
                        await index.RemoveOidAsync(oldTags[key], oid).LibAwait();
                        index.AllowPersistance(this.allowPersistence);
                        await index.PersistAsync().LibAwait();

                    }
                }
                if (newTags != null)
                {
                    foreach (string key in newTags.Keys)
                    {
                        if (!oldTags.ContainsKey(key))
                        {
                            IBTree index = await this.GetIndexAsync(key, newTags[key].GetType()).LibAwait();
                            await index.AddItemAsync(newTags[key], new int[] { oid }).LibAwait();
                            index.AllowPersistance(this.allowPersistence);
                            await index.PersistAsync().LibAwait();
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
                        IBTree index = await this.GetIndexAsync(key, newTags[key].GetType()).LibAwait();
                        await index.AddItemAsync(newTags[key], new int[] { oid }).LibAwait();
                        index.AllowPersistance(this.allowPersistence);
                        await index.PersistAsync().LibAwait();
                    }
                }
            }

        }
#endif
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
#if ASYNC
        public async Task UpdateIndexesAfterDeleteAsync(int oid, Dictionary<string, object> oldTags)
        {
            if (oldTags != null && oldTags.Count > 0)
            {
                foreach (string key in oldTags.Keys)
                {
                    IBTree index = await this.GetIndexAsync(key, oldTags[key].GetType()).LibAwait();
                    await index.RemoveOidAsync(oldTags[key], oid).LibAwait();
                    index.AllowPersistance(this.allowPersistence);
                    await index.PersistAsync().LibAwait();
                }
            }
        }
#endif
        

        internal void LoadOidsByIndex(Cryptonor.Queries.CryptonorQuery query, List<int> oids)
        {
            IBTree index = this.GetIndex(query.TagName, query.GetTagType());
            IndexQueryFinder.FindOids(index, query, oids);
           
        }
#if ASYNC 
        internal async Task LoadOidsByIndexAsync(Cryptonor.Queries.CryptonorQuery query, List<int> oids)
        {
            IBTree index = await this.GetIndexAsync(query.TagName, query.GetTagType()).LibAwait();
            await IndexQueryFinder.FindOidsAsync(index, query, oids).LibAwait();

        }
#endif
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
#if ASYNC
        internal async Task PersistAsync()
        {
            if (cache != null)
            {
                foreach (string keyIndex in cache.Keys)
                {
                    IBTree index = cache[keyIndex];
                    index.AllowPersistance(true);
                    if (index != null)
                    {
                        await index.PersistAsync().LibAwait();
                    }
                }

            }
        }
#endif
        private bool allowPersistence=true;
        public void AllowPersistence(bool allow)
        {
            this.allowPersistence = allow;
        }
       
    }
}
