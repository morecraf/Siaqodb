using Sqo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        public string BucketName { get; set; }
        public CryptonorLocalBucket(string bucketName,string localFolder,string uri,string dbName)
        {
            localDB = new CryptonorLocalDB(localFolder+Path.DirectorySeparatorChar+bucketName);
            this.BucketName = bucketName;
            this.uri = uri;
            this.dbName = dbName;
        }
        public Sqo.ISqoQuery<T> Cast<T>()
        {
            return localDB.Cast<T>();
        }
        public Sqo.ISqoQuery<Sqo.CryptonorObject> Query()
        {
            return localDB.Query();
        }
        public ISqoQuery<CryptonorObject> Query(long continuationToken)
        {
            int lastOID=(int)continuationToken;
            Expression<Func<CryptonorObject, bool>> predicate = cobj => cobj.OID > lastOID;
            SqoQuery<CryptonorObject> query = localDB.Query() as SqoQuery<CryptonorObject>;
            query.Expression = predicate;
            return query;
        }

        public async Task<Sqo.CryptonorObject> Get(string key)
        {
            return await localDB.Load(key);
        }

        public async Task<T> Get<T>(string key)
        {
            CryptonorObject obj= await localDB.Load(key);
            return obj.GetValue<T>();
        }
        public async Task<CryptonorResultSet> Get(System.Linq.Expressions.Expression expression)
        {
            var all= await localDB.Load(expression);
            var lastOne = all.OrderBy(a=>a.OID).LastOrDefault();
            long contiToken=0;
            if(lastOne!=null)
                 contiToken=lastOne.OID;
            return new CryptonorResultSet { Objects = all, Count = all.Count, ContinuationToken = contiToken };
        }
        public async Task<CryptonorResultSet> Get(System.Linq.Expressions.Expression expression, long continuationToken)
        {
            return await Get(expression);
        }
        public async Task<CryptonorResultSet> GetAll()
        {

            var all= await localDB.LoadAll();

            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }

        public async Task<CryptonorResultSet> GetAll(int limit,long continuationToken)
        {
            int lastOID = (int)continuationToken;
            var all=await localDB.Query().Where( cobj => cobj.OID > lastOID).Take(limit).ToListAsync();
            var lastOne = all.OrderBy(a=>a.OID).LastOrDefault();
            long contiToken=0;
            if(lastOne!=null)
                 contiToken=lastOne.OID;
            return new CryptonorResultSet { Objects = all, Count = all.Count, ContinuationToken = contiToken };
        }

      

        public async Task Store(Sqo.CryptonorObject obj)
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
            CryptonorObject cobj = await localDB.Query().Where(a => a.Key == key).FirstOrDefaultAsync();
            await localDB.Delete(cobj);
        }
        public async Task Delete(CryptonorObject obj)
        {
            await localDB.Delete(obj);
        }
        public async Task StoreBatch(IList<CryptonorObject> objs)
        {
            foreach (CryptonorObject cobj in objs)
                await localDB.Store(cobj);
        }
        public async Task Push()
        {

            await this._locker.LockAsync();
            try
            {
                this.syncStatistics = new SyncStatistics();
                this.syncStatistics.StartTime = DateTime.Now;
                this.OnSyncProgress(new SyncProgressEventArgs("Synchronization started..."));
                ChangeSet changeSet= await this.localDB.GetChangeSet();
                CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName);
                await httpClient.Put(this.BucketName, changeSet.ChangedObjects);
                syncStatistics.TotalChangesUploads = changeSet.ChangedObjects.Count;
                foreach (DeletedObject delObj in changeSet.DeletedObjects)
                {
                    await httpClient.Delete(this.BucketName, delObj.Key);
                }
                syncStatistics.TotalChangesUploads = changeSet.DeletedObjects.Count;
                await this.localDB.ClearSyncMetadata();

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
            CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName);
            var downloadedItems = await httpClient.Get(this.BucketName,limit,0);
            if (downloadedItems != null)
            {
                await this.StoreBatch(downloadedItems.Objects);
                long continuationToken = downloadedItems.ContinuationToken;
                int remainLimit = limit - downloadedItems.Count;
                while (remainLimit > 0)
                {
                    downloadedItems = await httpClient.Get(this.BucketName, remainLimit, continuationToken);
                    if (downloadedItems != null)
                    {
                        await this.StoreBatch(downloadedItems.Objects);
                        continuationToken = downloadedItems.ContinuationToken;
                        remainLimit -= downloadedItems.Count;
                    }
                    else
                    {
                        remainLimit = 0;
                    }

                }
            }

        }
        public async Task Pull(Expression expression, int limit)
        {
            QueryTranslator t = new QueryTranslator();
            List<Criteria> where = t.Translate(expression);
            if (where.Where(a => a.OperationType == Criteria.Equal).FirstOrDefault() == null)
                throw new Exception("At least one EQUAL operation must be set");

            await this.Push();
            CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName);
            var downloadedItems =   (await httpClient.GetByTag(this.BucketName, new QueryObject { Filter = where,Limit=limit, ContinuationToken = 0 }));
       
            if (downloadedItems != null)
            {
                await this.StoreBatch(downloadedItems.Objects);
                long continuationToken = downloadedItems.ContinuationToken;
                int remainLimit = limit - downloadedItems.Count;
                while (remainLimit > 0)
                {
                    downloadedItems = (await httpClient.GetByTag(this.BucketName, new QueryObject { Filter = where, Limit = remainLimit, ContinuationToken = continuationToken }));
                    if (downloadedItems != null)
                    {
                        await this.StoreBatch(downloadedItems.Objects);
                        continuationToken = downloadedItems.ContinuationToken;
                        remainLimit -= downloadedItems.Count;
                    }
                    else
                    {
                        remainLimit = 0;
                    }

                }
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
