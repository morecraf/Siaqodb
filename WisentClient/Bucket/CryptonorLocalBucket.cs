using Sqo;
using System;
using System.Collections.Generic;
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

        public async Task<IList<T>> GetAll<T>()
        {

            List<T> list = new List<T>();
            IList<CryptonorObject> list2 = await this.localDB.LoadAll();
            foreach (CryptonorObject current in list2)
            {
                list.Add(current.GetValue<T>());
            }
            return list;

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
            await localDB.Delete(key);
        }
        public void Push()
        { 
        
        }
        public void Pull()
        { 
            
        }
 
    }
}
