using Cryptonor;
using Cryptonor.Queries;
using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#if ASYNC
using System.Threading.Tasks;
#endif


namespace CryptonorClient
{
    public class CryptonorBucket : IBucket
    {
        public string BucketName { get; set; }
        CryptonorHttpClient httpClient;
        public CryptonorBucket(string uri, string dbName, string bucketName, string appKey, string secretKey)
        {
            this.BucketName = bucketName;
            this.httpClient = new CryptonorHttpClient(uri, dbName, appKey, secretKey);
        }

        public CryptonorObject Get(string key)
        {
            return httpClient.Get(this.BucketName, key);
        }

#if ASYNC
    public async Task<CryptonorObject> GetAsync(string key)
        {
            return await httpClient.GetAsync(this.BucketName, key);
        }
#endif

        public T Get<T>(string key)
        {
            CryptonorObject obj = httpClient.Get(this.BucketName, key);
            return obj.GetValue<T>();
        }

#if ASYNC
 public async Task<T> GetAsync<T>(string key)
        {
            CryptonorObject obj = await httpClient.GetAsync(this.BucketName, key);
            return obj.GetValue<T>();
        }
#endif

        public CryptonorResultSet GetAll()
        {
            var all = httpClient.Get(this.BucketName);
            return all;
        }

#if ASYNC
  public async Task<CryptonorResultSet> GetAllAsync()
        {
            var all=await httpClient.GetAsync(this.BucketName);
            return all;
        }
#endif

        public CryptonorResultSet GetAll(int skip, int limit)
        {
            var all = httpClient.Get(this.BucketName, skip, limit);
            return all;
        }

#if ASYNC
  public async Task<CryptonorResultSet> GetAllAsync(int skip,int limit)
        {
            var all = await httpClient.GetAsync(this.BucketName,skip,limit);
            return all;
        }
#endif

        public CryptonorResultSet Get(CryptonorQuery query)
        {
            return (httpClient.GetByTag(this.BucketName, query));
        }

#if ASYNC
 public async Task<CryptonorResultSet> GetAsync(CryptonorQuery query)
        {
           return (await httpClient.GetByTagAsync(this.BucketName, query));
        }
#endif

        public void Store(CryptonorObject obj)
        {
            CryptonorWriteResponse response = httpClient.Put(this.BucketName, obj);
            if (response.IsSuccess)
                obj.Version = response.Version;
            else
            {
                throw new Exception("Write error->" + response.Error);
            }

        }

#if ASYNC
  public async Task StoreAsync(CryptonorObject obj)
        {
            CryptonorWriteResponse response = await httpClient.PutAsync(this.BucketName, obj);
            if (response.IsSuccess)
                obj.Version = response.Version;
            else
            {
                throw new Exception("Write error->" + response.Error);
            }

        }
#endif

        public void Store(string key, object obj)
        {
            this.Store(key, obj, null);
        }

#if ASYNC
 public async Task StoreAsync(string key, object obj)
        {
            await this.StoreAsync(key, obj, null);
        }
#endif

        public void Store(string key, object obj, Dictionary<string, object> tags)
        {
            CryptonorObject cryObject = new CryptonorObject();
            cryObject.Key = key;
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

#if ASYNC
 public async Task StoreAsync(string key, object obj, Dictionary<string, object> tags)
        {
            CryptonorObject cryObject = new CryptonorObject();
            cryObject.Key = key;
            cryObject.SetValue(obj);

            if (tags != null)
            {
                foreach (string tagName in tags.Keys)
                {
                    cryObject.SetTag(tagName, tags[tagName]);
                }
            }

            await this.StoreAsync(cryObject);
        }
#endif

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

           await this.StoreAsync(key, obj, tags_Dict);
        }
#endif

        public void Delete(string key)
        {
            httpClient.Delete(this.BucketName, key, null);
        }

#if ASYNC
public async Task DeleteAsync(string key)
        {
             await httpClient.DeleteAsync(this.BucketName, key, null);
        }
#endif

         public void Delete(CryptonorObject obj)
        {
            httpClient.Delete(this.BucketName, obj.Key, obj.Version);
        }

#if ASYNC
 public async Task DeleteAsync(CryptonorObject obj)
        {
            await httpClient.DeleteAsync(this.BucketName, obj.Key, obj.Version);
        }
#endif

          public CryptonorBatchResponse StoreBatch(IList<CryptonorObject> objects)
        {
            return  httpClient.Put(this.BucketName, new CryptonorChangeSet { ChangedObjects = objects });
        }

#if ASYNC

        public async Task<CryptonorBatchResponse> StoreBatchAsync(IList<CryptonorObject> objects)
        {
            return await httpClient.PutAsync(this.BucketName, new CryptonorChangeSet { ChangedObjects = objects });
        }
#endif

    }
}
