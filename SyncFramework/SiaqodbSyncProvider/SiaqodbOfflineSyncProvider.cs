using System;
using System.Net;
using Microsoft.Synchronization.ClientServices;
using System.Collections.Generic;
using Sqo;
#if !CF
using System.IO.IsolatedStorage;
#endif
using System.Linq;
using System.Reflection;
using Sqo.Attributes;
using Sqo.Internal;
#if MONODROID
using Java.IO;
#else
using System.IO;
#endif
namespace SiaqodbSyncProvider
{
    public class SiaqodbOfflineSyncProvider : OfflineSyncProvider
    {
        
        public  CacheController CacheController{get;set;}
        internal event EventHandler<SyncProgressEventArgs> SyncProgress;

        SiaqodbOffline siaqodb;
        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
           
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
        public override void BeginSession()
        {
           this.OnSyncProgress(new SyncProgressEventArgs("Synchronization started..."));
        }

        public override void EndSession()
        {
            this.siaqodb.Flush();
			this.OnSyncProgress(new SyncProgressEventArgs("Synchronization finished."));
        }

        public override ChangeSet GetChangeSet(Guid state)
        {
            var changeSet = new ChangeSet();

            this.OnSyncProgress(new SyncProgressEventArgs("Getting local changes..."));
            List<SiaqodbOfflineEntity> changes = this.GetChanges();
            this.OnSyncProgress(new SyncProgressEventArgs("Uploading:"+changes.Count+" changes..."));

            changeSet.Data = changes.Select(c => (IOfflineEntity)c).ToList();
            changeSet.IsLastBatch = true;
            changeSet.ServerBlob = this.GetServerBlob();
            if (changeSet.ServerBlob.Length == 0) //means never initialized
            {
                changeSet.Data = new List<IOfflineEntity>();
                changeSet.IsLastBatch = true;
            }
           
            return changeSet;
        }
        private List<SiaqodbOfflineEntity> GetChanges()
        {

            List<SiaqodbOfflineEntity> changes = new List<SiaqodbOfflineEntity>();

            foreach (Type t in CacheController.ControllerBehavior.KnownTypes)
            {
                List<object> objects = _bs._gd(siaqodb, t);//get dirty objects
                foreach (object obj in objects)
                {
                    changes.Add((SiaqodbOfflineEntity)obj);
                }
            }
            return changes;
        }
        public override byte[] GetServerBlob()
        {
#if SILVERLIGHT

            if (this.UseElevatedTrust)
            {
                string filePath = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
                FileStream phisicalFile = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] fullFile = new byte[phisicalFile.Length];
                phisicalFile.Read(fullFile, 0, fullFile.Length);
                phisicalFile.Close();
                return fullFile;
            }
            else
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                IsolatedStorageFileStream phisicalFile = new IsolatedStorageFileStream(siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc", FileMode.OpenOrCreate, FileAccess.ReadWrite, isf);
                byte[] fullFile = new byte[phisicalFile.Length];

                phisicalFile.Read(fullFile, 0, fullFile.Length);
                phisicalFile.Close();
                return fullFile;
            }
#elif MONODROID
            string filePath = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
            RandomAccessFile phisicalFile = new RandomAccessFile(filePath, "rw");
            byte[] fullFile = new byte[phisicalFile.Length()];
            phisicalFile.Read(fullFile, 0, fullFile.Length);
            phisicalFile.Close();
            return fullFile;
#else
            string filePath = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_"+CacheController.ControllerBehavior.ScopeName+".anc";
            FileStream phisicalFile = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] fullFile = new byte[phisicalFile.Length];
            phisicalFile.Read(fullFile, 0, fullFile.Length);
            phisicalFile.Close();
            return fullFile;
#endif


        }

        public override void OnChangeSetUploaded(Guid state, ChangeSetResponse response)
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

            siaqodb.StartBulkInsert(CacheController.ControllerBehavior.KnownTypes.ToArray());
            try
            {
                if (null != response.UpdatedItems && 0 != response.UpdatedItems.Count)
                {
                    foreach (var item in response.UpdatedItems)
                    {
                        var offlineEntity = (SiaqodbOfflineEntity)item;
                        this.SaveEntityByPK(offlineEntity);
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
                            this.SaveEntity(offlineEntity);

                        }
                    }
                }

                List<SiaqodbOfflineEntity> changesJustUploaded = this.GetChanges();
                foreach (SiaqodbOfflineEntity en in changesJustUploaded)
                {
                    if (en.IsTombstone)
                    {
                        en.IsDirty = false;
                        en.IsTombstone = false;
                        //reset flags first
                        siaqodb.StoreObjectBase(en);

                        siaqodb.DeleteBase(en);

                    }
                    else
                    {
                        en.IsDirty = false;
                        siaqodb.StoreObjectBase(en);
                    }
                }
            }
            finally
            {
                siaqodb.EndBulkInsert(CacheController.ControllerBehavior.KnownTypes.ToArray());
                siaqodb.Flush();
            }

            this.SaveAnchor(response.ServerBlob);

            this.OnSyncProgress(new SyncProgressEventArgs("Downloading changes from server..."));
            
        }
        
        public void SaveAnchor(byte[] anchor)
        {
#if SILVERLIGHT
            if (this.UseElevatedTrust)
            {
                string filePath = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
                FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                file.Seek(0, SeekOrigin.Begin);
                file.Write(anchor, 0, anchor.Length);
                file.Close();
            }
            else
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                IsolatedStorageFileStream phisicalFile = new IsolatedStorageFileStream(siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc", FileMode.OpenOrCreate, FileAccess.ReadWrite, isf);
                phisicalFile.Seek(0, SeekOrigin.Begin);
                phisicalFile.Write(anchor, 0, anchor.Length);
                phisicalFile.Close();
            }
#elif MONODROID
            string filePath = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
            RandomAccessFile file = new RandomAccessFile(filePath, "rw");
            file.Seek(0);
            file.Write(anchor, 0, anchor.Length);
            file.Close();
#else
            string filePath = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
            FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            file.Seek(0, SeekOrigin.Begin);
            file.Write(anchor, 0, anchor.Length);
            file.Close();
#endif
        }
        public bool DropAnchor()
        {
#if SILVERLIGHT
            try
            {
                if (this.UseElevatedTrust)
                {
                    string fileName = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                        return true;
                    }
                    return false;
                }
                else
                {
                    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                    string fileName = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
                    if (isf.FileExists(fileName))
                    {
                        isf.DeleteFile(fileName);
                        return true;
                    }
                    return false;
                
                }
            }
            catch (IsolatedStorageException ex)
            {
                throw ex;
            }
#elif MONODROID
            try
            {

                string fileName = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
                File file = new File(fileName);
                if (file.Exists())
                {
                    return file.Delete();
                    
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
#else
            try
            {

                string fileName = siaqodb.GetDBPath() + System.IO.Path.DirectorySeparatorChar + "anchor_" + CacheController.ControllerBehavior.ScopeName + ".anc";
               
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
#endif
        }
        public override void SaveChangeSet(ChangeSet changeSet)
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
            this.UpgradeTypeIdHash<T>();
        }
        int hash;
        private void UpgradeTypeIdHash<T>()
        {
            hash = typeof(T).FullName.GetHashCode();
            var tvers = this.siaqodb.Query<SyncProviderTypeVersion>().Where<SyncProviderTypeVersion>(x => x.TypeNameHash == hash).FirstOrDefault();
            if (tvers == null)
            {
                SyncProviderTypeVersion vers = new SyncProviderTypeVersion();
                vers.Version = 12;
                vers.TypeNameHash = hash;
                siaqodb.StoreObject(vers);
                

                IList<T> objs = siaqodb.LoadAll<T>();
                foreach (T obj in objs)
                {
                    SiaqodbOfflineEntity en = obj as SiaqodbOfflineEntity;
                    if (en != null)
                    {
                        if (en._idMeta != null)
                        {
                            if (en._idMetaHash == 0)
                            {
                                en._idMetaHash = en._idMeta.GetHashCode();
                                siaqodb.StoreObject(obj);
                            }
                        }
                    }

                }

            }

        }
        private void SaveDownloadedChanges(byte[] anchor, IEnumerable<SiaqodbOfflineEntity> entities)
        {
         
            siaqodb.StartBulkInsert(this.CacheController.ControllerBehavior.KnownTypes.ToArray());
            try
            {
                foreach (SiaqodbOfflineEntity en in entities)
                {
                    SaveEntity(en);
                   
                }
            }
            finally
            {
                siaqodb.EndBulkInsert(this.CacheController.ControllerBehavior.KnownTypes.ToArray());
                siaqodb.Flush();
            }
          
            SaveAnchor(anchor);
            
        }
        internal void SaveEntityByPK(SiaqodbOfflineEntity en)
        {
            List<string> primaryKeys = new List<string>();

            PropertyInfo[] pi = en.GetType().GetProperties(flags);
            foreach (PropertyInfo p in pi)
            {
#if SILVERLIGHT
                Type ty = typeof(Microsoft.Synchronization.ClientServices.KeyAttribute);

#else
                 Type ty = typeof(KeyAttribute);
#endif
                object[] pk = p.GetCustomAttributes(ty, false);

                if (pk.Length > 0)
                {
                    primaryKeys.Add(Sqo.Utilities.ExternalMetaHelper.GetBackingField(p));

                }
            }

            if (primaryKeys.Count > 0)
            {
                if (en.IsTombstone)
                {
                    siaqodb.DeleteObjectByBase(en, primaryKeys.ToArray());
                }
                else
                {
                    bool updated = siaqodb.UpdateObjectByBase(en, primaryKeys.ToArray());
                    
                }
            }
          

        }
        internal void SaveEntity(SiaqodbOfflineEntity en)
        {

            if (en.IsTombstone)
            {
                siaqodb.DeleteObjectByBase("_idMetaHash", en);
            }
            else
            {
                bool updated = siaqodb.UpdateObjectByBase("_idMetaHash", en);
                if (!updated) //insert
                {
                    siaqodb.StoreObjectBase(en);
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
            
            foreach (Type t in CacheController.ControllerBehavior.KnownTypes)
            {
                siaqodb.DropType(t);
            }
            bool dropped=this.DropAnchor();
        }
        internal void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress != null)
            {
                this.SyncProgress(this, args);
            }
        }
        public void SetHTTPRequestTimeout(int timeout)
        {
            CacheController.HTTPRequestTimeout = timeout;
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
