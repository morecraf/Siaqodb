using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Queries;
using Sqo.Meta;
using System.Reflection;
using Sqo.Utilities;
using Sqo.MetaObjects;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo.Indexes
{
    class IndexManager
    {

        Siaqodb siaqodb;
        Cache.CacheForIndexes cacheIndexes;
#if UNITY3D
        DummyBtree dummy;
#endif
        public IndexManager(Siaqodb siaqodb)
        {
            this.siaqodb = siaqodb;
#if UNITY3D
            dummy = new DummyBtree(siaqodb);
#endif
        }
        public bool LoadOidsByIndex(SqoTypeInfo ti, string fieldName, Where where, List<int> oids)
        {
            if (cacheIndexes != null)
            {
                IBTree index = cacheIndexes.GetIndex(ti, fieldName);
                if (index != null && where.OperationType != OperationType.Contains && where.OperationType != OperationType.NotEqual && where.OperationType != OperationType.EndWith)
                {
                    this.LoadOidsByIndex(index, where, oids);
                    return true;
                }
            }
            return false;
        }
#if ASYNC
        public async Task<bool> LoadOidsByIndexAsync(SqoTypeInfo ti, string fieldName, Where where, List<int> oids)
        {
            if (cacheIndexes != null)
            {
                IBTree index = cacheIndexes.GetIndex(ti, fieldName);
                if (index != null && where.OperationType != OperationType.Contains && where.OperationType != OperationType.NotEqual && where.OperationType != OperationType.EndWith)
                {
                    await this.LoadOidsByIndexAsync(index, where, oids).ConfigureAwait(false);
                    return true;
                }
            }
            return false;
        }
#endif
       private void LoadOidsByIndex(IBTree index, Where where, List<int> oids)
       {
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
               List<int> oidsFound = index.FindItemsStartsWith(where.Value);
               if (oidsFound != null)
               {
                   oids.AddRange(oidsFound);

               }
              
           }

       }
#if ASYNC
       private async Task LoadOidsByIndexAsync(IBTree index, Where where, List<int> oids)
       {
           if (where.OperationType == OperationType.Equal)
           {
               int[] oidsFound = await index.FindItemAsync(where.Value).ConfigureAwait(false);
               if (oidsFound != null)
               {
                   oids.AddRange(oidsFound);

               }
           }

           else if (where.OperationType == OperationType.GreaterThan)
           {
               List<int> oidsFound = await index.FindItemsBiggerThanAsync(where.Value).ConfigureAwait(false);
               if (oidsFound != null)
               {
                   oidsFound.Reverse();
                   oids.AddRange(oidsFound);

               }

           }
           else if (where.OperationType == OperationType.GreaterThanOrEqual)
           {
               List<int> oidsFound = await index.FindItemsBiggerThanOrEqualAsync(where.Value).ConfigureAwait(false);
               if (oidsFound != null)
               {
                   oidsFound.Reverse();
                   oids.AddRange(oidsFound);


               }

           }
           else if (where.OperationType == OperationType.LessThan)
           {
               List<int> oidsFound = await index.FindItemsLessThanAsync(where.Value).ConfigureAwait(false);
               if (oidsFound != null)
               {
                   oids.AddRange(oidsFound);

               }
           }
           else if (where.OperationType == OperationType.LessThanOrEqual)
           {
               List<int> oidsFound = await index.FindItemsLessThanOrEqualAsync(where.Value).ConfigureAwait(false);
               if (oidsFound != null)
               {
                   oids.AddRange(oidsFound);

               }
           }
           else if (where.OperationType == OperationType.StartWith)
           {
               List<int> oidsFound = await index.FindItemsStartsWithAsync(where.Value).ConfigureAwait(false);
               if (oidsFound != null)
               {
                   oids.AddRange(oidsFound);

               }

           }

       }

#endif
       internal void BuildAllIndexes(List<SqoTypeInfo> typeInfos)
       {

           foreach (SqoTypeInfo ti in typeInfos)
           {
               BuildIndexes(ti);
           }

       }
#if ASYNC
       internal async Task BuildAllIndexesAsync(List<SqoTypeInfo> SqoTypeInfos)
       {

           foreach (SqoTypeInfo ti in SqoTypeInfos)
           {
               await BuildIndexesAsync(ti).ConfigureAwait(false);
           }

       }
#endif
       internal void BuildIndexes(SqoTypeInfo ti)
       {
           if (ti.Type == typeof(IndexInfo2) || ti.Type == typeof(RawdataInfo))
           {
               return;
           }
           Dictionary<FieldSqoInfo, IBTree> dict = new Dictionary<FieldSqoInfo, IBTree>();
           foreach (FieldSqoInfo f in ti.IndexedFields)
           {
               IBTree index = this.GetIndex(f, ti);
               dict.Add(f, index);
           }
           if (dict.Count > 0)
           {

               if (cacheIndexes == null)
               {
                   cacheIndexes = new Cache.CacheForIndexes();
               }
               cacheIndexes.Add(ti, dict);
           }


       }
#if ASYNC
       internal async Task BuildIndexesAsync(SqoTypeInfo ti)
       {
           if (ti.Type == typeof(IndexInfo2) || ti.Type == typeof(RawdataInfo))
           {
               return;
           }
           Dictionary<FieldSqoInfo, IBTree> dict = new Dictionary<FieldSqoInfo, IBTree>();
           foreach (FieldSqoInfo f in ti.IndexedFields)
           {
               IBTree index = await this.GetIndexAsync(f, ti).ConfigureAwait(false);
               dict.Add(f, index);
           }
           if (dict.Count > 0)
           {

               if (cacheIndexes == null)
               {
                   cacheIndexes = new Cache.CacheForIndexes();
               }
               cacheIndexes.Add(ti, dict);
           }


       }
#endif
       internal void ReBuildIndexesAfterCrash(SqoTypeInfo ti)
       {
           DropIndexes(ti,false);
           
           BuildIndexes(ti);
       }
#if ASYNC
       internal async Task ReBuildIndexesAfterCrashAsync(SqoTypeInfo ti)
       {
           await DropIndexesAsync(ti, false).ConfigureAwait(false);
           await BuildIndexesAsync(ti).ConfigureAwait(false);
       }
#endif
       internal void DropIndexes(SqoTypeInfo ti,bool claimFreeSpace)
       {
           if (cacheIndexes != null)
           {

               if (cacheIndexes.ContainsType(ti))
               {

                   Dictionary<FieldSqoInfo, IBTree> indexes = cacheIndexes.GetIndexes(ti);
                   foreach (FieldSqoInfo fi in indexes.Keys)
                   {
                       indexes[fi].Drop(claimFreeSpace);
                   }
                   storedIndexes = null;
                   cacheIndexes.RemoveType(ti);
               }
           }
       }
#if ASYNC
       internal async Task DropIndexesAsync(SqoTypeInfo ti, bool claimFreeSpace)
       {
           if (cacheIndexes != null)
           {

               if (cacheIndexes.ContainsType(ti))
               {

                   Dictionary<FieldSqoInfo, IBTree> indexes = cacheIndexes.GetIndexes(ti);
                   foreach (FieldSqoInfo fi in indexes.Keys)
                   {
                       await indexes[fi].DropAsync(claimFreeSpace).ConfigureAwait(false);
                   }
                   storedIndexes = null;
                   cacheIndexes.RemoveType(ti);
               }
           }
       }
#endif
       
       public IBTree GetIndex(string field, SqoTypeInfo tinfo)
       {
           return this.cacheIndexes.GetIndex(tinfo, field);
       }
        
       public IBTree GetIndex(FieldSqoInfo finfo, SqoTypeInfo tinfo)
       {
           Type t = typeof(BTree<>).MakeGenericType(finfo.AttributeType);
           ConstructorInfo ctor = t.GetConstructor(new Type[] { typeof(Siaqodb) });
           IBTree index = (IBTree)ctor.Invoke(new object[] { this.siaqodb });
           IndexInfo2 indexInfo = null;
           string indexName = finfo.Name + tinfo.TypeName;
           foreach (IndexInfo2 ii in StoredIndexes)
           {
               if (indexName.StartsWith(ii.IndexName) || ii.IndexName.StartsWith(indexName))
               {
                   indexInfo = ii;
                   break;
               }
           }
           bool indexExists = false;
           if (indexInfo == null)
           {
               indexInfo = this.BuildIndex(finfo, tinfo, index);
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
           Type nodeType = typeof(BTreeNode<>).MakeGenericType(finfo.AttributeType);
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
       public async Task<IBTree> GetIndexAsync(FieldSqoInfo finfo, SqoTypeInfo tinfo)
       {
           Type t = typeof(BTree<>).MakeGenericType(finfo.AttributeType);
           ConstructorInfo ctor = t.GetConstructor(new Type[] { typeof(Siaqodb) });
           IBTree index = (IBTree)ctor.Invoke(new object[] { this.siaqodb });
           IndexInfo2 indexInfo = null;
           string indexName = finfo.Name + tinfo.TypeName;
           IList<IndexInfo2> stIndexes = await this.GetStoredIndexesAsync().ConfigureAwait(false);
           foreach (IndexInfo2 ii in stIndexes)
           {
               if (ii.IndexName == indexName)
               {
                   indexInfo = ii;
                   break;
               }
           }
           bool indexExists = false;
           if (indexInfo == null)
           {
               indexInfo = await this.BuildIndexAsync(finfo, tinfo, index).ConfigureAwait(false);
           }
           else
           {
               indexExists = true;
           }
           index.SetIndexInfo(indexInfo);
           Type nodeType = typeof(BTreeNode<>).MakeGenericType(finfo.AttributeType);
           if (indexInfo.RootOID > 0 && indexExists)
           {
               object rootP = await siaqodb.LoadObjectByOIDAsync(nodeType, indexInfo.RootOID).ConfigureAwait(false);
               if (rootP != null)
               {
                   index.SetRoot(rootP);
               }
           }

           return index;
       }
#endif
       private IndexInfo2 BuildIndex(FieldSqoInfo finfo, SqoTypeInfo tinfo, IBTree index)
       {
           this.FillIndex(finfo, tinfo, index);
           IndexInfo2 ii = new IndexInfo2();
           ii.IndexName = finfo.Name + tinfo.TypeName;
           ii.RootOID = index.GetRootOid();
           siaqodb.StoreObject(ii);
           storedIndexes.Add(ii);
           return ii;
       }
#if ASYNC
       private async Task<IndexInfo2> BuildIndexAsync(FieldSqoInfo finfo, SqoTypeInfo tinfo, IBTree index)
       {
           await this.FillIndexAsync(finfo, tinfo, index).ConfigureAwait(false);
           IndexInfo2 ii = new IndexInfo2();
           ii.IndexName = finfo.Name + tinfo.TypeName;
           ii.RootOID = index.GetRootOid();
           await siaqodb.StoreObjectAsync(ii).ConfigureAwait(false);
           storedIndexes.Add(ii);
           return ii;
       }
#endif
       public void FillIndex(FieldSqoInfo finfo, SqoTypeInfo ti, IBTree index)
       {
           int nrRecords = ti.Header.numberOfRecords;
           for (int i = 0; i < nrRecords; i++)
           {


               int oid = i + 1;
               if (siaqodb.IsObjectDeleted(oid, ti))
               {
                   continue;
               }
               index.AddItem(siaqodb.LoadValue(oid,finfo.Name,ti.Type), new int[] { oid });

           }
          
       }
#if ASYNC
       public async Task FillIndexAsync(FieldSqoInfo finfo, SqoTypeInfo ti, IBTree index)
       {
           int nrRecords = ti.Header.numberOfRecords;
           for (int i = 0; i < nrRecords; i++)
           {


               int oid = i + 1;
               if (await siaqodb.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
               {
                   continue;
               }
               await index.AddItemAsync(await siaqodb.LoadValueAsync(oid, finfo.Name, ti.Type).ConfigureAwait(false), new int[] { oid }).ConfigureAwait(false);

           }

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
               storedIndexes = await siaqodb.LoadAllAsync<IndexInfo2>().ConfigureAwait(false);
           }
           return storedIndexes;

       }
#endif
       public void UpdateIndexesAfterDelete(ObjectInfo objInfo, SqoTypeInfo ti)
       {
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in objInfo.AtInfo.Keys)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {

                           // remove oid=> a node can remain with ZERO oids but is np
                           index.RemoveOid(objInfo.AtInfo[fi], objInfo.Oid);
                           index.Persist();

                       }
                   }

               }
           }
       }
#if ASYNC
       public async Task UpdateIndexesAfterDeleteAsync(ObjectInfo objInfo, SqoTypeInfo ti)
       {
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in objInfo.AtInfo.Keys)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {

                           // remove oid=> a node can remain with ZERO oids but is np
                           await index.RemoveOidAsync(objInfo.AtInfo[fi], objInfo.Oid).ConfigureAwait(false);
                           await index.PersistAsync().ConfigureAwait(false);

                       }
                   }

               }
           }
       }
#endif
       public void UpdateIndexesAfterDelete(int oid, SqoTypeInfo ti)
       {
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in ti.Fields)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {

                           object indexedVal = this.siaqodb.LoadValue(oid, fi.Name, ti.Type);
                           // remove oid=> a node can remain with ZERO oids but is np
                           index.RemoveOid(indexedVal, oid);
                           index.Persist();      

                       }
                   }

               }
           }
       }
#if ASYNC
       public async Task UpdateIndexesAfterDeleteAsync(int oid, SqoTypeInfo ti)
       {
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in ti.Fields)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {

                           object indexedVal = await this.siaqodb.LoadValueAsync(oid, fi.Name, ti.Type).ConfigureAwait(false);
                           // remove oid=> a node can remain with ZERO oids but is np
                           await index.RemoveOidAsync(indexedVal, oid).ConfigureAwait(false);
                           await index.PersistAsync().ConfigureAwait(false);

                       }
                   }

               }
           }
       }
#endif
       public void UpdateIndexes(ObjectInfo objInfo, SqoTypeInfo ti, Dictionary<string, object> oldValuesOfIndexedFields)
       {
           if (ti.Type == typeof(IndexInfo2) || ti.Type == typeof(RawdataInfo))
           {
               return;
           }
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in objInfo.AtInfo.Keys)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {
                           if (oldValuesOfIndexedFields.ContainsKey(fi.Name))//update occur
                           {
                               int c = 0;
                               if (objInfo.AtInfo[fi] == null || oldValuesOfIndexedFields[fi.Name] == null)
                               {
                                   if (objInfo.AtInfo[fi] == oldValuesOfIndexedFields[fi.Name])
                                       c = 0;
                                   else if (objInfo.AtInfo[fi] == null)
                                       c = -1;
                                   else if (oldValuesOfIndexedFields[fi.Name] == null)
                                       c = 1;
                               }
                               else
                               {
                                   Type fieldType = objInfo.AtInfo[fi].GetType();
                                   object currentFieldVal = objInfo.AtInfo[fi];
                                   if (fieldType.IsEnum())
                                   {
                                       Type enumType = Enum.GetUnderlyingType(fieldType);

                                       currentFieldVal = Convertor.ChangeType(objInfo.AtInfo[fi], enumType);
                                   }
                                   c = ((IComparable)currentFieldVal).CompareTo(oldValuesOfIndexedFields[fi.Name]);
                               }
                               if (c == 0)//do nothing because values are equal
                               {

                               }
                               else
                               {
                                   //first remove oid=> a node can remain with ZERO oids but is np
                                   index.RemoveOid(oldValuesOfIndexedFields[fi.Name], objInfo.Oid);
                                   //add new value(updated)
                                   index.AddItem(objInfo.AtInfo[fi],new int[]{ objInfo.Oid});
                                   index.Persist();
                               }
                           }
                           else//insert
                           {
                               index.AddItem(objInfo.AtInfo[fi], new int[] { objInfo.Oid });
                               index.Persist();
                           }

                       }
                   }

               }
           }
       }
#if ASYNC
       public async Task UpdateIndexesAsync(ObjectInfo objInfo, SqoTypeInfo ti, Dictionary<string, object> oldValuesOfIndexedFields)
       {
           if (ti.Type == typeof(IndexInfo2) || ti.Type == typeof(RawdataInfo))
           {
               return;
           }
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in objInfo.AtInfo.Keys)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {
                           if (oldValuesOfIndexedFields.ContainsKey(fi.Name))//update occur
                           {
                               int c = 0;
                               if (objInfo.AtInfo[fi] == null || oldValuesOfIndexedFields[fi.Name] == null)
                               {
                                   if (objInfo.AtInfo[fi] == oldValuesOfIndexedFields[fi.Name])
                                       c = 0;
                                   else if (objInfo.AtInfo[fi] == null)
                                       c = -1;
                                   else if (oldValuesOfIndexedFields[fi.Name] == null)
                                       c = 1;
                               }
                               else
                               {
                                   Type fieldType = objInfo.AtInfo[fi].GetType();
                                   object currentFieldVal = objInfo.AtInfo[fi];
                                   if (fieldType.IsEnum())
                                   {
                                       Type enumType = Enum.GetUnderlyingType(fieldType);

                                       currentFieldVal = Convertor.ChangeType(objInfo.AtInfo[fi], enumType);
                                   }
                                   c = ((IComparable)currentFieldVal).CompareTo(oldValuesOfIndexedFields[fi.Name]);
                               }
                               if (c == 0)//do nothing because values are equal
                               {

                               }
                               else
                               {
                                   //first remove oid=> a node can remain with ZERO oids but is np
                                   await index.RemoveOidAsync(oldValuesOfIndexedFields[fi.Name], objInfo.Oid).ConfigureAwait(false);
                                   //add new value(updated)
                                   await index.AddItemAsync(objInfo.AtInfo[fi], new int[] { objInfo.Oid }).ConfigureAwait(false);
                                   await index.PersistAsync().ConfigureAwait(false);
                               }
                           }
                           else//insert
                           {
                               await index.AddItemAsync(objInfo.AtInfo[fi], new int[] { objInfo.Oid }).ConfigureAwait(false);
                               await index.PersistAsync().ConfigureAwait(false);
                           }

                       }
                   }

               }
           }
       }
#endif
       public Dictionary<string, object> PrepareUpdateIndexes(ObjectInfo objInfo, SqoTypeInfo ti)
       {
           
           Dictionary<string, object> oldValues = new Dictionary<string, object>();
           if (ti.Type == typeof(IndexInfo2)||ti.Type==typeof(RawdataInfo))
           {
               return oldValues;
           }
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {
                   if (objInfo.Oid > 0 && objInfo.Oid <= ti.Header.numberOfRecords)
                   {
                       foreach (FieldSqoInfo fi in objInfo.AtInfo.Keys)
                       {
                           IBTree index = cacheIndexes.GetIndex(ti, fi);
                           if (index != null)
                           {
                               oldValues[fi.Name] = siaqodb.LoadValue( objInfo.Oid, fi.Name, ti.Type);
                           }
                       }
                   }
               }
           }
           return oldValues;

       }
#if ASYNC
       public async Task<Dictionary<string, object>> PrepareUpdateIndexesAsync(ObjectInfo objInfo, SqoTypeInfo ti)
       {

           Dictionary<string, object> oldValues = new Dictionary<string, object>();
           if (ti.Type == typeof(IndexInfo2) || ti.Type == typeof(RawdataInfo))
           {
               return oldValues;
           }
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {
                   if (objInfo.Oid > 0 && objInfo.Oid <= ti.Header.numberOfRecords)
                   {
                       foreach (FieldSqoInfo fi in objInfo.AtInfo.Keys)
                       {
                           IBTree index = cacheIndexes.GetIndex(ti, fi);
                           if (index != null)
                           {
                               oldValues[fi.Name] = await siaqodb.LoadValueAsync(objInfo.Oid, fi.Name, ti.Type).ConfigureAwait(false);
                           }
                       }
                   }
               }
           }
           return oldValues;

       }
#endif

       internal void Close()
       {
           this.cacheIndexes = null;
       }

       internal void Persist(SqoTypeInfo ti)
       {
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in ti.Fields)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {
                           index.Persist();
                       }
                   }
               }
           }
       }
#if ASYNC
       internal async Task PersistAsync(SqoTypeInfo ti)
       {
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in ti.Fields)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {
                           await index.PersistAsync().ConfigureAwait(false);
                       }
                   }
               }
           }
       }
#endif

       internal void PutIndexPersistenceOnOff(SqoTypeInfo ti, bool on)
       {
           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {

                   foreach (FieldSqoInfo fi in ti.Fields)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fi);
                       if (index != null)
                       {
                           index.AllowPersistance(on);
                       }
                   }
               }
           }
       }

       internal void DeleteAllIndexInfo()
       {
           cacheIndexes = null;
           storedIndexes = null;
       }
    }
}
