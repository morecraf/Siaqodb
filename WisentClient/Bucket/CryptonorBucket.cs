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
        public CryptonorBucket(string bucketName,string uri,string dbName)
        {
            this.BucketName = bucketName;
            this.httpClient = new CryptonorHttpClient(uri,dbName);
        }
        public Sqo.ISqoQuery<T> Cast<T>()
        {
            return (Sqo.ISqoQuery<T>)new CNQuery<CryptonorObject>(this);
        }
        public Sqo.ISqoQuery<Sqo.CryptonorObject> Query()
        {
            return new CNQuery<Sqo.CryptonorObject>(this);
        }
        public Sqo.ISqoQuery<Sqo.CryptonorObject> Query(long continuationToken)
        {
            return new CNQuery<Sqo.CryptonorObject>(this, continuationToken);
        }
        public async Task<Sqo.CryptonorObject> Get(string key)
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
        public async Task<CryptonorResultSet> GetAll(int limit, long continuationToken)
        {
            var all = await httpClient.Get(this.BucketName,limit,continuationToken);
            return all;
        }

        public async Task Store(Sqo.CryptonorObject obj)
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
            throw new NotImplementedException();
        }

        public async Task Delete(CryptonorObject obj)
        {
            throw new NotImplementedException();
        }
        public async Task<CryptonorResultSet> Get(System.Linq.Expressions.Expression expression)
        {
            return await this.Get(expression, 0);
        }
        public async Task<CryptonorResultSet> Get(System.Linq.Expressions.Expression expression,long continuationToken)
        {
            QueryTranslator t = new QueryTranslator();
            List<Criteria> where = t.Translate(expression);
            if (where.Where(a => a.OperationType == Criteria.Equal).FirstOrDefault() == null)
                throw new Exception("At least one EQUAL operation must be set");
          
            return (await httpClient.GetByTag(this.BucketName, new QueryObject { Filter = where,ContinuationToken=continuationToken }));
        }


        public async Task StoreBatch(IList<CryptonorObject> objects)
        {
            await httpClient.Put(this.BucketName, objects);
        }
    }
}
