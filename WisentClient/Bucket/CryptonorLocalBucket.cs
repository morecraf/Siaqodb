
using Sqo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cryptonor.Queries;
using Cryptonor;
namespace CryptonorClient
{
    public class CryptonorLocalBucket:IBucket
    {
        CryptonorLocalDB localDB;
        private readonly AsyncLock _locker = new AsyncLock();
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
            localDB = new CryptonorLocalDB(localFolder+Path.DirectorySeparatorChar+bucketName);
            this.BucketName = bucketName;
            this.uri = uri;
            this.dbName = dbName;
            this.secretKey = secretKey;
            this.appKey = appKey;
            DownloadBatchSize = 1000;
        }
        public int DownloadBatchSize { get; set; }
       
        public async Task<CryptonorObject> Get(string key)
        {
            return await localDB.Load(key);
        }

        public async Task<T> Get<T>(string key)
        {
            CryptonorObject obj= await localDB.Load(key);
            return obj.GetValue<T>();
        }
        public async Task<CryptonorResultSet> Get(CryptonorQuery query)
        {
            var objects = await localDB.Load(query);
            return new CryptonorResultSet
            {
                Objects = objects,
                Count = objects.Count
            };

        }

        public async Task<CryptonorResultSet> GetAll()
        {

            var all= await localDB.LoadAll();

            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }

        public async Task<CryptonorResultSet> GetAll(int skip,int limit)
        {

            var all = await localDB.LoadAll(skip, limit);
           
            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }
        public async Task Store(CryptonorObject obj)
        {
            await localDB.Store(obj);
        }

        public async Task Store(string key, object obj)
        {
            await this.Store(key, obj, null);
        }

        public async Task Store(string key, object obj, Dictionary<string, object> tags)
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

            await this.Store(cryObject);
        }

        public async Task Store(string key, object obj, object tags = null)
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

            await this.Store(key, obj, tags_Dict);
        }

        public async Task Delete(string key)
        {
            CryptonorObject cobj = await localDB.Load(key);
            if (cobj != null)
            {
                await localDB.Delete(cobj);
            }
        }
        public async Task Delete(CryptonorObject obj)
        {
            await localDB.Delete(obj);
        }
        public async Task<CryptonorBatchResponse> StoreBatch(IList<CryptonorObject> objs)
        {
            await localDB.StoreBatch(objs);
            //TODO create better resposne
            return new CryptonorBatchResponse() { IsSuccess = true };
        }
        public async Task Push()
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
                CryptonorChangeSet changeSet= await this.localDB.GetChangeSet();
                if ((changeSet.ChangedObjects != null && changeSet.ChangedObjects.Count > 0) ||
                    (changeSet.DeletedObjects != null && changeSet.DeletedObjects.Count > 0))
                {
                    this.OnSyncProgress(new SyncProgressEventArgs("Uploading local changes..."));
                    CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName, this.appKey, this.secretKey);
                    var response= await httpClient.Put(this.BucketName, changeSet);
                    this.OnSyncProgress(new SyncProgressEventArgs("Upload finished, build the result..."));

                    var conflictResponses = response.WriteResponses.Where(a => string.Compare(a.Error, "conflict", true) == 0);
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
                    await this.localDB.ClearSyncMetadata();
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
        
       

        public async Task Pull()
        {
            await this.Pull(null);

        }
        public async Task Pull(CryptonorQuery query)
        {

            await this.Push();

            await this._locker.LockAsync();
            var syncStatistics = new PullStatistics();
            syncStatistics.StartTime = DateTime.Now;
            Exception error = null;
            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Pull operation started..."));

                CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName, this.appKey, this.secretKey);
                string anchor = await localDB.GetAnchor();

                int remainLimit = 1;
                int nrBatch = 1;
                CryptonorChangeSet downloadedItems = null;
                while (remainLimit > 0)
                {
                    remainLimit = 0;
                    this.OnSyncProgress(new SyncProgressEventArgs("Downloading batch #" + nrBatch + " ..."));
                    downloadedItems = await DownloadChanges(httpClient, anchor, query);
                    this.OnSyncProgress(new SyncProgressEventArgs("Batch #" + nrBatch + " downloaded, store items locally ..."));

                    if (downloadedItems != null)
                    {
                        if (downloadedItems.ChangedObjects != null)
                        {
                            DateTime start = DateTime.Now;
                            await this.StoreBatch(downloadedItems.ChangedObjects);
                            string elapsed = (DateTime.Now - start).ToString();
                            remainLimit += downloadedItems.ChangedObjects.Count;
                            syncStatistics.TotalChangesDownloads += downloadedItems.ChangedObjects.Count;
                        }
                        if (downloadedItems.DeletedObjects != null)
                        {
                            foreach (DeletedObject delObj in downloadedItems.DeletedObjects)
                            {
                                await this.Delete(delObj.Key);
                            }
                            remainLimit += downloadedItems.DeletedObjects.Count;
                            syncStatistics.TotalDeletedDownloads += downloadedItems.DeletedObjects.Count;
                        }
                        anchor = downloadedItems.Anchor;
                        if (!string.IsNullOrEmpty(anchor))
                        {
                            await localDB.StoreAnchor(anchor);
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
        private async Task<CryptonorChangeSet> DownloadChanges(CryptonorHttpClient httpClient,string anchor,CryptonorQuery query)
        {
            CryptonorChangeSet changes = null;
            if (query == null)
            {
                changes =await httpClient.GetChanges(this.BucketName, DownloadBatchSize, anchor);
            }
            else
            {
                changes = await httpClient.GetChanges(this.BucketName, query, DownloadBatchSize, anchor);
            }
            return changes;
        }
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
        public async Task Purge()
        {
            await this.Purge(true);
        }
        public async Task Purge(bool pushFirst)
        {
            if (pushFirst)
            {
                await this.Push();
                this.localDB.Purge();
            }
        }



       
    }
   
   
   
}
