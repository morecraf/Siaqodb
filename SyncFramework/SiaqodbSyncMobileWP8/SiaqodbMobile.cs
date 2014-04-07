using Sqo;
using Sqo.Exceptions;
using Sqo.Transactions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SiaqodbSyncMobile
{
    public class SiaqodbMobile:Siaqodb
    {

        
        readonly object _locker = new object();
        private readonly AsyncLock _lockerAsync = new AsyncLock();

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

        private void CreateDirtyEntity(object obj, DirtyOperation dop)
        {
            this.CreateDirtyEntity(obj,dop, null);
        }
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop)
        {
            await this.CreateDirtyEntityAsync(obj, dop, null);
        }
        private void CreateDirtyEntity(object obj,DirtyOperation dop,Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = base.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            dirtyEntity.EntityType = ReflectionHelper.GetDiscoveringTypeName(obj.GetType());
            
            if (transaction != null)
            {
                base.StoreObject(dirtyEntity, transaction);
            }
            else
            {
                base.StoreObject(dirtyEntity);
            }
        }
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop, Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = base.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            dirtyEntity.EntityType = ReflectionHelper.GetDiscoveringTypeName(obj.GetType());

            if (transaction != null)
            {
                await base.StoreObjectAsync(dirtyEntity, transaction);
            }
            else
            {
                await base.StoreObjectAsync(dirtyEntity);
            }
        }
        private void CreateTombstoneDirtyEntity(object obj, int oid)
        {
            CreateTombstoneDirtyEntity(obj, oid, null);
        }
        private async Task CreateTombstoneDirtyEntityAsync(object obj, int oid)
        {
            await CreateTombstoneDirtyEntityAsync(obj, oid, null);
        }
        private void CreateTombstoneDirtyEntity(object obj,int oid,Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.EntityType = ReflectionHelper.GetDiscoveringTypeName(obj.GetType());
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
           
            if (transaction != null)
            {
                base.StoreObject(dirtyEntity,transaction);
            }
            else
            {
                base.StoreObject(dirtyEntity);
            }
            
        }
        private async Task CreateTombstoneDirtyEntityAsync(object obj, int oid, Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.EntityType = ReflectionHelper.GetDiscoveringTypeName(obj.GetType());
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;

            if (transaction != null)
            {
                await base.StoreObjectAsync(dirtyEntity, transaction);
            }
            else
            {
                await base.StoreObjectAsync(dirtyEntity);
            }

        }
        public new void StoreObject(object obj)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by ServerID
                {
                    Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                }
                oid = base.GetOID(obj);
                DirtyOperation dop = oid == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
                base.StoreObject(obj);
                CreateDirtyEntity(obj, dop);
            }
        }
        public new async Task StoreObjectAsync(object obj)
        {
              bool locked = false; await _lockerAsync.LockAsync(obj.GetType(), out locked);

              try
              {
                  int oid = base.GetOID(obj);
                  if (oid == 0)//try get by ServerID
                  {
                      Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                  }
                  oid = base.GetOID(obj);
                  DirtyOperation dop = oid == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
                  await base.StoreObjectAsync(obj);
                  await CreateDirtyEntityAsync(obj, dop);
              }
              finally
              {
                  if (locked) _lockerAsync.Release();
              }

        }
        public new void StoreObject(object obj, Transaction transaction)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by ServerID
                {
                    Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                }
                oid = base.GetOID(obj);
                DirtyOperation dop = oid == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
                base.StoreObject(obj, transaction);
                CreateDirtyEntity(obj, dop, transaction);
            }
        }
        internal void StoreObjectBase(object obj)
        {
            base.StoreObject(obj);
        }
        public new async Task StoreObjectAsync(object obj, Transaction transaction)
        {
             bool locked = false; await _lockerAsync.LockAsync(obj.GetType(), out locked);
             try
             {
                 int oid = base.GetOID(obj);
                 if (oid == 0)//try get by ServerID
                 {
                     Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                 }
                 oid = base.GetOID(obj);
                 DirtyOperation dop = oid == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
                 await base.StoreObjectAsync(obj, transaction);
                 await CreateDirtyEntityAsync(obj, dop, transaction);
             }
             finally
             {
                 if (locked) _lockerAsync.Release();
             }

        }
        internal async Task StoreObjectBaseAsync(object obj)
        {
            await base.StoreObjectAsync(obj);
        }
        public new void Delete(object obj)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by ServerID
                {
                    Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                }
                oid = base.GetOID(obj);
                base.Delete(obj);
                CreateTombstoneDirtyEntity(obj, oid);
            }
        }
        public new async Task DeleteAsync(object obj)
        {
             bool locked = false; await _lockerAsync.LockAsync(obj.GetType(), out locked);
             try
             {
                 int oid = base.GetOID(obj);
                 if (oid == 0)//try get by ServerID
                 {
                     Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                 }
                 oid = base.GetOID(obj);
                 await base.DeleteAsync(obj);
                 await CreateTombstoneDirtyEntityAsync(obj, oid);
             }
             finally { if (locked) _lockerAsync.Release(); }
        }
        public new void Delete(object obj, Transaction transaction)
        {
            lock (_locker)
            {
                int oid = base.GetOID(obj);
                if (oid == 0)//try get by ServerID
                {
                    Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                }
                oid = base.GetOID(obj);
                base.Delete(obj, transaction);
                CreateTombstoneDirtyEntity(obj, oid,transaction);
            }
        }
        public new async Task DeleteAsync(object obj, Transaction transaction)
        {
             bool locked = false; await _lockerAsync.LockAsync(obj.GetType(), out locked);
             try
             {
                 int oid = base.GetOID(obj);
                 if (oid == 0)//try get by ServerID
                 {
                     Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);
                 }
                 oid = base.GetOID(obj);
                 await base.DeleteAsync(obj, transaction);
                 await CreateTombstoneDirtyEntityAsync(obj, oid, transaction);
             }
             finally { if (locked) _lockerAsync.Release(); }
        }
        internal void DeleteBase(object obj)
        {
            base.Delete(obj);
        }
        internal async Task DeleteBaseAsync(object obj)
        {
            await base.DeleteAsync(obj);
        }
        public new bool UpdateObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new async Task<bool> UpdateObjectByAsync(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool UpdateObjectBy(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new async Task<bool> UpdateObjectByAsync(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool UpdateObjectBy(object obj, Transaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new async Task<bool> UpdateObjectByAsync(object obj, Transaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        internal bool UpdateObjectByBase(string fieldName, object obj)
        {
            return base.UpdateObjectBy(fieldName, obj);
        }
        internal async Task<bool> UpdateObjectByBaseAsync(string fieldName, object obj)
        {
            return await base.UpdateObjectByAsync(fieldName, obj);
        }
        internal bool UpdateObjectByBase(object obj, params string[] fieldNames)
        {
            return base.UpdateObjectBy(obj, fieldNames);
        }
        internal async Task<bool> UpdateObjectByBaseAsync(object obj, params string[] fieldNames)
        {
            return await base.UpdateObjectByAsync(obj, fieldNames);
        }
        public new bool DeleteObjectBy(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new async Task<bool> DeleteObjectByAsync(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool DeleteObjectBy(object obj, Transaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new async Task<bool> DeleteObjectByAsync(object obj, Transaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new bool DeleteObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new async Task<bool> DeleteObjectByAsync(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        internal bool DeleteObjectByBase(string fieldName, object obj)
        {
            return base.DeleteObjectBy(fieldName, obj);
        }
        internal async Task<bool> DeleteObjectByBaseAsync(string fieldName, object obj)
        {
            return await base.DeleteObjectByAsync(fieldName, obj);
        }
        internal bool DeleteObjectByBase(object obj, params string[] fieldNames)
        {
            return base.DeleteObjectBy(obj, fieldNames);
        }
        internal async Task<bool> DeleteObjectByBaseAsync(object obj, params string[] fieldNames)
        {
            return await base.DeleteObjectByAsync(obj, fieldNames);
        }
        public new int DeleteObjectBy<T>(Dictionary<string, object> criteria)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public async Task<int> DeleteObjectByAsync<T>(Dictionary<string, object> criteria)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new int DeleteObjectBy(Type objectType,Dictionary<string, object> criteria)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        public new async Task<int> DeleteObjectByAsync(Type objectType, Dictionary<string, object> criteria)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbMobile.");
        }
        internal int DeleteObjectByBase(Type objectType, Dictionary<string, object> criteria)
        {
            return base.DeleteObjectBy(objectType, criteria);
        }
        internal async Task<int> DeleteObjectByBaseAsync(Type objectType, Dictionary<string, object> criteria)
        {
            return await base.DeleteObjectByAsync(objectType, criteria);
        }
        public void AddSyncType<T>(string azure_table)
        {
            SyncProvider.AddAsyncType<T>(azure_table);
        }
       internal void StoreDownloadedEntity(object obj)
        {
            lock (_locker)
            {

                Sqo.Internal._bs._loidby(this, ReflectionHelper.GetIdBackingField(obj.GetType()), obj);

                base.StoreObject(obj);
            }
        }

      
    }
}
