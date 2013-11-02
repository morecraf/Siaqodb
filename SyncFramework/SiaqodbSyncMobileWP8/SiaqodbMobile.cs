using Sqo;
using Sqo.Exceptions;
using Sqo.Transactions;
using System;
using System.Collections.Generic;
using System.Net;

namespace SiaqodbSyncMobile
{
    public class SiaqodbMobile:Siaqodb
    {

        
        readonly object _locker = new object();
#if !NETFX_CORE
        public SiaqodbMobile(string applicationUrl,string applicationKey,string dbPath):base(dbPath)
        {
            this.SyncProvider = new SiaqodbSyncMobileProvider(this,applicationUrl,applicationKey);
        }
#else
        public SiaqodbMobile(string applicationUrl, string applicationKey, Windows.Storage.StorageFolder databaseFolder): base()
        {
            this.Open(databaseFolder);
            this.SyncProvider = new SiaqodbSyncMobileProvider(this, applicationUrl, applicationKey);

        }
#endif
        public SiaqodbSyncMobileProvider SyncProvider { get; set; }
      
        private void CreateDirtyEntity(object obj)
        {
            this.CreateDirtyEntity(obj, null);
        }
        private void CreateDirtyEntity(object obj,Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = base.GetOID(obj);
            dirtyEntity.EntityType = obj.GetType().AssemblyQualifiedName;//TODO
            
            if (transaction != null)
            {
                base.StoreObject(dirtyEntity, transaction);
            }
            else
            {
                base.StoreObject(dirtyEntity);
            }
        }
        private void CreateTombstoneDirtyEntity(object obj, int oid)
        {
            CreateTombstoneDirtyEntity(obj, oid, null);
        }
        private void CreateTombstoneDirtyEntity(object obj,int oid,Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.EntityType = obj.GetType().AssemblyQualifiedName;
            dirtyEntity.IsTombstone = true;
           
            if (transaction != null)
            {
                base.StoreObject(dirtyEntity,transaction);
            }
            else
            {
                base.StoreObject(dirtyEntity);
            }
            
        }
        public new void StoreObject(object obj)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by UID
                {
                    Sqo.Internal._bs._loidby(this, "<UID>k__BackingField", obj);
                }
                base.StoreObject(obj);
                CreateDirtyEntity(obj);
            }
        }
        public new void StoreObject(object obj, Transaction transaction)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by UID
                {
                    Sqo.Internal._bs._loidby(this, "<UID>k__BackingField", obj);
                }
                base.StoreObject(obj, transaction);
                CreateDirtyEntity(obj, transaction);
            }
        }
        internal void StoreObjectBase(object obj)
        {
            base.StoreObject(obj);
        }
        public new void Delete(object obj)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by UID
                {
                    Sqo.Internal._bs._loidby(this, "<UID>k__BackingField", obj);
                }
                base.Delete(obj);
                CreateTombstoneDirtyEntity(obj, oid);
            }
        }
        public new void Delete(object obj, Transaction transaction)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by UID
                {
                    Sqo.Internal._bs._loidby(this, "<UID>k__BackingField", obj);
                }
                base.Delete(obj, transaction);
                CreateTombstoneDirtyEntity(obj, oid,transaction);
            }
        }
        internal void DeleteBase(object obj)
        {
            base.Delete(obj);
        }
        public new bool UpdateObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool UpdateObjectBy(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool UpdateObjectBy(object obj, Transaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        internal bool UpdateObjectByBase(string fieldName, object obj)
        {
            return base.UpdateObjectBy(fieldName, obj);
        }
        internal bool UpdateObjectByBase(object obj, params string[] fieldNames)
        {
            return base.UpdateObjectBy(obj, fieldNames);
        }
        public new bool DeleteObjectBy(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool DeleteObjectBy(object obj, Transaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool DeleteObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        internal bool DeleteObjectByBase(string fieldName, object obj)
        {
            return base.DeleteObjectBy(fieldName, obj);
        }
        internal bool DeleteObjectByBase(object obj, params string[] fieldNames)
        {
            return base.DeleteObjectBy(obj, fieldNames);
        }
        public int DeleteObjectBy<T>(Dictionary<string, object> criteria)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public int DeleteObjectBy(Type objectType,Dictionary<string, object> criteria)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        internal int DeleteObjectByBase(Type objectType, Dictionary<string, object> criteria)
        {
            return base.DeleteObjectBy(objectType, criteria);
        }
        public void AddSyncType<T>(string azure_table)
        {
            SyncProvider.AddAsyncType<T>(azure_table);
        }
       internal void StoreDownloadedEntity(object obj)
        {
            lock (_locker)
            {

                Sqo.Internal._bs._loidby(this, "<UID>k__BackingField", obj);

                base.StoreObject(obj);
            }
        }
      
    }
}
