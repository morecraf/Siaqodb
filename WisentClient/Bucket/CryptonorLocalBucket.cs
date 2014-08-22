
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
        public event EventHandler<SyncCompletedEventArgs> SyncCompleted;
        public event EventHandler<SyncProgressEventArgs> SyncProgress;
        SyncStatistics syncStatistics;
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
        }
       
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
            await localDB.Delete(cobj);
        }
        public async Task Delete(CryptonorObject obj)
        {
            await localDB.Delete(obj);
        }
        public async Task<CryptonorBatchResponse> StoreBatch(IList<CryptonorObject> objs)
        {
            foreach (CryptonorObject cobj in objs)
                await localDB.Store(cobj);
            //TODO create better resposne
            return new CryptonorBatchResponse() { IsSuccess = true };
        }
        public async Task Push()
        {

            await this._locker.LockAsync();
            try
            {
                this.syncStatistics = new SyncStatistics();
                this.syncStatistics.StartTime = DateTime.Now;
                this.OnSyncProgress(new SyncProgressEventArgs("Synchronization started..."));
                CryptonorChangeSet changeSet= await this.localDB.GetChangeSet();
                if ((changeSet.ChangedObjects != null && changeSet.ChangedObjects.Count > 0) ||
                    (changeSet.DeletedObjects != null && changeSet.DeletedObjects.Count > 0))
                {
                    CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName, this.appKey, this.secretKey);
                    var response= await httpClient.Put(this.BucketName, changeSet);
                    syncStatistics.TotalChangesUploads = changeSet.ChangedObjects.Count;

                    await this.localDB.ClearSyncMetadata();
                }

                this.OnSyncProgress(new SyncProgressEventArgs("Synchronization finshed!"));
                this.syncStatistics.EndTime = DateTime.Now;
                this.OnSyncCompleted(new SyncCompletedEventArgs(null, this.syncStatistics));
            }
            catch (Exception error)
            {
                this.syncStatistics.EndTime = DateTime.Now;
                this.OnSyncCompleted(new SyncCompletedEventArgs(error, this.syncStatistics));
            }
            finally
            {
                this._locker.Release();
            }
        }
        
        internal void OnSyncCompleted(SyncCompletedEventArgs args)
        {
            if (this.SyncCompleted != null)
            {
                this.SyncCompleted(this, args);
            }
        }
        internal void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress != null)
            {
                this.SyncProgress(this, args);
            }
        }

        public async Task Pull()
        {
            await this.Pull(0);//default

        }
        public async Task Pull(int limit)
        {
            await this.Push();
            CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName,this.appKey,this.secretKey);
            var downloadedItems = await httpClient.Get(this.BucketName,0,limit);
            if (downloadedItems != null)
            {
                await this.StoreBatch(downloadedItems.Objects);
                int remainLimit = limit - downloadedItems.Count;
                int skip = downloadedItems.Count;
                while (remainLimit > 0)
                {
                    downloadedItems = await httpClient.Get(this.BucketName,skip, remainLimit);
                    if (downloadedItems != null)
                    {
                        await this.StoreBatch(downloadedItems.Objects);
                       
                        remainLimit -= downloadedItems.Count;
                        skip += downloadedItems.Count;
                    }
                    else
                    {
                        remainLimit = 0;
                    }

                }
            }

        }
        public async Task Pull(CryptonorQuery query)
        {
            
            await this.Push();
            CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName,this.appKey,this.secretKey);
            var downloadedItems =   (await httpClient.GetByTag(this.BucketName, query));
       
            if (downloadedItems != null)
            {
                await this.StoreBatch(downloadedItems.Objects);
               
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
    public class SyncProgressEventArgs : EventArgs
    {
        public string Message
        {
            get;
            private set;
        }
        public SyncProgressEventArgs(string message)
        {
            this.Message = message;
        }
    }
    public class SyncCompletedEventArgs : EventArgs
    {
        public Exception Error
        {
            get;
            private set;
        }
        public SyncStatistics Statistics
        {
            get;
            private set;
        }
        public SyncCompletedEventArgs(Exception error, SyncStatistics statistics)
        {
            this.Error = error;
            this.Statistics = statistics;
        }
    }
    public class SyncStatistics
    {
        public DateTime StartTime
        {
            get;
            internal set;
        }
        public DateTime EndTime
        {
            get;
            internal set;
        }
        public int TotalUploads
        {
            get
            {
                return this.TotalChangesUploads + this.TotalDeletedUploads;
            }
        }
        public int TotalDeletedUploads
        {
            get;
            internal set;
        }
      
        public int TotalChangesUploads
        {
            get;
            internal set;
        }
        public int TotalDownloads
        {
            get;
            internal set;
        }
    }
}
