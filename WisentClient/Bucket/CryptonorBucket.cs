using Cryptonor;
using Cryptonor.Queries;
using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public class CryptonorBucket :IBucket
    {
        public string BucketName { get; set; }
        CryptonorHttpClient httpClient;
        public CryptonorBucket(string uri,string dbName,string bucketName,string appKey,string secretKey)
        {
            this.BucketName = bucketName;
            this.httpClient = new CryptonorHttpClient(uri,dbName,appKey,secretKey);
        }
       
        public async Task<CryptonorObject> Get(string key)
        {
            return await httpClient.Get(this.BucketName, key);
        }

        public async Task<T> Get<T>(string key)
        {
            CryptonorObject obj = await httpClient.Get(this.BucketName, key);
            return obj.GetValue<T>();
        }

        public async Task<CryptonorResultSet> GetAll()
        {
            var all=await httpClient.Get(this.BucketName);
            return all;
        }
        public async Task<CryptonorResultSet> GetAll(int skip,int limit)
        {
            var all = await httpClient.Get(this.BucketName,skip,limit);
            return all;
        }
        public async Task<CryptonorResultSet> Get(CryptonorQuery query)
        {
           return (await httpClient.GetByTag(this.BucketName, query));
        }
        public async Task Store(CryptonorObject obj)
        {
            await httpClient.Put(this.BucketName, obj);
        }

        public async Task Store(string key, object obj)
        {
            await this.Store(key, obj, null);
        }

        public async Task Store(string key, object obj, Dictionary<string, object> tags)
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
            await httpClient.Delete(this.BucketName, key, null);
        }

        public async Task Delete(CryptonorObject obj)
        {
            await httpClient.Delete(this.BucketName, obj.Key, obj.Version);
        }
      
      


        public async Task StoreBatch(IList<CryptonorObject> objects)
        {
            await httpClient.Put(this.BucketName, new CryptonorChangeSet { ChangedObjects = objects });
        }
    }
}
