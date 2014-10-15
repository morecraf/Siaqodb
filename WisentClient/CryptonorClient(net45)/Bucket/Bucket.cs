using Cryptonor;
using Cryptonor.Queries;
using CryptonorClient.Exceptions;
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
    public class Bucket : IBucket
    {
        public string BucketName { get; set; }
        CryptonorHttpClient httpClient;
        public Bucket(string uri, string bucketName, string username, string password)
        {
            this.BucketName = bucketName;
            this.httpClient = new CryptonorHttpClient(uri, username, password);
        }
#if NON_ASYNC
        public CryptonorObject Get(string key)
        {
            return httpClient.Get(this.BucketName, key);
        }
#endif

#if ASYNC
        public async Task<CryptonorObject> GetAsync(string key)
        {
            return await httpClient.GetAsync(this.BucketName, key);
        }
#endif
#if NON_ASYNC
        public T Get<T>(string key)
        {
            CryptonorObject obj = httpClient.Get(this.BucketName, key);
            if (obj == null)
                return default(T);
            return obj.GetValue<T>();
        }
#endif

#if ASYNC
        public async Task<T> GetAsync<T>(string key)
        {
            CryptonorObject obj = await httpClient.GetAsync(this.BucketName, key);
            if (obj == null)
                return default(T);
            return obj.GetValue<T>();
        }
#endif
#if NON_ASYNC
        public ResultSet GetAll()
        {
            var all = httpClient.Get(this.BucketName);
            return all;
        }
#endif

#if ASYNC
        public async Task<ResultSet> GetAllAsync()
        {
            var all = await httpClient.GetAsync(this.BucketName);
            return all;
        }
#endif
#if NON_ASYNC
        public ResultSet GetAll(int skip, int limit)
        {
            var all = httpClient.Get(this.BucketName, skip, limit);
            return all;
        }
#endif

#if ASYNC
        public async Task<ResultSet> GetAllAsync(int skip, int limit)
        {
            var all = await httpClient.GetAsync(this.BucketName, skip, limit);
            return all;
        }
#endif
#if NON_ASYNC
        public ResultSet Get(Query query)
        {
            return (httpClient.GetByTag(this.BucketName, query));
        }
#endif

#if ASYNC
        public async Task<ResultSet> GetAsync(Query query)
        {
            return (await httpClient.GetByTagAsync(this.BucketName, query));
        }
#endif
#if NON_ASYNC
        public void Store(CryptonorObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (string.IsNullOrEmpty(obj.Key))
                throw new ArgumentNullException("Key of CryptonorObject cannot be NULL");

            WriteResponse response = httpClient.Put(this.BucketName, obj);
            if (response.IsSuccess)
                obj.Version = response.Version;
            else
            {
                throw new WriteException(string.Format("Write error->{0}:{1}", response.Error, response.ErrorDesc));
            }

        }
#endif

#if ASYNC
        public async Task StoreAsync(CryptonorObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if(string.IsNullOrEmpty(obj.Key))
                throw new ArgumentNullException("Key of CryptonorObject cannot be NULL");
            WriteResponse response = await httpClient.PutAsync(this.BucketName, obj);
            if (response.IsSuccess)
                obj.Version = response.Version;
            else
            {
                throw new WriteException(string.Format("Write error->{0}:{1}", response.Error,response.ErrorDesc));
            }

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
            await this.StoreAsync(key, obj, null);
        }
#endif
#if NON_ASYNC
        public void Store(string key, object obj, Dictionary<string, object> tags)
        {
            CryptonorObject crObject = new CryptonorObject();
            crObject.Key = key;
            crObject.SetValue(obj);

            if (tags != null)
            {
                foreach (string tagName in tags.Keys)
                {
                    crObject.SetTag(tagName, tags[tagName]);
                }
            }

            Store(crObject);
            Type type = obj.GetType();
            if (CryptonorConfigurator.VersionGetConventions.ContainsKey(type))
            {
                CryptonorConfigurator.VersionGetConventions[type](obj, crObject.Version);
            }
        }
#endif

#if ASYNC
        public async Task StoreAsync(string key, object obj, Dictionary<string, object> tags)
        {
            CryptonorObject crObject = new CryptonorObject();
            crObject.Key = key;
            crObject.SetValue(obj);

            if (tags != null)
            {
                foreach (string tagName in tags.Keys)
                {
                    crObject.SetTag(tagName, tags[tagName]);
                }
            }

            await this.StoreAsync(crObject);
            Type type = obj.GetType();
            if (CryptonorConfigurator.VersionGetConventions.ContainsKey(type))
            {
                CryptonorConfigurator.VersionGetConventions[type](obj, crObject.Version);
            }
        }
#endif
#if NON_ASYNC
#if CF 
        public void Store(string key, object obj, object tags )
#else
        public void Store(string key, object obj, object tags = null)
#endif
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

            await this.StoreAsync(key, obj, tags_Dict);
        }
#endif
#if ASYNC
        public async Task StoreAsync(object obj, object tags = null)
        {
            await this.StoreAsync(null, obj, tags);
        }
#endif
#if NON_ASYNC
#if CF
         public void Store(object obj, object tags)
#else
        public void Store(object obj, object tags = null)
#endif
        
        {
            this.Store(null, obj, tags);
        }
#endif

#if NON_ASYNC
        public void Delete(string key)
        {
            httpClient.Delete(this.BucketName, key, null);
        }
#endif


#if ASYNC
        public async Task DeleteAsync(string key)
        {
            await httpClient.DeleteAsync(this.BucketName, key, null);
        }
#endif

#if NON_ASYNC
         public void Delete(CryptonorObject obj)
        {
            httpClient.Delete(this.BucketName, obj.Key, obj.Version);
        }
#endif
#if ASYNC
        public async Task DeleteAsync(CryptonorObject obj)
        {
            await httpClient.DeleteAsync(this.BucketName, obj.Key, obj.Version);
        }
#endif
#if NON_ASYNC
          public BatchResponse StoreBatch(IList<CryptonorObject> objects)
        {
            return  httpClient.Put(this.BucketName, new ChangeSet { ChangedObjects =new List<CryptonorObject>( objects) });
        }
#endif

#if ASYNC

        public async Task<BatchResponse> StoreBatchAsync(IList<CryptonorObject> objects)
        {
            return await httpClient.PutAsync(this.BucketName, new ChangeSet { ChangedObjects = new List<CryptonorObject>(objects) });
        }
#endif

    }
}
