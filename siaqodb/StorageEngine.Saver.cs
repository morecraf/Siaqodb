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
#if ASYNC
using System.Threading.Tasks;
#endif
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
#if ASYNC
        internal async Task<int> SaveObjectAsync(object oi, SqoTypeInfo ti)
        {
            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(oi, ti, metaCache);

            return await this.SaveObjectAsync(oi, ti, objInfo).ConfigureAwait(false);

        }
#endif
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
        
#if ASYNC
        internal async Task<int> SaveObjectAsync(object oi, SqoTypeInfo ti, ObjectInfo objInfo)
        {


            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedSaveComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedSaveComplexObjectAsync);
            await CheckForConcurencyAsync(oi, objInfo, ti, serializer, false).ConfigureAwait(false);

            await CheckConstraintsAsync(objInfo, ti).ConfigureAwait(false);

            Dictionary<string, object> oldValuesOfIndexedFields = await this.indexManager.PrepareUpdateIndexesAsync(objInfo, ti).ConfigureAwait(false);

            await serializer.SerializeObjectAsync(objInfo, this.rawSerializer).ConfigureAwait(false);

            metaCache.SetOIDToObject(oi, objInfo.Oid, ti);

            await this.indexManager.UpdateIndexesAsync(objInfo, ti, oldValuesOfIndexedFields).ConfigureAwait(false);

            return objInfo.Oid;



        }
#endif
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
#if ASYNC
        internal async Task SaveObjectPartiallyAsync(object obj, SqoTypeInfo ti, string[] properties)
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
                object oldPropVal = await indexManager.GetValueForFutureUpdateIndexAsync(oid, backingField, tiOfProp);
                await this.SaveValueAsync(oid, backingField, tiOfProp, objOfProp).ConfigureAwait(false);
                await indexManager.UpdateIndexesAsync(oid, backingField, tiOfProp, oldPropVal, objOfProp);

            }
        }
#endif
       
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
                if (serializer.IsObjectDeleted(oid, objBytes))
                    return false;

                Array.Copy(valuesToSave.Value, 0, objBytes, valuesToSave.Name, valuesToSave.Value.Length);
                transaction.Put(db, key, objBytes);
                return true;


            }
            else
                return false;

        }
#if ASYNC
        internal async Task<bool> SaveValueAsync(int oid, string field, SqoTypeInfo ti, object value)
        {
            if (field == "OID")
            {
                throw new SiaqodbException("OID cannot be saved from client code!");
            }
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedSaveComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedSaveComplexObjectAsync);

            if (oid > 0 && oid <= ti.Header.numberOfRecords && !(await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false)))
            {

                return await serializer.SaveFieldValueAsync(oid, field, ti, value, this.rawSerializer).ConfigureAwait(false);
            }
            else
                return false;

        }
#endif
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
#if ASYNC
        internal async Task<bool> UpdateObjectByAsync(string[] fieldNames, object obj, SqoTypeInfo ti, Transaction transact)
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

            List<int> oids = await wPrev.GetOIDsAsync().ConfigureAwait(false);
            if (oids.Count > 1)
            {
                throw new SiaqodbException("In database exists more than one object with value of fields specified");
            }
            else if (oids.Count == 1)
            {
                objInfo.Oid = oids[0];
              
                if (transact == null)
                {
                    await this.SaveObjectAsync(obj, ti, objInfo).ConfigureAwait(false);
                }
                else
                {
                    await this.SaveObjectAsync(obj, ti, objInfo, transact).ConfigureAwait(false);
                }
                return true;
            }
            else
            {
                return false;
            }


        }
#endif
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
#if ASYNC
        async Task serializer_NeedSaveComplexObjectAsync(object sender, ComplexObjectEventArgs e)
        {
            if (e.JustSetOID)
            {
                metaCache.SetOIDToObject(e.ObjInfo.BackendObject, e.ObjInfo.Oid, e.ObjInfo.SqoTypeInfo);
            }
            else
            {
                await this.OnNeedSaveComplexObjectAsync(e).ConfigureAwait(false);
            }
            return;

        }
      
#endif
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
#if ASYNC
        private async Task CheckConstraintsAsync(ObjectInfo objInfo, SqoTypeInfo ti)
        {
            if (objInfo.Oid <= 0) //if insert
            {
                foreach (FieldSqoInfo fi in ti.UniqueFields)
                {
                    Where w = new Where(fi.Name, OperationType.Equal, objInfo.AtInfo[fi]);
                    List<int> oids = await this.LoadFilteredOidsAsync(w, ti).ConfigureAwait(false);
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
                    List<int> oids = await this.LoadFilteredOidsAsync(w, ti).ConfigureAwait(false);
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
#endif
        
        internal void DeleteObject(object obj, SqoTypeInfo ti,LightningTransaction transaction)
        {
            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti,metaCache);

            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            lock (_syncRoot)
            {

                CheckForConcurency(obj, objInfo, ti, serializer, true, transaction);

                this.MarkObjectAsDelete(serializer, objInfo.Oid, ti, transaction);

                this.indexManager.UpdateIndexesAfterDelete(objInfo, ti, transaction);

                metaCache.SetOIDToObject(obj, -1, ti);


            }
        }
#if ASYNC
        internal async Task DeleteObjectAsync(object obj, SqoTypeInfo ti)
        {
            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti, metaCache);

            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            await CheckForConcurencyAsync(obj, objInfo, ti, serializer, true).ConfigureAwait(false);

            await this.MarkObjectAsDeleteAsync(serializer, objInfo.Oid, ti).ConfigureAwait(false);

            await this.indexManager.UpdateIndexesAfterDeleteAsync(objInfo, ti).ConfigureAwait(false);

            metaCache.SetOIDToObject(obj, -1, ti);



        }
#endif
        
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
                this.DeleteObject(obj, ti, transaction);
                
                return oids[0];
            }
            else
            {
                return -1;
            }
        }
#if ASYNC
        internal async Task<int> DeleteObjectByAsync(string[] fieldNames, object obj, SqoTypeInfo ti, Transaction transaction)
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

            List<int> oids = await wPrev.GetOIDsAsync().ConfigureAwait(false);


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

                if (transaction == null)
                {


                    await CheckForConcurencyAsync(obj, objInfo, ti, serializer, true).ConfigureAwait(false);

                    await this.MarkObjectAsDeleteAsync(serializer, objInfo.Oid, ti).ConfigureAwait(false);

                    await this.indexManager.UpdateIndexesAfterDeleteAsync(objInfo, ti).ConfigureAwait(false);


                }
                else
                {
                    await this.DeleteObjectAsync(obj, ti, transaction, objInfo).ConfigureAwait(false);
                }

                return oids[0];
            }
            else
            {
                return -1;
            }
        }
#endif
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
#if ASYNC
        internal async Task<List<int>> DeleteObjectByAsync(SqoTypeInfo ti, Dictionary<string, object> criteria)
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

            List<int> oids = await wPrev.GetOIDsAsync().ConfigureAwait(false);

            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            foreach (int oid in oids)
            {
                await this.MarkObjectAsDeleteAsync(serializer, oid, ti).ConfigureAwait(false);

                await this.indexManager.UpdateIndexesAfterDeleteAsync(oid, ti).ConfigureAwait(false);
            }


            return oids;

        }
#endif
        internal void DeleteObjectByOID(int oid, SqoTypeInfo tinf)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(tinf), useElevatedTrust);
           var transaction=transactionManager.GetActiveTransaction();
            {
                this.MarkObjectAsDelete(serializer, oid, tinf,transaction);
                
            }
        }
#if ASYNC
        internal async Task DeleteObjectByOIDAsync(int oid, SqoTypeInfo tinf)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(tinf), useElevatedTrust);
            await this.MarkObjectAsDeleteAsync(serializer, oid, tinf).ConfigureAwait(false);
        }
#endif
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
#if ASYNC
        private async Task CheckForConcurencyAsync(object oi, ObjectInfo objInfo, SqoTypeInfo ti, ObjectSerializer serializer, bool updateTickCountInDB)
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
                            tickCount = (ulong)(await serializer.ReadFieldValueAsync(ti, objInfo.Oid, fi).ConfigureAwait(false));
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
                            await serializer.SaveFieldValueAsync(objInfo.Oid, "tickCount", ti, tickCount, this.rawSerializer).ConfigureAwait(false);
                        }
                    }
                }
            }

        }
#endif
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
#if ASYNC
        private async Task CheckForConcurencyOnlyAsync(object oi, ObjectInfo objInfo, SqoTypeInfo ti, ObjectSerializer serializer)
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
                            tickCount = (ulong)(await serializer.ReadFieldValueAsync(ti, objInfo.Oid, fi).ConfigureAwait(false));
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
#endif
        internal int AllocateNewOID(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.SaveNrRecords(ti, ti.Header.numberOfRecords + 1);
            return ti.Header.numberOfRecords;
        }
#if ASYNC
        internal async Task<int> AllocateNewOIDAsync(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            await serializer.SaveNrRecordsAsync(ti, ti.Header.numberOfRecords + 1).ConfigureAwait(false);
            return ti.Header.numberOfRecords;
        }
#endif
        private void MarkObjectAsDelete(ObjectSerializer serializer, int oid, SqoTypeInfo ti, LightningTransaction transaction)
        {
            foreach (FieldSqoInfo ai in ti.Fields)
            {
                IByteTransformer byteTrans = ByteTransformerFactory.GetByteTransformer(null, null, ai, ti);
                if (byteTrans is ArrayByteTranformer || byteTrans is DictionaryByteTransformer)
                {
                    ATuple<int, int> arrayInfo = this.GetArrayMetaOfField(ti, oid, ai);
                    if (arrayInfo.Name > 0)
                    {
                        rawSerializer.MarkRawInfoAsFree(arrayInfo.Name, transaction);//this helps Shrink method to detect unused rawdata blocks.
                    }
                }
            }
            byte[] deletedOid = serializer.MarkObjectAsDelete(oid, ti);
            var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);


            byte[] key = ByteConverter.IntToByteArray(oid);

            byte[] objBytes = transaction.Get(db, key);
            Array.Copy(deletedOid, 0, objBytes, 0, deletedOid.Length);
            transaction.Put(db, key, objBytes);




        }
#if ASYNC
        private async Task MarkObjectAsDeleteAsync(ObjectSerializer serializer, int oid, SqoTypeInfo ti)
        {
            foreach (FieldSqoInfo ai in ti.Fields)
            {
                IByteTransformer byteTrans = ByteTransformerFactory.GetByteTransformer(null, null, ai, ti);
                if (byteTrans is ArrayByteTranformer || byteTrans is DictionaryByteTransformer)
                {
                    ATuple<int, int> arrayInfo = await this.GetArrayMetaOfFieldAsync(ti, oid, ai).ConfigureAwait(false);
                    if (arrayInfo.Name > 0)
                    {
                        await rawSerializer.MarkRawInfoAsFreeAsync(arrayInfo.Name).ConfigureAwait(false);//this helps Shrink method to detect unused rawdata blocks.
                    }
                }
            }
            await serializer.MarkObjectAsDeleteAsync(oid, ti).ConfigureAwait(false);
        }
#endif
        
#if ASYNC
        private async Task MarkFreeSpaceAsync(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            int nrRecords = ti.Header.numberOfRecords;
            List<FieldSqoInfo> existingDynamicFields = new List<FieldSqoInfo>();
            foreach (FieldSqoInfo ai in ti.Fields)
            {
                IByteTransformer byteTrans = ByteTransformerFactory.GetByteTransformer(null, null, ai, ti);
                if (byteTrans is ArrayByteTranformer || byteTrans is DictionaryByteTransformer)
                {
                    existingDynamicFields.Add(ai);
                }
            }
            if (existingDynamicFields.Count > 0)
            {
                for (int i = 0; i < nrRecords; i++)
                {

                    int oid = i + 1;
                    foreach (FieldSqoInfo ai in existingDynamicFields)
                    {
                        ATuple<int, int> arrayInfo = await this.GetArrayMetaOfFieldAsync(ti, oid, ai).ConfigureAwait(false);
                        if (arrayInfo.Name > 0)
                        {
                            await rawSerializer.MarkRawInfoAsFreeAsync(arrayInfo.Name).ConfigureAwait(false);//this helps Shrink method to detect unused rawdata blocks.
                        }
                    }
                }
            }
        }
#endif
        internal void MarkRawInfoAsFree(List<int> rawdataInfoOIDs)
        {
           var transaction=transactionManager.GetActiveTransaction();
            {
                foreach (int oid in rawdataInfoOIDs)
                {
                    rawSerializer.MarkRawInfoAsFree(oid,transaction);//this helps Shrink method to detect unused rawdata blocks.
                }
                
            }
        }
#if ASYNC
        internal async Task MarkRawInfoAsFreeAsync(List<int> rawdataInfoOIDs)
        {
            foreach (int oid in rawdataInfoOIDs)
            {
                await rawSerializer.MarkRawInfoAsFreeAsync(oid).ConfigureAwait(false);//this helps Shrink method to detect unused rawdata blocks.
            }
        }
#endif
        internal void SetFileLength(long newLength, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.SetLength(newLength);
        }
        internal int SaveObjectBytes(byte[] objBytes, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return serializer.SerializeObjectWithNewOID(objBytes, ti);
          
        }
#if ASYNC
        internal async Task<int> SaveObjectBytesAsync(byte[] objBytes, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return await serializer.SerializeObjectWithNewOIDAsync(objBytes, ti).ConfigureAwait(false);

        }
#endif
        internal void AdjustComplexFieldsAfterShrink(SqoTypeInfo ti, IList<ShrinkResult> shrinkResults)
        {
            throw new NotImplementedException();
            /*ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            List<FieldSqoInfo> complexFields = (from FieldSqoInfo fi in ti.Fields
                                                where fi.AttributeTypeId == MetaExtractor.complexID || fi.AttributeTypeId == MetaExtractor.documentID
                                                select fi).ToList();
            if (complexFields.Count > 0)
            {
                foreach (FieldSqoInfo fi in complexFields)
                {
                    Dictionary<int,int> oldOidNewOid=new Dictionary<int,int>();
                    int nrRecords = ti.Header.numberOfRecords;
                    int k = 0;
                    for (int i = 0; i < nrRecords; i++)
                    {

                        int oid = i + 1;
                        KeyValuePair<int,int> Oid_Tid= serializer.ReadOIDAndTID(ti, oid, fi);
                        if (Oid_Tid.Key == 0 && Oid_Tid.Value == 0)//mean complex object is null
                        {
                            continue;
                        }
                        if (k == 0)
                        {
                            var shrinkResultsFiltered = from ShrinkResult shrinkRes in shrinkResults
                                                     where shrinkRes.TID == Oid_Tid.Value
                                                     select shrinkRes;
                            
                            foreach(ShrinkResult shF in shrinkResultsFiltered)
                            {
                                oldOidNewOid[shF.Old_OID]=shF.New_OID;
                            }
                        }
                        if (oldOidNewOid.ContainsKey(Oid_Tid.Key))
                        {
                            int newOid = oldOidNewOid[Oid_Tid.Key];
                            
                            serializer.SaveComplexFieldContent(new KeyValuePair<int,int>(newOid,Oid_Tid.Value), fi, ti, oid);
                        }
                        k++;
                    }
                }
            }*/
        }
#if ASYNC
        internal async Task AdjustComplexFieldsAfterShrinkAsync(SqoTypeInfo ti, IList<ShrinkResult> shrinkResults)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            List<FieldSqoInfo> complexFields = (from FieldSqoInfo fi in ti.Fields
                                                where fi.AttributeTypeId == MetaExtractor.complexID
                                                select fi).ToList();
            if (complexFields.Count > 0)
            {
                foreach (FieldSqoInfo fi in complexFields)
                {
                    Dictionary<int, int> oldOidNewOid = new Dictionary<int, int>();
                    int nrRecords = ti.Header.numberOfRecords;
                    int k = 0;
                    for (int i = 0; i < nrRecords; i++)
                    {

                        int oid = i + 1;
                        KeyValuePair<int, int> Oid_Tid = await serializer.ReadOIDAndTIDAsync(ti, oid, fi).ConfigureAwait(false);
                        if (Oid_Tid.Key == 0 && Oid_Tid.Value == 0)//mean complex object is null
                        {
                            continue;
                        }
                        if (k == 0)
                        {
                            var shrinkResultsFiltered = from ShrinkResult shrinkRes in shrinkResults
                                                        where shrinkRes.TID == Oid_Tid.Value
                                                        select shrinkRes;

                            foreach (ShrinkResult shF in shrinkResultsFiltered)
                            {
                                oldOidNewOid[shF.Old_OID] = shF.New_OID;
                            }
                        }
                        if (oldOidNewOid.ContainsKey(Oid_Tid.Key))
                        {
                            int newOid = oldOidNewOid[Oid_Tid.Key];

                            await serializer.SaveComplexFieldContentAsync(new KeyValuePair<int, int>(newOid, Oid_Tid.Value), fi, ti, oid).ConfigureAwait(false);
                        }
                        k++;
                    }
                }
            }
        }
#endif
        internal void AdjustArrayFieldsAfterShrink(SqoTypeInfo ti, FieldSqoInfo fi, int objectOID, int newOID)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.SaveArrayOIDFieldContent(ti, fi, objectOID, newOID);

        }
    }
}
