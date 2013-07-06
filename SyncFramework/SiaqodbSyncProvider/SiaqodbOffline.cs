using System;
using System.Net;


using Sqo;
using Microsoft.Synchronization.ClientServices;
using System.ComponentModel;
using Sqo.Transactions;

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
	            
	            base.StoreObject(obj);
			}
		}
        public new void StoreObject(object obj,Transaction transaction)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SiaqodbOfflineEntity type");
	            }
	            entity.IsDirty = true;
	
	            base.StoreObject(obj,transaction);
			}
        }
        internal void StoreObjectBase(ISqoDataObject obj)
        {

            base.StoreObject(obj);
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
	            entity.IsDirty = true;
	            entity.IsTombstone = true;
	            base.StoreObject(obj);
	
	            base.Delete(obj);
			}
        }
        public new void Delete(object obj,Transaction transaction)
        {	
			lock (_locker)
            {
            
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
	            entity.IsDirty = true;
	            entity.IsTombstone = true;
	            base.StoreObject(obj,transaction);
	            
	            base.Delete(obj,transaction);
			}
        }
        internal void DeleteBase(ISqoDataObject obj)
        {
            base.Delete(obj);
        }

        public new bool UpdateObjectBy(string fieldName, object obj)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
		        {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
	            entity.IsDirty = true;
	            bool updated = base.UpdateObjectBy(fieldName, obj);
	            if (!updated)
	            {
	                entity.IsDirty = false;
	
	            }
	            return updated;
			}
        }
        public new bool UpdateObjectBy(object obj, params string[] fieldNames)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
	            entity.IsDirty = true;
	            bool updated = base.UpdateObjectBy(obj, fieldNames);
	            if (!updated)
	            {
	                entity.IsDirty = false;
	
	            }
	            return updated;
			}
        }
        public new bool UpdateObjectBy(object obj, Transaction transaction,params string[] fieldNames)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
	            entity.IsDirty = true;
	            bool updated = base.UpdateObjectBy(obj,transaction, fieldNames);
	            if (!updated)
	            {
	                entity.IsDirty = false;
	
	            }
	            return updated;
			}
        }
        internal bool UpdateObjectByBase(string fieldName, ISqoDataObject obj)
        {
            return base.UpdateObjectBy(fieldName, obj);
        }
        internal bool UpdateObjectByBase(ISqoDataObject obj, params string[] fieldNames)
        {
            return base.UpdateObjectBy(obj,fieldNames);
        }
        public new bool DeleteObjectBy(object obj,params string[] fieldNames)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
	            entity.IsDirty = true;
	            entity.IsTombstone = true;
	
	            base.StoreObject(obj);
	
	            bool deleted = base.DeleteObjectBy(obj,fieldNames);
	            if (!deleted)
	            {
	                entity.IsDirty = false;
	                entity.IsTombstone = false;
	                base.StoreObject(obj);
	            }
	            return deleted;
			}
        }
        public new bool DeleteObjectBy(object obj,Transaction transaction, params string[] fieldNames)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
	            entity.IsDirty = true;
	            entity.IsTombstone = true;
	
	            base.StoreObject(obj);
	
	            bool deleted = base.DeleteObjectBy(obj,transaction, fieldNames);
	            if (!deleted)
	            {
	                entity.IsDirty = false;
	                entity.IsTombstone = false;
	                base.StoreObject(obj,transaction);
	            }
	            return deleted;
			}
        }
        public new bool DeleteObjectBy(string fieldName, object obj)
        {
            lock (_locker)
            {
				SiaqodbOfflineEntity entity = obj as SiaqodbOfflineEntity;
	            if (entity == null)
	            {
	                throw new Exception("Entity should be SqoOfflineEntity type");
	            }
	            entity.IsDirty = true;
	            entity.IsTombstone = true;
	
	            base.StoreObject(obj);
	
	            bool deleted = base.DeleteObjectBy(fieldName, obj);
	            if (!deleted)
	            {
	                entity.IsDirty = false;
	                entity.IsTombstone = false;
	                base.StoreObject(obj);
	            }
	            return deleted;
			}
        }
        internal bool DeleteObjectByBase(string fieldName, ISqoDataObject obj)
        {
            return base.DeleteObjectBy(fieldName, obj);
        }
        internal bool DeleteObjectByBase(ISqoDataObject obj,params string[] fieldNames )
        {
            return base.DeleteObjectBy(obj, fieldNames);
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

        public void Synchronize()
        {
            if (this.provider == null)
            {
                throw new Exception("Provider cannot be null");
            }
            this.provider.CacheController.RefreshCompleted -= new EventHandler<RefreshCompletedEventArgs>(CacheController_RefreshCompleted);
            this.provider.CacheController.RefreshCompleted += new EventHandler<RefreshCompletedEventArgs>(CacheController_RefreshCompleted);
            this.provider.CacheController.RefreshAsync();
            this.provider.SyncProgress -= new EventHandler<SyncProgressEventArgs>(provider_SyncProgress);
            this.provider.SyncProgress += new EventHandler<SyncProgressEventArgs>(provider_SyncProgress);
			


        }

        void CacheController_RefreshCompleted(object sender, RefreshCompletedEventArgs e)
        {
            this.Flush();
			SyncCompletedEventArgs args = new SyncCompletedEventArgs(e.Cancelled, e.Error, e.Statistics);
            this.OnSyncCompleted(args);
			
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
