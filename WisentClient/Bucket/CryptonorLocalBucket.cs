using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public class CryptonorLocalBucket:IBucket
    {
        CryptonorLocalDB localDB;
        public string BucketName { get; set; }
        public CryptonorLocalBucket(string bucketName)
        {
            localDB = new CryptonorLocalDB(bucketName);
            this.BucketName = bucketName;
        }
        public Sqo.ISqoQuery<T> Cast<T>()
        {
            return localDB.Cast<T>();
        }

        public Sqo.CryptonorObject Get(string key)
        {
            return localDB.Load(key);
        }

        public T Get<T>(string key)
        {
            CryptonorObject obj= localDB.Load(key);
            return obj.GetValue<T>();
        }
        public IList<CryptonorObject> Get(System.Linq.Expressions.Expression expression)
        {
            return localDB.Load(expression);
        }
        public async Task<IList<Sqo.CryptonorObject>> GetAllAsync()
        {
            return  localDB.LoadAll();
        }

        public async Task<IList<T>> GetAllAsync<T>()
        {

            List<T> list = new List<T>();
            IList<CryptonorObject> list2 = this.localDB.LoadAll();
            foreach (CryptonorObject current in list2)
            {
                list.Add(current.GetValue<T>());
            }
            return list;

        }

        public Sqo.ISqoQuery<Sqo.CryptonorObject> Query()
        {
            return localDB.Query();
        }

        public void Store(Sqo.CryptonorObject obj)
        {
            localDB.Store(obj);
        }

        public void Store(string key, object obj)
        {
            this.Store(key, obj, null);
        }

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
            localDB.Delete(key);
        }
        public void Push()
        { 
        
        }
        public void Pull()
        { 
            
        }





        public Task<IList<CryptonorObject>> GetAsync(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
