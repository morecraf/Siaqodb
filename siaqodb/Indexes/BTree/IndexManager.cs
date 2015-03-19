using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Queries;
using Sqo.Meta;
using System.Reflection;
using Sqo.Utilities;
using Sqo.MetaObjects;
using Sqo.Exceptions;
using LightningDB;
using Sqo.Core;
#if ASYNC
using System.Threading.Tasks;

#endif
namespace Sqo.Indexes
{
    class IndexManager
    {

        Siaqodb siaqodb;
        Dictionary<string, string> existingIndexes = new Dictionary<string, string>();
        const string sys_indexinfo = "sys_indexinfo";
        public IndexManager(Siaqodb siaqodb)
        {
            this.siaqodb = siaqodb;
        }
        public bool LoadOidsByIndex(SqoTypeInfo ti, string fieldName, Where where, List<int> oids,LightningDB.LightningTransaction transaction)
        {
            string indexName = fieldName + ti.TypeName;
            if(existingIndexes.ContainsKey(indexName))
            {
                IBTree index = new BTree(indexName, transaction);
                if (index != null && where.OperationType != OperationType.Contains && where.OperationType != OperationType.NotEqual && where.OperationType != OperationType.EndWith)
                {

                    this.LoadOidsByIndex(index, where, oids);

                    return true;
                }
            }
            return false;
        }

       private void LoadOidsByIndex(IBTree index, Where where, List<int> oids)
       {
           if (where.OperationType == OperationType.Equal)
           {
               var oidsFound = index.FindItem(where.Value);
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
                  oidsFound = index.FindItemsStartsWith(where.Value,true,StringComparison.Ordinal);
               }
               if (oidsFound != null)
               {
                   oids.AddRange(oidsFound);

               }
              
           }

       }

       internal void BuildAllIndexes(List<SqoTypeInfo> typeInfos,LightningDB.LightningTransaction transaction)
       {

           foreach (SqoTypeInfo ti in typeInfos)
           {
               BuildIndexes(ti, transaction);
           }
           //if an index gets removed we have to clean also the DB with index headers
           foreach (string indexStored in GetStoredIndexes(transaction))
           {
               if (!existingIndexes.ContainsKey(indexStored))
               {
                   this.DropIndex(indexStored,transaction);
               }
           }

       }

       private void DropIndex(string indexName, LightningTransaction transaction)
       {
           var db = transaction.OpenDatabase(sys_indexinfo, DatabaseOpenFlags.Create);
           byte[] key = ByteConverter.StringToByteArray(indexName);
           transaction.Delete(db, key);
           
          
       }
       internal void BuildIndexes(SqoTypeInfo ti,LightningDB.LightningTransaction transaction)
       {
           if (ti.Type == typeof(RawdataInfo))
           {
               return;
           }   
           foreach (FieldSqoInfo f in ti.IndexedFields)
           {
               IBTree index = this.GetIndex(f, ti, transaction);
               existingIndexes.Add(index.IndexName, index.IndexName);
           }
           

       }

       /*internal void DropIndexes(SqoTypeInfo ti,bool claimFreeSpace)
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
        */

       public IBTree GetIndex(FieldSqoInfo finfo, SqoTypeInfo tinfo, LightningDB.LightningTransaction transaction)
       {

           string indexName = finfo.Name + tinfo.TypeName;
           BTree index = new BTree(indexName, transaction);
           bool indexExists = false;

           foreach (string ii in GetStoredIndexes(transaction))
           {
               if (indexName.StartsWith(ii) || ii.StartsWith(indexName))
               {
                   indexExists = true;
                   break;
               }
           }

           if (!indexExists)
           {
               this.BuildIndex(finfo, tinfo, index, transaction);
           }

           return index;
       }

       private void BuildIndex(FieldSqoInfo finfo, SqoTypeInfo tinfo,BTree index, LightningDB.LightningTransaction transaction)
       {
           this.FillIndex(finfo, tinfo, index,transaction);
           var db = transaction.OpenDatabase(sys_indexinfo, DatabaseOpenFlags.Create);
           byte[] key = ByteConverter.StringToByteArray(index.IndexName);
           transaction.Put(db, key, key);
           storedIndexes.Add(index.IndexName);
       }

       public void FillIndex(FieldSqoInfo finfo, SqoTypeInfo ti,IBTree index, LightningDB.LightningTransaction transaction)
       {
           var all = siaqodb.GetAllValues(ti, finfo, transaction);
           foreach (var item in all)
           {
               index.AddItem(item.Value, item.Name);
           }

       }

       private IList<string> storedIndexes;
       public IList<string> GetStoredIndexes(LightningDB.LightningTransaction transaction)
       {

           if (storedIndexes == null)
           {
               storedIndexes = new List<string>();

               var db = transaction.OpenDatabase(sys_indexinfo, DatabaseOpenFlags.Create);

               using (var cursor = transaction.CreateCursor(db))
               {
                   var current = cursor.MoveNext();

                   while (current.HasValue)
                   {

                       byte[] indexNameBytes = current.Value.Key;
                       string indexName = ByteConverter.ByteArrayToString(indexNameBytes);
                       storedIndexes.Add(indexName);

                       current = cursor.MoveNext();
                   }
               }


           }
           return storedIndexes;

       }

       public void UpdateIndexesAfterDelete(ObjectInfo objInfo, SqoTypeInfo ti, LightningTransaction transaction)
       {

           foreach (FieldSqoInfo fi in ti.IndexedFields)
           {
               string indexName = fi.Name + ti.TypeName;
               IBTree index = new BTree(indexName, transaction);
              
               index.DeleteItem(objInfo.AtInfo[fi], objInfo.Oid);

           }

       }

       public void UpdateIndexesAfterDelete(int oid, SqoTypeInfo ti, LightningTransaction transaction)
       {


           foreach (FieldSqoInfo fi in ti.IndexedFields)
           {
               string indexName = fi.Name + ti.TypeName;
               IBTree index = new BTree(indexName, transaction);
               object indexedVal = this.siaqodb.LoadValue(oid, fi.Name, ti.Type, transaction);
              
               index.DeleteItem(indexedVal, oid);

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
       public void UpdateIndexes(ObjectInfo objInfo, SqoTypeInfo ti, Dictionary<string, object> oldValuesOfIndexedFields, LightningTransaction transaction)
       {
           if (ti.Type == typeof(RawdataInfo))
           {
               return;
           }


           foreach (FieldSqoInfo fi in ti.IndexedFields)
           {
               string indexName = fi.Name + ti.TypeName;
               IBTree index = new BTree(indexName, transaction);

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
                       index.DeleteItem(oldValuesOfIndexedFields[fi.Name], objInfo.Oid);
                       //add new value(updated)
                       index.AddItem(objInfo.AtInfo[fi],  objInfo.Oid );
                       
                   }
               }
               else//insert
               {
                   index.AddItem(objInfo.AtInfo[fi], objInfo.Oid );
                  
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
       public Dictionary<string, object> PrepareUpdateIndexes(ObjectInfo objInfo, SqoTypeInfo ti,LightningTransaction transaction)
       {
           
           Dictionary<string, object> oldValues = new Dictionary<string, object>();
           if (ti.Type == typeof(RawdataInfo))
           {
               return oldValues;
           }

           if (objInfo.Oid > 0 && objInfo.Oid <= ti.Header.numberOfRecords)
           {

               foreach (FieldSqoInfo fi in ti.IndexedFields)
               {
                   string indexName = fi.Name + ti.TypeName;
                   IBTree index = new BTree(indexName, transaction);

                   oldValues[fi.Name] = siaqodb.LoadValue(objInfo.Oid, fi.Name, ti.Type,transaction);

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
       public object GetValueForFutureUpdateIndex(int oid, string fieldName, SqoTypeInfo ti,LightningTransaction transaction)
       {

           if (oid > 0 && oid <= ti.Header.numberOfRecords)
           {
               string indexName = fieldName + ti.TypeName;
               if (existingIndexes.ContainsKey(indexName))
               {
                   IBTree index = new BTree(indexName, transaction);
                   return siaqodb.LoadValue(oid, fieldName, ti.Type,transaction);

               }

           }

           return null;
       }
#if ASYNC
       public async Task<object> GetValueForFutureUpdateIndexAsync(int oid, string fieldName, SqoTypeInfo ti)
       {

           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {
                   if (oid > 0 && oid <= ti.Header.numberOfRecords)
                   {
                       IBTree index = cacheIndexes.GetIndex(ti, fieldName);
                       if (index != null)
                       {
                           return await siaqodb.LoadValueAsync(oid, fieldName, ti.Type);
                       }

                   }
               }
           }

           return null;
       }
#endif
       public void UpdateIndexes(int oid, string fieldName, SqoTypeInfo ti, object oldValue, object newValue,LightningTransaction transaction)
       {

           string indexName = fieldName + ti.TypeName;
           if (existingIndexes.ContainsKey(indexName))
           {
               IBTree index = new BTree(indexName, transaction);
               int c = 0;
               if (newValue == null || oldValue == null)
               {
                   if (newValue == oldValue)
                       c = 0;
                   else if (newValue == null)
                       c = -1;
                   else if (oldValue == null)
                       c = 1;
               }
               else
               {
                   Type fieldType = newValue.GetType();
                   object currentFieldVal = newValue;
                   if (fieldType.IsEnum())
                   {
                       Type enumType = Enum.GetUnderlyingType(fieldType);

                       currentFieldVal = Convertor.ChangeType(newValue, enumType);
                   }
                   c = ((IComparable)currentFieldVal).CompareTo(oldValue);
               }
               if (c == 0)//do nothing because values are equal
               {

               }
               else
               {
                   //first remove oid=> a node can remain with ZERO oids but is np
                   index.DeleteItem(oldValue, oid);
                   //add new value(updated)
                   index.AddItem(newValue,  oid );
                   
               }
           }
       }
#if ASYNC
       public async Task UpdateIndexesAsync(int oid, string fieldName, SqoTypeInfo ti, object oldValue, object newValue)
       {

           if (cacheIndexes != null)
           {
               if (cacheIndexes.ContainsType(ti))
               {
                   IBTree index = cacheIndexes.GetIndex(ti, fieldName);
                   if (index != null)
                   {

                       int c = 0;
                       if (newValue == null || oldValue == null)
                       {
                           if (newValue == oldValue)
                               c = 0;
                           else if (newValue == null)
                               c = -1;
                           else if (oldValue == null)
                               c = 1;
                       }
                       else
                       {
                           Type fieldType = newValue.GetType();
                           object currentFieldVal = newValue;
                           if (fieldType.IsEnum())
                           {
                               Type enumType = Enum.GetUnderlyingType(fieldType);

                               currentFieldVal = Convertor.ChangeType(newValue, enumType);
                           }
                           c = ((IComparable)currentFieldVal).CompareTo(oldValue);
                       }
                       if (c == 0)//do nothing because values are equal
                       {

                       }
                       else
                       {
                           //first remove oid=> a node can remain with ZERO oids but is np
                           await index.RemoveOidAsync(oldValue, oid);
                           //add new value(updated)
                           await index.AddItemAsync(newValue, new int[] { oid });
                           await index.PersistAsync();
                       }
                   }

               }
           }

       }
#endif
      

    }
}
