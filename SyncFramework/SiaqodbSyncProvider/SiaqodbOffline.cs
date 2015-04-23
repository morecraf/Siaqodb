using System;
using System.Net;


using Sqo;
using Microsoft.Synchronization.ClientServices;
using System.ComponentModel;
using Sqo.Transactions;
using SiaqodbSyncProvider.Utilities;
using System.Threading.Tasks;

namespace SiaqodbSyncProvider
{
    public class SiaqodbOffline : Siaqodb
    {
        public event EventHandler<SyncProgressEventArgs> SyncProgress;
        public event EventHandler<SyncCompletedEventArgs> SyncCompleted;
		readonly object _locker = new object();
        SiaqodbOfflineSyncProvider provider;
       
        
        public SiaqodbOffline(string path, Uri uri) : base(path)
        {
            provider = new SiaqodbOfflineSyncProvider(this, uri);
        }
        public SiaqodbOffline(string path, Uri uri,string scopeName): base(path)
        {
            provider = new SiaqodbOfflineSyncProvider(this, uri,scopeName);
        }
        public SiaqodbOffline(string path, SiaqodbOfflineSyncProvider provider):base(path)
        {
            this.provider = provider;
        }
        public SiaqodbOffline():base()
        {

        }
#if SL4
        public SiaqodbOffline(string path,Environment.SpecialFolder specialFolder, Uri uri) : base(path,specialFolder)
        {
            provider = new SiaqodbOfflineSyncProvider(this, uri);
            provider.UseElevatedTrust = true;
        }
        public SiaqodbOffline(string path, Environment.SpecialFolder specialFolder, Uri uri, string scopeName)
            : base(path,specialFolder)
        {
            provider = new SiaqodbOfflineSyncProvider(this, uri,scopeName);
            provider.UseElevatedTrust = true;
        }
        public SiaqodbOffline(string path, Environment.SpecialFolder specialFolder, SiaqodbOfflineSyncProvider provider)
            : base(path,specialFolder)
        {
            this.provider = provider;
            provider.UseElevatedTrust = true;
        }
#endif
        private void CreateDirtyEntity(object obj, DirtyOperation dop, ITransaction transaction)
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
        private void CreateTombstoneDirtyEntity(SiaqodbOfflineEntity obj, int oid, ITransaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.EntityType = ReflectionHelper.GetDiscoveringTypeName(obj.GetType());
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
            obj.IsDirty = true;
            obj.IsTombstone = true;
            dirtyEntity.TombstoneObj = JSerializer.Serialize(obj);
            if (transaction != null)
            {
                base.StoreObject(dirtyEntity, transaction);
            }
            else
            {
                base.StoreObject(dirtyEntity);
            }

        }
        public SiaqodbOfflineSyncProvider SyncProvider { get { return provider; } set { provider = value; } }

        public new void StoreObject(object obj)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SiaqodbOfflineEntity type");
	            }
	            entity.IsDirty = true;
                DirtyOperation dop = entity.OID == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
	            base.StoreObject(obj);
                CreateDirtyEntity(obj, dop, null);
			}
		}
        public new void StoreObject(object obj,ITransaction transaction)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SiaqodbOfflineEntity type");
	            }
	            entity.IsDirty = true;
                DirtyOperation dop = entity.OID == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
	            base.StoreObject(obj,transaction);
                CreateDirtyEntity(obj, dop, transaction);
			}
        }

        internal void StoreObjectBase(ISqoDataObject obj)
        {

            base.StoreObject(obj);
        }
        internal void StoreObjectBase(ISqoDataObject obj,ITransaction transaction)
        {

            base.StoreObject(obj,transaction);
        }
        public new void Delete(object obj)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
                CreateTombstoneDirtyEntity(entity, entity.OID, null);
	
	            base.Delete(obj);
			}
        }
        public new void Delete(object obj,ITransaction transaction)
        {	
			lock (_locker)
            {
            
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
                CreateTombstoneDirtyEntity(entity, entity.OID, transaction);
	
	            
	            base.Delete(obj,transaction);
			}
        }
        internal void DeleteBase(object obj)
        {
            base.Delete(obj);
        }
        internal void DeleteBase(object obj,ITransaction transaction)
        {
            base.Delete(obj,transaction);
        }
        public new bool UpdateObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }
        public new bool UpdateObjectBy(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }
        public new bool UpdateObjectBy(object obj, ITransaction transaction,params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }
        internal bool UpdateObjectByBase(string fieldName, ISqoDataObject obj)
        {
            return base.UpdateObjectBy(fieldName, obj);
        }
        internal bool UpdateObjectByBase(ISqoDataObject obj, params string[] fieldNames)
        {
            return base.UpdateObjectBy(obj,fieldNames);
        }
        internal bool UpdateObjectByBase(ISqoDataObject obj,ITransaction transaction, params string[] fieldNames)
        {
            return base.UpdateObjectBy(obj,transaction, fieldNames);
        }
        public new bool DeleteObjectBy(object obj,params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }
        public new bool DeleteObjectBy(object obj,ITransaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }
        public new bool DeleteObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }
        internal bool DeleteObjectByBase(string fieldName, ISqoDataObject obj)
        {
            return base.DeleteObjectBy(fieldName, obj);
        }
       
        internal bool DeleteObjectByBase(ISqoDataObject obj,params string[] fieldNames )
        {
            return base.DeleteObjectBy(obj, fieldNames);
        }
        internal bool DeleteObjectByBase(ISqoDataObject obj,ITransaction transaction, params string[] fieldNames)
        {
            return base.DeleteObjectBy(obj, transaction, fieldNames);
        }
        public void AddTypeForSync<T>() where T : IOfflineEntity
        {
            if (this.provider == null)
            {
                throw new Exception("Provider cannot be null");
            }
            this.provider.AddType<T>();
        }

#if CF
        public CacheRefreshStatistics Synchronize()
        {
            if (this.provider == null)
            {
                throw new Exception("Provider cannot be null");
            }
            this.provider.SyncProgress -= new EventHandler<SyncProgressEventArgs>(provider_SyncProgress);
            this.provider.SyncProgress += new EventHandler<SyncProgressEventArgs>(provider_SyncProgress);
           
            return  this.provider.CacheController.Refresh();
        }
#else

        public async Task<CacheRefreshStatistics> Synchronize()
        {
            if (this.provider == null)
            {
                throw new Exception("Provider cannot be null");
            }
            this.provider.SyncProgress -= new EventHandler<SyncProgressEventArgs>(provider_SyncProgress);
            this.provider.SyncProgress += new EventHandler<SyncProgressEventArgs>(provider_SyncProgress);
			
            var stat= await this.provider.CacheController.SynchronizeAsync();
            SyncCompletedEventArgs args = new SyncCompletedEventArgs(stat.Cancelled, stat.Error, stat);
            this.OnSyncCompleted(args);
            return stat;

        }

       
#endif


        void provider_SyncProgress(object sender, SyncProgressEventArgs e)
        {
            this.OnSyncProgress(e);
        }
        public void AddScopeParameters(string key, string value)
        {
            if (this.provider == null)
            {
                throw new Exception("Provider cannot be null");
            }
            provider.CacheController.ControllerBehavior.AddScopeParameters(key, value);
        }
        protected void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress != null)
            {
                this.SyncProgress(this, args);
            }
        }
        protected void OnSyncCompleted(SyncCompletedEventArgs args)
        {
            if (this.SyncCompleted != null)
            {
                this.SyncCompleted(this, args);
            }
        }

    }

}
