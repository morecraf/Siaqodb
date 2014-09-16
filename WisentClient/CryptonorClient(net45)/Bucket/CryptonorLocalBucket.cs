
using Sqo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Cryptonor.Queries;
using Cryptonor;

#if ASYNC
using System.Threading.Tasks;
#endif

#if WinRT
using Windows.Storage;
#endif
namespace CryptonorClient
{
    public class CryptonorLocalBucket:IBucket
    {
        CryptonorLocalDB localDB;
#if ASYNC
        private readonly AsyncLock _locker = new AsyncLock();
#endif
        public event EventHandler<PushCompletedEventArgs> PushCompleted;
        public event EventHandler<PullCompletedEventArgs> PullCompleted;
        public event EventHandler<SyncProgressEventArgs> SyncProgress;
      
        string uri;
        string dbName;
        string appKey;
        string secretKey;
        public string BucketName { get; set; }
        public CryptonorLocalBucket(string uri,string dbName,string bucketName,string localFolder,string appKey,string secretKey)
        {
            string fullPath = localFolder + Path.DirectorySeparatorChar + bucketName;
#if WinRT
            StorageFolder folder = StorageFolder.GetFolderFromPathAsync(fullPath).AsTask().Result;
            localDB = new CryptonorLocalDB(folder);
#else
             localDB = new CryptonorLocalDB(fullPath);
#endif
            this.BucketName = bucketName;
            this.uri = uri;
            this.dbName = dbName;
            this.secretKey = secretKey;
            this.appKey = appKey;
            DownloadBatchSize = 10000;
        }
        public int DownloadBatchSize { get; set; }

        #if NON_ASYNC
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CryptonorObject Get(string key)
        {
            return localDB.Load(key);
        }
#endif

#if ASYNC
    public async Task<CryptonorObject> GetAsync(string key)
        {
            return await localDB.LoadAsync(key).LibAwait();
        }
#endif
#if NON_ASYNC
        public T Get<T>(string key)
        {
            CryptonorObject obj = localDB.Load(key);
            return obj.GetValue<T>();
        }
#endif

#if ASYNC
    public async Task<T> GetAsync<T>(string key)
        {
            CryptonorObject obj = await localDB.LoadAsync(key).LibAwait();
            return obj.GetValue<T>();
        }
#endif
#if NON_ASYNC
        public  CryptonorResultSet Get(CryptonorQuery query)
        {
            var objects =  localDB.Load(query);
            return new CryptonorResultSet
            {
                Objects = objects,
                Count = objects.Count
            };

        }
#endif

#if ASYNC
      public async Task<CryptonorResultSet> GetAsync(CryptonorQuery query)
        {
            var objects = await localDB.LoadAsync(query).LibAwait();
            return new CryptonorResultSet
            {
                Objects = objects,
                Count = objects.Count
            };

        }
#endif
#if NON_ASYNC
        public CryptonorResultSet GetAll()
        {

            var all =  localDB.LoadAll();

            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }
#endif

#if ASYNC
  public async Task<CryptonorResultSet> GetAllAsync()
        {

            var all = await localDB.LoadAllAsync().LibAwait();

            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }

#endif
#if NON_ASYNC
        public CryptonorResultSet GetAll(int skip, int limit)
        {

            var all =  localDB.LoadAll(skip, limit);

            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }
#endif

#if ASYNC
   public async Task<CryptonorResultSet> GetAllAsync(int skip,int limit)
        {

            var all = await localDB.LoadAllAsync(skip, limit).LibAwait();
           
            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }
#endif

        #if NON_ASYNC
        public void Store(CryptonorObject obj)
        {
             localDB.Store(obj);
        }
#endif

#if ASYNC
   public async Task StoreAsync(CryptonorObject obj)
        {
            await localDB.StoreAsync(obj).LibAwait();
        }
#endif
        #if NON_ASYNC
        public void Store(string key, object obj)
        {
             this.Store(key, obj, null);
        }
#endif

#if ASYNC
   public async Task StoreAsync(string key, object obj)
        {
            await this.StoreAsync(key, obj, null).LibAwait();
        }
#endif
        #if NON_ASYNC
        public void Store(string key, object obj, Dictionary<string, object> tags)
        {
            CryptonorObject cryObject = new CryptonorObject();
            cryObject.Key = key;
            cryObject.IsDirty = true;
            cryObject.SetValue(obj);

            if (tags != null)
            {
                foreach (string tagName in tags.Keys)
                {
                    cryObject.SetTag(tagName, tags[tagName]);
                }
            }

           Store(cryObject);
        }
#endif

#if ASYNC
 public async Task StoreAsync(string key, object obj, Dictionary<string, object> tags)
        {
            CryptonorObject cryObject = new CryptonorObject();
            cryObject.Key = key;
            cryObject.IsDirty = true;
            cryObject.SetValue(obj);

            if (tags != null)
            {
                foreach (string tagName in tags.Keys)
                {
                    cryObject.SetTag(tagName, tags[tagName]);
                }
            }

            await this.StoreAsync(cryObject).LibAwait();
        }
#endif
        #if NON_ASYNC
        public void Store(string key, object obj, object tags = null)
        {
            Dictionary<string, object> tags_Dict = null;
            if (tags != null)
            {
                tags_Dict = new Dictionary<string, object>();
                object o = tags;
                Type tagsType = o.GetType();

                PropertyInfo[] pi = tagsType.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    tags_Dict.Add(p.Name, p.GetValue(o,null));
                }
            }

            Store(key, obj, tags_Dict);
        }
#endif

#if ASYNC
  public async Task StoreAsync(string key, object obj, object tags = null)
        {
            Dictionary<string, object> tags_Dict = null;
            if (tags != null)
            {
                tags_Dict = new Dictionary<string, object>();
                object o = tags;
                Type tagsType = o.GetType();

                PropertyInfo[] pi = tagsType.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    tags_Dict.Add(p.Name, p.GetValue(o));
                }
            }

            await this.StoreAsync(key, obj, tags_Dict).LibAwait();
        }
#endif
        #if NON_ASYNC
        public void Delete(string key)
        {
            CryptonorObject cobj =  localDB.Load(key);
            if (cobj != null)
            {
                localDB.Delete(cobj);
            }
        }
#endif

#if ASYNC
      public async Task DeleteAsync(string key)
        {
            CryptonorObject cobj = await localDB.LoadAsync(key).LibAwait();
            if (cobj != null)
            {
                await localDB.DeleteAsync(cobj).LibAwait();
            }
        }
#endif
#if NON_ASYNC
        public void Delete(CryptonorObject obj)
        {
            localDB.Delete(obj);
        }
#endif

#if ASYNC
        public async Task DeleteAsync(CryptonorObject obj)
        {
            await localDB.DeleteAsync(obj).LibAwait();
        }
#endif
        #if NON_ASYNC
        public 
#else 
        internal
#endif
        CryptonorBatchResponse StoreBatch(IList<CryptonorObject> objs)
        {
            localDB.StoreBatch(objs);
            //TODO create better resposne
            return new CryptonorBatchResponse() { IsSuccess = true };
        }

#if ASYNC
      public async Task<CryptonorBatchResponse> StoreBatchAsync(IList<CryptonorObject> objs)
        {
            await localDB.StoreBatchAsync(objs).LibAwait();
            //TODO create better resposne
            return new CryptonorBatchResponse() { IsSuccess = true };
        }
#endif
        #if NON_ASYNC
        public void Push()
        {
            var syncStatistics = new PushStatistics();
            syncStatistics.StartTime = DateTime.Now;
            Exception error = null;
            List<Conflict> conflicts = null;
            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Push operation started..."));
                this.OnSyncProgress(new SyncProgressEventArgs("Get local changes..."));
                CryptonorChangeSet changeSet = localDB.GetChangeSet();
                if ((changeSet.ChangedObjects != null && changeSet.ChangedObjects.Count > 0) ||
                    (changeSet.DeletedObjects != null && changeSet.DeletedObjects.Count > 0))
                {
                    this.OnSyncProgress(new SyncProgressEventArgs("Uploading local changes..."));
                    CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName, this.appKey, this.secretKey);
                    var response =  httpClient.Put(BucketName, changeSet);
                    this.OnSyncProgress(new SyncProgressEventArgs("Upload finished, build the result..."));

                    var conflictResponses = response.WriteResponses.Where(a => string.Compare(a.Error, "conflict", StringComparison.OrdinalIgnoreCase) == 0);
                    foreach (var conflictR in conflictResponses)
                    {

                        if (conflicts == null)
                            conflicts = new List<Conflict>();
                        Conflict cf = new Conflict() { Key = conflictR.Key, Version = conflictR.Version, Description = conflictR.ErrorDesc };
                        conflicts.Add(cf);
                    }
                    if (changeSet.ChangedObjects != null)
                    {
                        syncStatistics.TotalChangesUploads = changeSet.ChangedObjects.Count;
                    }
                    if (changeSet.DeletedObjects != null)
                    {
                        syncStatistics.TotalDeletedUploads = changeSet.DeletedObjects.Count;
                    }
                    if (conflicts != null)
                    {
                        syncStatistics.TotalConflicted = conflicts.Count;
                    }
                    localDB.ClearSyncMetadata();
                }

                this.OnSyncProgress(new SyncProgressEventArgs("Push finshed!"));

            }
            catch (Exception err)
            {
                error = err;
            }
            syncStatistics.EndTime = DateTime.Now;
            OnPushCompleted(new PushCompletedEventArgs(error, syncStatistics, conflicts));
        }
#endif

#if ASYNC
    public async Task PushAsync()
        {

            await this._locker.LockAsync();
            var syncStatistics = new PushStatistics();
            syncStatistics.StartTime = DateTime.Now;
            Exception error = null;
            List<Conflict> conflicts = null;
            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Push operation started..."));
                this.OnSyncProgress(new SyncProgressEventArgs("Get local changes..."));
                CryptonorChangeSet changeSet = await this.localDB.GetChangeSetAsync().LibAwait();
                if ((changeSet.ChangedObjects != null && changeSet.ChangedObjects.Count > 0) ||
                    (changeSet.DeletedObjects != null && changeSet.DeletedObjects.Count > 0))
                {
                    this.OnSyncProgress(new SyncProgressEventArgs("Uploading local changes..."));
                    CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName, this.appKey, this.secretKey);
                    var response = await httpClient.PutAsync(this.BucketName, changeSet).LibAwait();
                    this.OnSyncProgress(new SyncProgressEventArgs("Upload finished, build the result..."));

                    var conflictResponses = response.WriteResponses.Where(a => string.Compare(a.Error, "conflict", StringComparison.OrdinalIgnoreCase) == 0);
                    foreach (var conflictR in conflictResponses)
                    {
                       
                        if (conflicts == null)
                            conflicts = new List<Conflict>();
                        Conflict cf = new Conflict() { Key = conflictR.Key, Version = conflictR.Version, Description = conflictR.ErrorDesc };
                        conflicts.Add(cf);
                    }
                    if (changeSet.ChangedObjects != null)
                    {
                        syncStatistics.TotalChangesUploads = changeSet.ChangedObjects.Count;
                    }
                    if (changeSet.DeletedObjects != null)
                    {
                        syncStatistics.TotalDeletedUploads = changeSet.DeletedObjects.Count;
                    }
                    if (conflicts != null)
                    {
                        syncStatistics.TotalConflicted = conflicts.Count;
                    }
                    await this.localDB.ClearSyncMetadataAsync();
                }

                this.OnSyncProgress(new SyncProgressEventArgs("Push finshed!"));
               
            }
            catch (Exception err)
            {
                error = err;
            }
            finally
            {
                this._locker.Release();
            }
            syncStatistics.EndTime = DateTime.Now;
            OnPushCompleted(new PushCompletedEventArgs(error, syncStatistics, conflicts));
        }
       
#endif
        #if NON_ASYNC
        public void Pull()
        {
            this.Pull(null);

        }
#endif

#if ASYNC
        public async Task PullAsync()
        {
            await this.PullAsync(null).LibAwait();

        }
#endif

        #if NON_ASYNC
        public void Pull(CryptonorQuery query)
        {

            Push();

            var syncStatistics = new PullStatistics();
            syncStatistics.StartTime = DateTime.Now;
            Exception error = null;
            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Pull operation started..."));

                CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName, this.appKey, this.secretKey);
                string anchor = localDB.GetAnchor();

                int remainLimit = 1;
                int nrBatch = 1;
                CryptonorChangeSet downloadedItems = null;
                while (remainLimit > 0)
                {
                    remainLimit = 0;
                    this.OnSyncProgress(new SyncProgressEventArgs("Downloading batch #" + nrBatch + " ..."));
                    downloadedItems = DownloadChanges(httpClient, anchor, query);
                    this.OnSyncProgress(new SyncProgressEventArgs("Batch #" + nrBatch + " downloaded, store items locally ..."));

                    if (downloadedItems != null)
                    {
                        if (downloadedItems.ChangedObjects != null)
                        {
                            DateTime start = DateTime.Now;
                            this.StoreBatch(downloadedItems.ChangedObjects);
                            string elapsed = (DateTime.Now - start).ToString();
                            remainLimit += downloadedItems.ChangedObjects.Count;
                            syncStatistics.TotalChangesDownloads += downloadedItems.ChangedObjects.Count;
                        }
                        if (downloadedItems.DeletedObjects != null)
                        {
                            foreach (DeletedObject delObj in downloadedItems.DeletedObjects)
                            {
                                Delete(delObj.Key);
                            }
                            remainLimit += downloadedItems.DeletedObjects.Count;
                            syncStatistics.TotalDeletedDownloads += downloadedItems.DeletedObjects.Count;
                        }
                        anchor = downloadedItems.Anchor;
                        if (!string.IsNullOrEmpty(anchor))
                        {
                            localDB.StoreAnchor(anchor);
                        }
                        this.OnSyncProgress(new SyncProgressEventArgs("Items of batch " + nrBatch + "stored locally ..."));

                    }
                    nrBatch++;
                }
                this.OnSyncProgress(new SyncProgressEventArgs("Push finshed!"));
            }
            catch (Exception ex)
            {
                error = ex;
            }
            syncStatistics.EndTime = DateTime.Now;
            OnPullCompleted(new PullCompletedEventArgs(error, syncStatistics));

        }
#endif

#if ASYNC

        public async Task PullAsync(CryptonorQuery query)
        {

            await this.PushAsync().LibAwait();

            await this._locker.LockAsync();
            var syncStatistics = new PullStatistics();
            syncStatistics.StartTime = DateTime.Now;
            Exception error = null;
            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Pull operation started..."));

                CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName, this.appKey, this.secretKey);
                string anchor = await localDB.GetAnchorAsync().LibAwait();

                int remainLimit = 1;
                int nrBatch = 1;
                CryptonorChangeSet downloadedItems = null;
                while (remainLimit > 0)
                {
                    remainLimit = 0;
                    this.OnSyncProgress(new SyncProgressEventArgs("Downloading batch #" + nrBatch + " ..."));
                    downloadedItems = await DownloadChangesAsync(httpClient, anchor, query).LibAwait();
                    this.OnSyncProgress(new SyncProgressEventArgs("Batch #" + nrBatch + " downloaded, store items locally ..."));

                    if (downloadedItems != null)
                    {
                        if (downloadedItems.ChangedObjects != null)
                        {
                            DateTime start = DateTime.Now;
                            this.StoreBatch(downloadedItems.ChangedObjects);
                            string elapsed = (DateTime.Now - start).ToString();
                            remainLimit += downloadedItems.ChangedObjects.Count;
                            syncStatistics.TotalChangesDownloads += downloadedItems.ChangedObjects.Count;
                        }
                        if (downloadedItems.DeletedObjects != null)
                        {
                            foreach (DeletedObject delObj in downloadedItems.DeletedObjects)
                            {
                                await this.DeleteAsync(delObj.Key).LibAwait();
                            }
                            remainLimit += downloadedItems.DeletedObjects.Count;
                            syncStatistics.TotalDeletedDownloads += downloadedItems.DeletedObjects.Count;
                        }
                        anchor = downloadedItems.Anchor;
                        if (!string.IsNullOrEmpty(anchor))
                        {
                            await localDB.StoreAnchorAsync(anchor).LibAwait();
                        }
                        this.OnSyncProgress(new SyncProgressEventArgs("Items of batch " + nrBatch + "stored locally ..."));

                    }
                    nrBatch++;
                }
                this.OnSyncProgress(new SyncProgressEventArgs("Push finshed!"));
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                this._locker.Release();
            }
            syncStatistics.EndTime = DateTime.Now;
            OnPullCompleted(new PullCompletedEventArgs(error, syncStatistics));
           
        }
#endif
        #if NON_ASYNC
        private CryptonorChangeSet DownloadChanges(CryptonorHttpClient httpClient, string anchor, CryptonorQuery query)
        {
            CryptonorChangeSet changes = null;
            if (query == null)
            {
                changes = httpClient.GetChanges(this.BucketName, DownloadBatchSize, anchor);
            }
            else
            {
                changes = httpClient.GetChanges(this.BucketName, query, DownloadBatchSize, anchor);
            }
            return changes;
        }
#endif

#if ASYNC
  private async Task<CryptonorChangeSet> DownloadChangesAsync(CryptonorHttpClient httpClient,string anchor,CryptonorQuery query)
        {
            CryptonorChangeSet changes = null;
            if (query == null)
            {
                changes = await httpClient.GetChangesAsync(this.BucketName, DownloadBatchSize, anchor).LibAwait();
            }
            else
            {
                changes = await httpClient.GetChangesAsync(this.BucketName, query, DownloadBatchSize, anchor).LibAwait();
            }
            return changes;
        }
#endif


        internal void OnPullCompleted(PullCompletedEventArgs args)
        {
            if (this.PullCompleted != null)
            {
                this.PullCompleted(this, args);
            }
        }

     internal void OnPushCompleted(PushCompletedEventArgs args)
        {
            if (this.PushCompleted != null)
            {
                this.PushCompleted(this, args);
            }
        }


       internal void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress != null)
            {
                this.SyncProgress(this, args);
            }
        }

        #if NON_ASYNC
       public void Purge()
       {
            this.Purge(true);
       }
#endif

#if ASYNC
       public async Task PurgeAsync()
        {
            await this.PurgeAsync(true).LibAwait();
        }
#endif
        #if NON_ASYNC
       public void Purge(bool pushFirst)
       {
           if (pushFirst)
           {
                this.Push();
               
           }
           this.localDB.Purge();
       }
#endif

#if ASYNC
        public async Task PurgeAsync(bool pushFirst)
        {
            if (pushFirst)
            {
                await this.PushAsync().LibAwait();
                
            }
        this.localDB.Purge();
        }


#endif



    }
   
   
   
}
