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
        public Sqo.CryptonorObject Get(string key)
        {
            return httpClient.Get(this.BucketName, key).Result;
        }

        public T Get<T>(string key)
        {
            CryptonorObject obj = httpClient.Get(this.BucketName, key).Result;
            return obj.GetValue<T>();
        }

        public IList<Sqo.CryptonorObject> GetAll()
        {
            return httpClient.Get(this.BucketName).Result.ToList();
        }

        public IList<T> GetAll<T>()
        {
            List<T> list = new List<T>();
            IList<CryptonorObject> list2 = this.GetAll();
            foreach (CryptonorObject current in list2)
            {
                list.Add(current.GetValue<T>());
            }
            return list;
        }

      

        public void Store(Sqo.CryptonorObject obj)
        {
            httpClient.Put(this.BucketName, obj).Wait();
        }

        public void Store(string key, object obj)
        {
            this.Store(key, obj, null);
        }

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

            this.Store(cryObject);
        }

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
                    tags_Dict.Add(p.Name, p.GetValue(o));
                }
            }

            this.Store(key, obj, tags_Dict);
        }

        public void Delete(string key)
        {
            throw new NotImplementedException();
        }


        public IList<Sqo.CryptonorObject> Get(System.Linq.Expressions.Expression expression)
        {
            QueryTranslator t = new QueryTranslator();
            Where where = t.Translate(expression);
            return httpClient.GetByTag(this.BucketName, where.TagName, Mapper.GetAPIOperator(where.OperationType), where.TagValue).Result.ToList();
        }
        public async Task<IList<Sqo.CryptonorObject>> GetAsync(System.Linq.Expressions.Expression expression)
        {
            QueryTranslator t = new QueryTranslator();
            Where where = t.Translate(expression);
            return (await httpClient.GetByTag(this.BucketName, where.TagName, Mapper.GetAPIOperator(where.OperationType), where.TagValue)).ToList();
        }
    }
}
