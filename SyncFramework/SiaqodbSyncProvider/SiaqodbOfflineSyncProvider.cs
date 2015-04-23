using System;
using System.Net;
using Microsoft.Synchronization.ClientServices;
using System.Collections.Generic;
using Sqo;

using System.Linq;
using System.Reflection;
using Sqo.Attributes;
using Sqo.Internal;

using System.IO;
using SiaqodbSyncProvider.Utilities;
using Sqo.Transactions;
using System.Threading.Tasks;


using Microsoft.Synchronization.Services.Formatters;
namespace SiaqodbSyncProvider
{
    public class SiaqodbOfflineSyncProvider : OfflineSyncProvider
    {
        
        public  CacheController CacheController{get;set;}
        internal event EventHandler<SyncProgressEventArgs> SyncProgress;
        private Dictionary<Guid, ICollection<IOfflineEntity>> currentChanges = new Dictionary<Guid, ICollection<IOfflineEntity>>();
        private Dictionary<Guid, ICollection<DirtyEntity>> currentDirtyItems = new Dictionary<Guid, ICollection<DirtyEntity>>();
       
        SiaqodbOffline siaqodb;
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
           
        public event EventHandler<ConflictsEventArgs> ConflictOccur;

        public SiaqodbOfflineSyncProvider(SiaqodbOffline siaqodb, Uri uri)
        {
     
            
            if (!Sqo.Internal._bs._hsy())
            {
                throw new Exception("Siaqodb Sync Provider License not valid!");
            }

            CacheController = new CacheController(uri, "DefaultScope", this);
            this.siaqodb = siaqodb;
            
           
        }
        public SiaqodbOfflineSyncProvider(SiaqodbOffline siaqodb, Uri uri,string syncScopeName)
        {
            if (!Sqo.Internal._bs._hsy())
            {
                throw new Exception("Siaqodb Sync Provider License not valid!");
            }
            CacheController = new CacheController(uri, syncScopeName, this);
            this.siaqodb = siaqodb;
            
        }
        
        public bool UseElevatedTrust { get; set; }
        public override async Task BeginSession()
        {
           this.OnSyncProgress(new SyncProgressEventArgs("Synchronization started..."));
        }

        public override void EndSession()
        {
            this.siaqodb.Flush();
			this.OnSyncProgress(new SyncProgressEventArgs("Synchronization finished."));
        }

        public override async Task<ChangeSet> GetChangeSet(Guid state)
        {
            var changeSet = new ChangeSet();

            this.OnSyncProgress(new SyncProgressEventArgs("Getting local changes..."));
            var changes = this.GetChanges();
            this.OnSyncProgress(new SyncProgressEventArgs("Uploading:"+changes.Key.Count+" changes..."));

            changeSet.Data = changes.Key.Select(c => (IOfflineEntity)c).ToList();
            changeSet.IsLastBatch = true;
            changeSet.ServerBlob = this.GetServerBlob();
            if (changeSet.ServerBlob == null) //means never initialized
            {
                changeSet.Data = new List<IOfflineEntity>();
                changeSet.IsLastBatch = true;
            }
            currentChanges[state] = changeSet.Data;
            currentDirtyItems[state] = changes.Value;
            return changeSet;
        }
        private KeyValuePair<ICollection<SiaqodbOfflineEntity>,ICollection<DirtyEntity>> GetChanges()
        {

            List<SiaqodbOfflineEntity> changes = new List<SiaqodbOfflineEntity>();
          
            IList<DirtyEntity> allDirtyItems = siaqodb.LoadAll<DirtyEntity>();
            ILookup<string, DirtyEntity> lookup = allDirtyItems.ToLookup(a => a.EntityType);
            foreach (var item in lookup)
            {
                IEnumerable<DirtyEntity> entities = lookup[item.Key];
                Type type = ReflectionHelper.GetTypeByDiscoveringName(entities.First<DirtyEntity>().EntityType);
                Dictionary<int, Tuple<object, DirtyEntity>> inserts = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> updates = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> deletes = new Dictionary<int, Tuple<object, DirtyEntity>>();
                if (!this.CacheController.ControllerBehavior.KnownTypes.Contains(type))
                {
                    continue;
                }
                foreach (DirtyEntity en in entities)
                {
                    if (en.DirtyOp == DirtyOperation.Deleted)
                    {
                        if (inserts.ContainsKey(en.EntityOID))
                        {

                            inserts.Remove(en.EntityOID);
                            continue;
                        }
                        else if (updates.ContainsKey(en.EntityOID))
                        {

                            updates.Remove(en.EntityOID);
                        }
                    }
                    else
                    {
                        if (deletes.ContainsKey(en.EntityOID) || inserts.ContainsKey(en.EntityOID) || updates.ContainsKey(en.EntityOID))
                        {

                            continue;
                        }
                    }
                    object entityFromDB = null;
                    if (en.DirtyOp == DirtyOperation.Deleted)
                    {
                        entityFromDB = (SiaqodbOfflineEntity)JSerializer.Deserialize(type, en.TombstoneObj);
                    }
                    else
                    {
                        entityFromDB = _bs._lobjby(this.siaqodb, type, en.EntityOID);
                    }
                    if (en.DirtyOp == DirtyOperation.Inserted)
                    {
                        inserts.Add(en.EntityOID, new Tuple<object, DirtyEntity>(entityFromDB, en));
                    }
                    else if (en.DirtyOp == DirtyOperation.Updated)
                    {
                        updates.Add(en.EntityOID, new Tuple<object, DirtyEntity>(entityFromDB, en));
                    }
                    else if (en.DirtyOp == DirtyOperation.Deleted)
                    {
                        deletes.Add(en.EntityOID, new Tuple<object, DirtyEntity>(entityFromDB, en));
                    }
                    changes.Add((SiaqodbOfflineEntity)entityFromDB);
                }
            }


            return new KeyValuePair<ICollection<SiaqodbOfflineEntity>,ICollection<DirtyEntity>>( changes,allDirtyItems);
        }
       

        public override async Task OnChangeSetUploaded(Guid state, ChangeSetResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (response.Error != null)
            {
                throw response.Error;
            }
            this.OnSyncProgress(new SyncProgressEventArgs("Upload finished,mark local entities as uploaded..."));

            ITransaction transaction =siaqodb.BeginTransaction();
            try
            {
                if (null != response.UpdatedItems && 0 != response.UpdatedItems.Count)
                {
                    foreach (var item in response.UpdatedItems)
                    {
                        var offlineEntity = (SiaqodbOfflineEntity)item;
                        offlineEntity.IsDirty = false;
                        offlineEntity.IsTombstone = false;
                        this.SaveEntityByPK(offlineEntity,transaction);
                    }
                }

                if (response.Conflicts != null && response.Conflicts.Count > 0)
                {
                    ConflictsEventArgs ceArgs = new ConflictsEventArgs(response.Conflicts);
                    this.OnConflictOccur(ceArgs);
                    if (!ceArgs.CancelResolvingConflicts)
                    {

                        foreach (var conflict in response.Conflicts)
                        {
                            var offlineEntity = (SiaqodbOfflineEntity)conflict.LiveEntity;
                            offlineEntity.IsDirty = false;
                            offlineEntity.IsTombstone = false;
                            this.SaveEntity(offlineEntity,transaction);

                        }
                    }
                }

                ICollection<IOfflineEntity> changesJustUploaded = this.currentChanges[state];
                foreach (IOfflineEntity enI in changesJustUploaded)
                {
                    SiaqodbOfflineEntity en = enI as SiaqodbOfflineEntity;
                    //check if we did not updated above
                    if (null != response.UpdatedItems && 0 != response.UpdatedItems.Count)
                    {
                        bool existsUpdated = false;
                        foreach (var item in response.UpdatedItems)
                        {
                            var offlineEntity = (SiaqodbOfflineEntity)item;
                            if (EntitiesEqualByPK(offlineEntity, en))
                            {
                                existsUpdated = true;
                            }
                        }
                        if (existsUpdated)
                            continue;
                    }
                    if (response.Conflicts != null && response.Conflicts.Count > 0)
                    {
                        bool existsUpdated = false;
                        foreach (var conflict in response.Conflicts)
                        {
                            var offlineEntity = (SiaqodbOfflineEntity)conflict.LiveEntity;
                            if (EntitiesEqualByPK(offlineEntity, en))
                            {
                                existsUpdated = true;
                            }
                        }
                        if (existsUpdated)
                            continue;
                    }
                    if (en.IsTombstone)
                    {
                       //already deleted
                        //siaqodb.DeleteBase(en,transaction);

                    }
                    else
                    {
                        en.IsDirty = false;
                        siaqodb.StoreObjectBase(en,transaction);
                    }
                }
                foreach (DirtyEntity dEn in currentDirtyItems[state])
                {
                    siaqodb.DeleteBase(dEn, transaction);
                }
                this.SaveAnchor(response.ServerBlob);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                
                siaqodb.Flush();
            }

           

            this.OnSyncProgress(new SyncProgressEventArgs("Downloading changes from server..."));
            currentChanges.Remove(state);
        }
        private bool EntitiesEqualByPK(SiaqodbOfflineEntity a, SiaqodbOfflineEntity b)
        {
            if (a.GetType() == b.GetType())//has same type, eq: Customer
            {
                List<PropertyInfo> piPK = new List<PropertyInfo>();
                IEnumerable<PropertyInfo> pi = a.GetType().GetProperties();
                foreach (PropertyInfo p in pi)
                {
#if SILVERLIGHT
                    Type ty = typeof(Microsoft.Synchronization.ClientServices.KeyAttribute);

#else
                 Type ty = typeof(KeyAttribute);
#endif
#if WinRT
                 IEnumerable<Attribute> pk = p.GetCustomAttributes(ty, false);
#else
                object[] pk = p.GetCustomAttributes(ty, false);
#endif


                 if (pk != null && pk.ToList().Count > 0)
                    {
                        piPK.Add(p);

                    }
                }

                if (piPK.Count > 0)
                {
                    foreach (PropertyInfo pk in piPK)
                    {
                        var valA = pk.GetValue(a);
                        var valB = pk.GetValue(b);
                        if (valA == null || valB == null)
                        {
                            if (valA != valB)
                                return false;
                        }
                        else
                        {
                            int vomp = ((IComparable)valA).CompareTo((IComparable)valB);
                            if (vomp != 0)
                                return false;
                        }
                    }
                    return true;
                }

            }
            return false;
        }
        public void SaveAnchor(byte[] anchor)
        {

            string key = "anchor_" + CacheController.ControllerBehavior.ScopeName;
            _bs._sanc(this.siaqodb, anchor,key);
        }
        public bool DropAnchor()
        {
            string key = "anchor_" + CacheController.ControllerBehavior.ScopeName;
            _bs._danc(this.siaqodb, key);
            return true;
        }
        public override byte[] GetServerBlob()
        {

            string key = "anchor_" + CacheController.ControllerBehavior.ScopeName;
            return _bs._ganc(this.siaqodb, key);
        }

        
        public override async Task SaveChangeSet(ChangeSet changeSet)
        {
            if (null == changeSet)
            {
                throw new ArgumentException("changeSet is null", "changeSet");
            }
            this.OnSyncProgress(new SyncProgressEventArgs("Download finished, saving object on local db..."));
            var entities = changeSet.Data.Cast<SiaqodbOfflineEntity>();

            this.SaveDownloadedChanges(changeSet.ServerBlob, entities);

            this.OnSyncProgress(new SyncProgressEventArgs("Sync finished!"));
           
        }

        public void AddType<T>()where T:IOfflineEntity
        {
            this.CacheController.ControllerBehavior.AddType<T>();
           
        }
        int hash;
       
        private void SaveDownloadedChanges(byte[] anchor, IEnumerable<SiaqodbOfflineEntity> entities)
        {

            ITransaction transaction = siaqodb.BeginTransaction();
            try
            {
                foreach (SiaqodbOfflineEntity en in entities)
                {
                    SaveEntity(en, transaction);

                }
                SaveAnchor(anchor);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
               
                siaqodb.Flush();
            }
          
          
            
        }
        internal void SaveEntityByPK(SiaqodbOfflineEntity en,ITransaction transaction)
        {
            List<string> primaryKeys = new List<string>();

            IEnumerable<PropertyInfo> pi = en.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
            {
#if SILVERLIGHT
                Type ty = typeof(Microsoft.Synchronization.ClientServices.KeyAttribute);

#else
                 Type ty = typeof(KeyAttribute);
#endif
#if WinRT
                IEnumerable<Attribute> pk = p.GetCustomAttributes(ty, false);
#else
                object[] pk = p.GetCustomAttributes(ty, false);
#endif
                if ( pk!=null && pk.ToList().Count > 0)
                {
                    primaryKeys.Add(Sqo.Utilities.ExternalMetaHelper.GetBackingField(p));

                }
            }

            if (primaryKeys.Count > 0)
            {
                if (en.IsTombstone)
                {
                    siaqodb.DeleteObjectByBase(en,transaction, primaryKeys.ToArray());
                }
                else
                {
                    bool updated = siaqodb.UpdateObjectByBase(en,transaction, primaryKeys.ToArray());
                    
                }
            }
          

        }
        internal void SaveEntity(SiaqodbOfflineEntity en,ITransaction transaction)
        {

            if (en.IsTombstone)
            {
                try
                {
                    siaqodb.DeleteObjectByBase(en, transaction, "_idMetaHash");
                }
                catch (Exception ex)
                {
                    if (!ex.Message.StartsWith("MDB_NOTFOUND"))
                        throw;
                }
            }
            else
            {
                bool updated = siaqodb.UpdateObjectByBase(en, transaction, "_idMetaHash");
                if (!updated) //insert
                {
                    siaqodb.StoreObjectBase(en,transaction);
                }
            }



        }
        protected virtual void OnConflictOccur(ConflictsEventArgs args)
        {
            if (this.ConflictOccur != null)
            {
                this.ConflictOccur(this, args);
            }
        }
        public void Reinitialize()
        {
              ITransaction transaction = siaqodb.BeginTransaction();
              try
              {
                  foreach (Type t in CacheController.ControllerBehavior.KnownTypes)
                  {
                      siaqodb.DropType(t, transaction);
                  }
                  bool dropped = this.DropAnchor();
                  transaction.Commit();
              }
              catch (Exception ex)
              {
                  transaction.Rollback();
                  throw ex;
              }
        }
        internal void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress != null)
            {
                this.SyncProgress(this, args);
            }
        }
        
    }
    public class ConflictsEventArgs:EventArgs
    {
        IEnumerable<Conflict> conflicts;
       
        public ConflictsEventArgs(IEnumerable<Conflict> conflicts)
        {
            this.conflicts=conflicts;
        }
        public bool CancelResolvingConflicts { get; set; }
    }
    public class SyncProviderTypeVersion : SiaqodbOfflineEntity
    {
        public int Version;
        public int TypeNameHash;
    }
}
