using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Sqo.Meta;
using System.Diagnostics;
using Sqo.Core;
using System.IO;
using Sqo.Queries;
using System.Reflection;
using Sqo.Exceptions;
using Sqo.Utilities;
using Sqo.Indexes;
using Sqo.Transactions;
using Sqo.Cache;
using System.Linq;
using LightningDB;

#if SILVERLIGHT
	using System.IO.IsolatedStorage;
#endif
namespace Sqo
{
    partial class StorageEngine
    {
        internal int SaveObject(object oi, SqoTypeInfo ti)
        {
           
            return this.SaveObject(oi, ti,null);

        }
        internal int SaveObject(object oi, SqoTypeInfo ti,LightningTransaction transaction)
        {
            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(oi, ti, metaCache);

            return this.SaveObject(oi, ti, objInfo,transaction);

        }

        internal int SaveObject(object oi, SqoTypeInfo ti, ObjectInfo objInfo, LightningTransaction transaction)
        {

            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedSaveComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedSaveComplexObject);
            //TODO LMDB- pass transaction
            CheckConstraints(objInfo, ti);

            CheckForConcurency(oi, objInfo, ti, serializer, false, transaction);

            string dbName = GetFileByType(ti);
            var db = transaction.OpenDatabase(dbName, DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);

            Dictionary<string, object> oldValuesOfIndexedFields = this.indexManager.PrepareUpdateIndexes(objInfo, ti, transaction);


            byte[] objBytes = serializer.SerializeObject(objInfo, this.rawSerializer, transaction);

            byte[] key = ByteConverter.IntToByteArray(objInfo.Oid);
            transaction.Put(db, key, objBytes);

            if (objInfo.Inserted)
            {
                this.SaveType(objInfo.SqoTypeInfo, transaction);
            }
            metaCache.SetOIDToObject(oi, objInfo.Oid, ti);
            this.indexManager.UpdateIndexes(objInfo, ti, oldValuesOfIndexedFields, transaction);




            return objInfo.Oid;


        }
        
        internal void SaveObjectPartially(object obj, SqoTypeInfo ti, string[] properties)
        {
           var transaction=transactionManager.GetActiveTransaction();
            {

                foreach (string path in properties)
                {
                    string[] arrayPath = path.Split('.');

                    PropertyInfo property;
                    Type type = ti.Type;
                    object objOfProp = obj;
                    SqoTypeInfo tiOfProp = ti;
                    int oid = -1;
                    string backingField = null;
                    foreach (var include in arrayPath)
                    {
                        if ((property = type.GetProperty(include)) == null)
                        {
                            throw new Sqo.Exceptions.SiaqodbException("Property:" + include + " does not belong to Type:" + type.FullName);

                        }
                        backingField = ExternalMetaHelper.GetBackingField(property);

                        tiOfProp = this.GetSqoTypeInfoSoft(type);

                        ATuple<int, object> val = MetaExtractor.GetPartialObjectInfo(objOfProp, tiOfProp, backingField, metaCache);
                        objOfProp = val.Value;
                        oid = val.Name;
                        if (oid == 0)
                        {
                            throw new Sqo.Exceptions.SiaqodbException("Only updates are allowed through this method.");
                        }
                        type = property.PropertyType;

                    }

                    object oldPropVal = indexManager.GetValueForFutureUpdateIndex(oid, backingField, tiOfProp, transaction);

                    this.SaveValue(oid, backingField, tiOfProp, objOfProp,transaction);



                    indexManager.UpdateIndexes(oid, backingField, tiOfProp, oldPropVal, objOfProp, transaction);
                }
                

            }
        }

        internal bool SaveValue(int oid, string field, SqoTypeInfo ti, object value,LightningTransaction transaction)
        {
            if (field == "OID")
            {
                throw new SiaqodbException("OID cannot be saved from client code!");
            }
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedSaveComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedSaveComplexObject);

            if (oid > 0 && oid <= ti.Header.numberOfRecords)
            {

                var valuesToSave = serializer.SaveFieldValue(oid, field, ti, value, this.rawSerializer, transaction);
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);

                byte[] key = ByteConverter.IntToByteArray(oid);

                byte[] objBytes = transaction.Get(db, key);
                if (objBytes == null)
                    return false;

                Array.Copy(valuesToSave.Value, 0, objBytes, valuesToSave.Name, valuesToSave.Value.Length);
                transaction.Put(db, key, objBytes);
                return true;


            }
            else
                return false;

        }

        internal int InsertObjectByMeta(SqoTypeInfo tinf)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(tinf), useElevatedTrust);
           var transaction=transactionManager.GetActiveTransaction();
            {
                ATuple<int, byte[]> emptyObj = serializer.InsertEmptyObject(tinf);
                int oid=emptyObj.Name;
                var db = transaction.OpenDatabase(GetFileByType(tinf), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    byte[] key = ByteConverter.IntToByteArray(oid);
                    transaction.Put(db, key, emptyObj.Value);



                    this.SaveType(tinf, transaction);
                    
                }
                return oid;
            }
        }
        internal bool UpdateObjectBy(string[] fieldNames, object obj, SqoTypeInfo ti, LightningTransaction transact)
        {

            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti, metaCache);
            int i = 0;
            ICriteria wPrev = null;

            foreach (string fieldName in fieldNames)
            {
                FieldSqoInfo fi = MetaHelper.FindField(ti.Fields, fieldName);
                if (fi == null)
                {
                    throw new SiaqodbException("Field:" + fieldName + " was not found as member of Type:" + ti.TypeName);
                }

                Where w = new Where(fieldName, OperationType.Equal, objInfo.AtInfo[fi]);
                w.StorageEngine = this;
                w.ParentSqoTypeInfo = ti;
                w.ParentType.Add(w.ParentSqoTypeInfo.Type);

                if (i > 0)
                {
                    And and = new And();
                    and.Add(w, wPrev);

                    wPrev = and;
                }
                else
                {
                    wPrev = w;
                }
                i++;
            }

            List<int> oids = wPrev.GetOIDs();
            if (oids.Count > 1)
            {
                throw new SiaqodbException("In database exists more than one object with value of fields specified");
            }
            else if (oids.Count == 1)
            {
                objInfo.Oid = oids[0];

                this.SaveObject(obj, ti, objInfo, transact);

                return true;
            }
            else
            {
                return false;
            }


        }

        void serializer_NeedSaveComplexObject(object sender, ComplexObjectEventArgs e)
        {
            if (e.JustSetOID)
            {
                metaCache.SetOIDToObject(e.ObjInfo.BackendObject, e.ObjInfo.Oid, e.ObjInfo.SqoTypeInfo);
            }
            else
            {
                this.OnNeedSaveComplexObject(e);
            }
        }

        private void CheckConstraints(ObjectInfo objInfo, SqoTypeInfo ti)
        {
            if (objInfo.Oid <= 0) //if insert
            {
                foreach (FieldSqoInfo fi in ti.UniqueFields)
                {
                    Where w = new Where(fi.Name, OperationType.Equal, objInfo.AtInfo[fi]);
                    List<int> oids = this.LoadFilteredOids(w, ti);
                    if (oids.Count > 0)
                    {
                        throw new UniqueConstraintException("Field:" + fi.Name + " of object with OID=" + objInfo.Oid.ToString() + " of Type:" + ti.TypeName + "  has UniqueConstraint, duplicates not allowed!");
                    }
                }
            }
            else //if update
            {
                foreach (FieldSqoInfo fi in ti.UniqueFields)
                {
                    Where w = new Where(fi.Name, OperationType.Equal, objInfo.AtInfo[fi]);
                    List<int> oids = this.LoadFilteredOids(w, ti);
                    if (oids.Count > 0)
                    {
                        if (oids.Contains(objInfo.Oid) && oids.Count == 1)//is current one
                        {
                            continue;
                        }
                        else
                        {
                            throw new UniqueConstraintException("Field:" + fi.Name + " of object with OID=" + objInfo.Oid.ToString() + " of Type:" + ti.TypeName + "  has UniqueConstraint, duplicates not allowed!");
                        }
                    }
                }
            }

        }

        private bool _deletingNested = false;
        private List<string> _cachedObjectTypesForDelete;
        /// <summary>
        /// Get a list of all types names without the comma
        /// </summary>
        /// <returns></returns>
        public List<string> GetListOfAllTypeNames()
        {
            if (_cachedObjectTypesForDelete == null || !_deletingNested)
            {
                List<string> tiList = this.LoadAllTypesForObjectManager().Select(x => x.TypeName).ToList();
                _cachedObjectTypesForDelete = new List<string>();
                foreach (var item in tiList)
                {
                    _cachedObjectTypesForDelete.Add(item.Substring(0, item.IndexOf(",")));
                }

            }

            return _cachedObjectTypesForDelete;
        }
        
        internal void DeleteObject(object obj, SqoTypeInfo ti,LightningTransaction transaction, bool delete_nested)
        {
            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti,metaCache);

            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            List<object> _deleteNestedCache = new List<object>();
            lock (_syncRoot)
            {

                CheckForConcurency(obj, objInfo, ti, serializer, true, transaction);

                this.MarkObjectAsDelete(serializer, objInfo.Oid, ti, transaction);

                this.indexManager.UpdateIndexesAfterDelete(objInfo, ti, transaction);

                metaCache.SetOIDToObject(obj, -1, ti);

            }
        }

        
        internal int DeleteObjectBy(string[] fieldNames, object obj, SqoTypeInfo ti, LightningTransaction transaction)
        {
            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti,metaCache);
            int i = 0;
            ICriteria wPrev = null;

            foreach (string fieldName in fieldNames)
            {
                FieldSqoInfo fi = MetaHelper.FindField(ti.Fields, fieldName);
                if (fi == null)
                {
                    throw new SiaqodbException("Field:" + fieldName + " was not found as member of Type:" + ti.TypeName);
                }

                Where w = new Where(fieldName, OperationType.Equal, objInfo.AtInfo[fi]);
                w.StorageEngine = this;
                w.ParentSqoTypeInfo = ti;
                w.ParentType.Add(w.ParentSqoTypeInfo.Type);
                if (i > 0)
                {
                    And and = new And();
                    and.Add(w, wPrev);

                    wPrev = and;
                }
                else
                {
                    wPrev = w;
                }
                i++;
            }

            List<int> oids = wPrev.GetOIDs();


            if (oids.Count > 1)
            {
                throw new SiaqodbException("In database exists more than one object with value of fields specified");
            }
            else if (oids.Count == 1)
            {
                objInfo.Oid = oids[0];
                //obj.OID = oids[0];
                metaCache.SetOIDToObject(obj, oids[0], ti);

                ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
                this.DeleteObject(obj, ti, transaction, false);
                
                return oids[0];
            }
            else
            {
                return -1;
            }
        }

        internal List<int> DeleteObjectBy(SqoTypeInfo ti, Dictionary<string, object> criteria)
        {
            int i = 0;
            ICriteria wPrev = null;

            foreach (string fieldName in criteria.Keys)
            {
                FieldSqoInfo fi = MetaHelper.FindField(ti.Fields, fieldName);
                if (fi == null)
                {
                    throw new SiaqodbException("Field:" + fieldName + " was not found as member of Type:" + ti.TypeName);
                }

                Where w = new Where(fieldName, OperationType.Equal, criteria[fieldName]);
                w.StorageEngine = this;
                w.ParentSqoTypeInfo = ti;
                w.ParentType.Add(w.ParentSqoTypeInfo.Type);
                if (i > 0)
                {
                    And and = new And();
                    and.Add(w, wPrev);

                    wPrev = and;
                }
                else
                {
                    wPrev = w;
                }
                i++;
            }

            List<int> oids = wPrev.GetOIDs();

            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            lock (_syncRoot)
            {
               var transaction=transactionManager.GetActiveTransaction();
                {

                    foreach (int oid in oids)
                    {
                        this.MarkObjectAsDelete(serializer, oid, ti,transaction);

                        this.indexManager.UpdateIndexesAfterDelete(oid, ti,transaction);
                    }
                    
                }

            }
            return oids;

        }

        internal void DeleteObjectByOID(int oid, SqoTypeInfo tinf)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(tinf), useElevatedTrust);
           var transaction=transactionManager.GetActiveTransaction();
            {
                this.MarkObjectAsDelete(serializer, oid, tinf,transaction);
                
            }
        }

        private void CheckForConcurency(object oi, ObjectInfo objInfo, SqoTypeInfo ti, ObjectSerializer serializer, bool updateTickCountInDB,LightningTransaction transaction)
        {
            if (SiaqodbConfigurator.OptimisticConcurrencyEnabled)
            {
                FieldSqoInfo fi = MetaHelper.FindField(ti.Fields, "tickCount");
                if (fi != null)
                {
                    if (fi.AttributeType == typeof(ulong))
                    {
                        ulong tickCount = 0;

                        if (objInfo.Oid > 0 && objInfo.Oid <= ti.Header.numberOfRecords) //update or delete
                        {
                            var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);

                            byte[] key = ByteConverter.IntToByteArray(objInfo.Oid);

                            byte[] objBytes = transaction.Get(db, key);
                            if (objBytes == null)//object is deleted
                            {
                                throw new OptimisticConcurrencyException("Another version of object with OID=" + objInfo.Oid.ToString() + " of Type:" + ti.TypeName + " is saved in database, refresh your object before save!");
                            }
                            tickCount = (ulong)serializer.ReadFieldValue(ti, objInfo.Oid, objBytes, fi, transaction);

                            if (objInfo.TickCount != 0)
                            {
                                if (tickCount != objInfo.TickCount)
                                {
                                    throw new OptimisticConcurrencyException("Another version of object with OID=" + objInfo.Oid.ToString() + " of Type:" + ti.TypeName + " is saved in database, refresh your object before save!");
                                }
                            }


                        }
                        tickCount = tickCount + 1L;
                        objInfo.AtInfo[fi] = tickCount;
#if SILVERLIGHT
                    MetaHelper.CallSetValue(fi.FInfo, tickCount, oi, ti.Type);
#else
                        fi.FInfo.SetValue(oi, tickCount);
#endif

                        if (updateTickCountInDB)
                        {

                            var valuesToSave = serializer.SaveFieldValue(objInfo.Oid, "tickCount", ti, tickCount, this.rawSerializer, transaction);
                            var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);

                            byte[] key = ByteConverter.IntToByteArray(objInfo.Oid);

                            byte[] objBytes = transaction.Get(db, key);
                            Array.Copy(valuesToSave.Value, 0, objBytes, valuesToSave.Name, valuesToSave.Value.Length);
                            transaction.Put(db, key, objBytes);


                        }
                    }
                }
            }

        }
        private void CheckForConcurencyOnly(object oi, ObjectInfo objInfo, SqoTypeInfo ti, ObjectSerializer serializer)
        {
            if (SiaqodbConfigurator.OptimisticConcurrencyEnabled)
            {
                FieldSqoInfo fi = MetaHelper.FindField(ti.Fields, "tickCount");
                if (fi != null)
                {
                    if (fi.AttributeType == typeof(ulong))
                    {
                        ulong tickCount = 0;

                        if (objInfo.Oid > 0 && objInfo.Oid <= ti.Header.numberOfRecords) //update or delete
                        {
                           var transaction=transactionManager.GetActiveTransaction();
                            {
                                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                                {
                                    byte[] key = ByteConverter.IntToByteArray(objInfo.Oid);

                                    byte[] objBytes = transaction.Get(db, key);
                                    tickCount = (ulong)serializer.ReadFieldValue(ti, objInfo.Oid, objBytes, fi, transaction);
                                }
                            }

                            if (objInfo.TickCount != 0)
                            {
                                if (tickCount != objInfo.TickCount)
                                {
                                    throw new OptimisticConcurrencyException("Another version of object with OID=" + objInfo.Oid.ToString() + " of Type:" + ti.TypeName + " is saved in database, refresh your object before save!");
                                }
                            }


                        }
                    }
                }
            }
        }

  
        private void MarkObjectAsDelete(ObjectSerializer serializer, int oid, SqoTypeInfo ti, LightningTransaction transaction)
        {
            foreach (FieldSqoInfo ai in ti.Fields)
            {
                IByteTransformer byteTrans = ByteTransformerFactory.GetByteTransformer(null, null, ai, ti,oid);
                if (byteTrans is ArrayByteTranformer || byteTrans is DictionaryByteTransformer)
                {
                    string dbName = string.Format("raw.{0}", ti.GetDBName());

                    rawSerializer.DeleteRawRecord(oid,dbName,ai.Name, transaction);

                }
            }
           // byte[] deletedOid = serializer.MarkObjectAsDelete(oid, ti);
            var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);

            byte[] key = ByteConverter.IntToByteArray(oid);

            transaction.Delete(db, key);

        }

       
       
        #region Anchor (Sync FRW provider)
        internal void SaveAnchor(string key, byte[] value, LightningTransaction transaction)
        {
            string anchorDB = "AnchorDB";
            var db = transaction.OpenDatabase(anchorDB, DatabaseOpenFlags.Create);
            byte[] keyBytes = ByteConverter.StringToByteArray(key);
            transaction.Put(db, keyBytes, value);
        }
        internal byte[] GetAnchor(string key, LightningTransaction transaction)
        {
            string anchorDB = "AnchorDB";
            var db = transaction.OpenDatabase(anchorDB, DatabaseOpenFlags.Create);
            byte[] keyBytes = ByteConverter.StringToByteArray(key);

            return transaction.Get(db, keyBytes);


        }
        internal void DropAnchor(string key, LightningTransaction transaction)
        {
            string anchorDB = "AnchorDB";
            var db = transaction.OpenDatabase(anchorDB, DatabaseOpenFlags.Create);
            byte[] keyBytes = ByteConverter.StringToByteArray(key);
            try
            {
                transaction.Delete(db, keyBytes);
            }
            catch (LightningException ex)
            {
                if(ex.Message.StartsWith("MDB_NOTFOUND"))
                    return;
                else throw;

            }
        }
        #endregion
    }
}
