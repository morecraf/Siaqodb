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
       
        public async Task<Sqo.CryptonorObject> Get(string key)
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
            Expression<Func<CryptonorObject, bool>> expr = this.GetFilterExpression(query);
            if (expr == null)
            {
                expr = a => a.OID > 0;
            }
            IList<CryptonorObject> objects = null;

            if (query.Skip != null && query.Limit != null)
            {
                int skip = query.Skip.Value;
                int limit = query.Limit.Value;
                objects = await localDB.Query().Where(expr).Skip(skip).Take(limit).ToListAsync();
            }
            else if (query.Skip == null && query.Limit != null)
            {

                int limit = query.Limit.Value;
                objects = await localDB.Query().Where(expr).Take(limit).ToListAsync();
            }
            else if (query.Skip != null && query.Limit == null)
            {
                int skip = query.Skip.Value;

                objects = await localDB.Query().Where(expr).Skip(skip).ToListAsync();
            }
            else
            {
                objects = await localDB.Query().Where(expr).ToListAsync();
            }
            return new CryptonorResultSet
            {
                Objects = objects,
                Count = objects.Count
            };

        }

        private Expression<Func<CryptonorObject, bool>> GetFilterExpression(CryptonorQuery query)
        {
            //TODO: this is ugly refactor me
            Expression<Func<CryptonorObject, bool>> expr=null;
            if (query.Value != null && string.Compare( query.TagName,"key",true)==0)
            {
                expr = a => a.Key == query.Value.ToString();
            }
            else if (query.Value != null)
            {
                if (query.Value.GetType() == typeof(int) )
                {
                    expr = a => a.GetTag<int>(query.TagName) == (int)query.Value;
                }
                else if (query.Value.GetType() == typeof(long))
                {
                    expr = a => a.GetTag<long>(query.TagName) == (long)query.Value;
                }
                else if (query.Value.GetType() == typeof(string))
                {
                    expr = a => a.GetTag<string>(query.TagName) == (string)query.Value;
                }
                else if (query.Value.GetType() == typeof(DateTime))
                {
                    expr = a => a.GetTag<DateTime>(query.TagName) == (DateTime)query.Value;
                }
                else if (query.Value.GetType() == typeof(double))
                {
                    expr = a => a.GetTag<double>(query.TagName) == (double)query.Value;
                }
                else if (query.Value.GetType() == typeof(float))
                {
                    expr = a => a.GetTag<float>(query.TagName) == (float)query.Value;
                }
            }
            else if (query.Start != null && query.End != null && string.Compare(query.TagName, "key", true) == 0)
            {
                throw new NotSupportedException();
            }
            else if (query.Start != null && query.End != null )
            {
                if (query.Value.GetType() == typeof(int))
                {
                    expr = a => a.GetTag<int>(query.TagName) >= (int)query.Start && a.GetTag<int>(query.TagName) <= (int)query.End;
                }
                else if (query.Value.GetType() == typeof(long))
                {
                    expr = a => a.GetTag<long>(query.TagName) >= (long)query.Start && a.GetTag<long>(query.TagName) <= (long)query.End;
       
                }
                else if (query.Value.GetType() == typeof(string))
                {
                    throw new NotSupportedException();
                }
                else if (query.Value.GetType() == typeof(DateTime))
                {
                    expr = a => a.GetTag<DateTime>(query.TagName) >= (DateTime)query.Start && a.GetTag<DateTime>(query.TagName) <= (DateTime)query.End;
       
                }
                else if (query.Value.GetType() == typeof(double))
                {
                    expr = a => a.GetTag<double>(query.TagName) >= (double)query.Start && a.GetTag<double>(query.TagName) <= (double)query.End;
       
                }
                else if (query.Value.GetType() == typeof(float))
                {

                    expr = a => a.GetTag<float>(query.TagName) >= (float)query.Start && a.GetTag<float>(query.TagName) <= (float)query.End;

                }
            }
            else if (query.Start != null && query.End == null)
            {
                if (query.Value.GetType() == typeof(int))
                {
                    expr = a => a.GetTag<int>(query.TagName) >= (int)query.Start;
                }
                else if (query.Value.GetType() == typeof(long))
                {
                    expr = a => a.GetTag<long>(query.TagName) >= (long)query.Start;

                }
                else if (query.Value.GetType() == typeof(string))
                {
                    throw new NotSupportedException();
                }
                else if (query.Value.GetType() == typeof(DateTime))
                {
                    expr = a => a.GetTag<DateTime>(query.TagName) >= (DateTime)query.Start;

                }
                else if (query.Value.GetType() == typeof(double))
                {
                    expr = a => a.GetTag<double>(query.TagName) >= (double)query.Start;

                }
                else if (query.Value.GetType() == typeof(float))
                {

                    expr = a => a.GetTag<float>(query.TagName) >= (float)query.Start;

                }
            }
            else if (query.Start == null && query.End !=null)
            {
                if (query.Value.GetType() == typeof(int))
                {
                    expr = a =>  a.GetTag<int>(query.TagName) <= (int)query.End;
                }
                else if (query.Value.GetType() == typeof(long))
                {
                    expr = a => a.GetTag<long>(query.TagName) <= (long)query.End;

                }
                else if (query.Value.GetType() == typeof(string))
                {
                    throw new NotSupportedException();
                }
                else if (query.Value.GetType() == typeof(DateTime))
                {
                    expr = a =>  a.GetTag<DateTime>(query.TagName) <= (DateTime)query.End;

                }
                else if (query.Value.GetType() == typeof(double))
                {
                    expr = a => a.GetTag<double>(query.TagName) <= (double)query.End;

                }
                else if (query.Value.GetType() == typeof(float))
                {

                    expr = a =>  a.GetTag<float>(query.TagName) <= (float)query.End;

                }
            }
            return expr;
        }
        
        public async Task<CryptonorResultSet> GetAll()
        {

            var all= await localDB.LoadAll();

            return new CryptonorResultSet { Objects = all, Count = all.Count };
        }

        public async Task<CryptonorResultSet> GetAll(int skip,int limit)
        {
           
            var all=await localDB.Query().Skip(skip).Take(limit).ToListAsync();
           
            return new CryptonorResultSet { Objects = all, Count = all.Count };
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
                CryptonorChangeSet changeSet= await this.localDB.GetChangeSet();
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
            CryptonorHttpClient httpClient = new CryptonorHttpClient(this.uri, this.dbName);
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
