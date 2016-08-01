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
        public bool LoadOidsByIndex(SqoTypeInfo ti, string fieldName, Where where, List<int> oids, LightningDB.LightningTransaction transaction)
        {
            string indexName = BuildIndexName(fieldName, ti);
            if (existingIndexes.ContainsKey(indexName))
            {
                IBTree index = new BTree(indexName, transaction);
                if (index != null && where.OperationType != OperationType.Contains && where.OperationType != OperationType.NotEqual && where.OperationType != OperationType.EndWith)
                {
                    Sqo.Meta.FieldSqoInfo fi = MetaHelper.FindField(ti.Fields, fieldName);
                    if (where.Value != null && where.Value.GetType() != fi.AttributeType)
                    {
                        where.Value = Convertor.ChangeType(where.Value, fi.AttributeType);
                    }
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
                    oidsFound = index.FindItemsStartsWith(where.Value, true, StringComparison.Ordinal);
                }
                if (oidsFound != null)
                {
                    oids.AddRange(oidsFound);

                }

            }

        }

        internal void BuildAllIndexes(List<SqoTypeInfo> typeInfos, LightningDB.LightningTransaction transaction)
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
                    this.DropIndex(indexStored, transaction);
                }
            }

        }


        internal void BuildIndexes(SqoTypeInfo ti, LightningDB.LightningTransaction transaction)
        {

            foreach (FieldSqoInfo f in ti.IndexedFields)
            {
                IBTree index = this.GetIndex(f, ti, transaction);
                existingIndexes.Add(index.IndexName, index.IndexName);
            }


        }


        public IBTree GetIndex(FieldSqoInfo finfo, SqoTypeInfo tinfo, LightningDB.LightningTransaction transaction)
        {

            string indexName = BuildIndexName(finfo.Name, tinfo);

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

        private void BuildIndex(FieldSqoInfo finfo, SqoTypeInfo tinfo, BTree index, LightningDB.LightningTransaction transaction)
        {
            this.FillIndex(finfo, tinfo, index, transaction);
            var db = transaction.OpenDatabase(sys_indexinfo, DatabaseOpenFlags.Create);
            byte[] key = ByteConverter.StringToByteArray(index.IndexName);
            transaction.Put(db, key, key);
            storedIndexes.Add(index.IndexName);
        }

        public void FillIndex(FieldSqoInfo finfo, SqoTypeInfo ti, IBTree index, LightningDB.LightningTransaction transaction)
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
                string indexName = BuildIndexName(fi.Name, ti);
                IBTree index = new BTree(indexName, transaction);

                index.DeleteItem(objInfo.AtInfo[fi], objInfo.Oid);

            }

        }

        public void UpdateIndexesAfterDelete(int oid, SqoTypeInfo ti, LightningTransaction transaction)
        {


            foreach (FieldSqoInfo fi in ti.IndexedFields)
            {
                string indexName = BuildIndexName(fi.Name, ti);
                IBTree index = new BTree(indexName, transaction);
                object indexedVal = this.siaqodb.LoadValue(oid, fi.Name, ti.Type, transaction);

                index.DeleteItem(indexedVal, oid);

            }


        }

        public void UpdateIndexes(ObjectInfo objInfo, SqoTypeInfo ti, Dictionary<string, object> oldValuesOfIndexedFields, LightningTransaction transaction)
        {

            foreach (FieldSqoInfo fi in ti.IndexedFields)
            {
                string indexName = BuildIndexName(fi.Name, ti);
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
                        index.AddItem(objInfo.AtInfo[fi], objInfo.Oid);

                    }
                }
                else//insert
                {
                    index.AddItem(objInfo.AtInfo[fi], objInfo.Oid);

                }


            }


        }

        public Dictionary<string, object> PrepareUpdateIndexes(ObjectInfo objInfo, SqoTypeInfo ti, LightningTransaction transaction)
        {

            Dictionary<string, object> oldValues = new Dictionary<string, object>();


            if (objInfo.Oid > 0 && objInfo.Oid <= ti.Header.numberOfRecords)
            {

                foreach (FieldSqoInfo fi in ti.IndexedFields)
                {
                    string indexName = BuildIndexName(fi.Name, ti);
                    IBTree index = new BTree(indexName, transaction);

                    oldValues[fi.Name] = siaqodb.LoadValue(objInfo.Oid, fi.Name, ti.Type, transaction);

                }

            }

            return oldValues;

        }

        public object GetValueForFutureUpdateIndex(int oid, string fieldName, SqoTypeInfo ti, LightningTransaction transaction)
        {

            if (oid > 0 && oid <= ti.Header.numberOfRecords)
            {
                string indexName = BuildIndexName(fieldName, ti);
                if (existingIndexes.ContainsKey(indexName))
                {
                    IBTree index = new BTree(indexName, transaction);
                    return siaqodb.LoadValue(oid, fieldName, ti.Type, transaction);

                }

            }

            return null;
        }

        public void UpdateIndexes(int oid, string fieldName, SqoTypeInfo ti, object oldValue, object newValue, LightningTransaction transaction)
        {

            string indexName = BuildIndexName(fieldName, ti);
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
                    index.AddItem(newValue, oid);

                }
            }
        }




        internal void DropIndexes(SqoTypeInfo ti, LightningTransaction transaction)
        {
            foreach (FieldSqoInfo fi in ti.IndexedFields)
            {
                string indexName = BuildIndexName(fi.Name, ti);

                DropIndex(indexName, transaction);
                existingIndexes.Remove(indexName);
            }


        }
        private void DropIndex(string indexName, LightningTransaction transaction)
        {
            IBTree index = new BTree(indexName, transaction);
            index.Drop();
            var db = transaction.OpenDatabase(sys_indexinfo, DatabaseOpenFlags.Create);

            byte[] val = transaction.Get(db, ByteConverter.StringToByteArray(indexName));
            if (val != null)
            {

                transaction.Delete(db, ByteConverter.StringToByteArray(indexName));
            }

        }
        private string BuildIndexName(string fieldName, SqoTypeInfo ti)
        {
            return string.Format("idxv2{0}{1}", fieldName, ti.GetDBName());
        }

    }
}
