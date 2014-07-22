using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public interface IBucket
    {
        ISqoQuery<T> Cast<T>();
        Sqo.ISqoQuery<Sqo.CryptonorObject> Query();
        CryptonorObject Get(string key);
        T Get<T>(string key);
        System.Collections.Generic.IList<CryptonorObject> Get(System.Linq.Expressions.Expression expression);
        Task<System.Collections.Generic.IList<CryptonorObject>> GetAsync(System.Linq.Expressions.Expression expression);
     
        Task<System.Collections.Generic.IList<CryptonorObject>> GetAllAsync();
        Task<System.Collections.Generic.IList<T>> GetAllAsync<T>();
        void Store(CryptonorObject obj);
        void Store(string key, object obj);
        void Store(string key, object obj, System.Collections.Generic.Dictionary<string, object> tags);
        void Store(string key, object obj, object tags = null);
        void Delete(string key);
        string BucketName { get; set; }

        
    }
}
