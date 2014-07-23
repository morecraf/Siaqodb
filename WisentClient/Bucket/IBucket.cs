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
        Sqo.ISqoQuery<Sqo.CryptonorObject> Query(long continuationToken);
      
        Task<CryptonorObject> Get(string key);
        Task<T> Get<T>(string key);
        Task<CryptonorResultSet> Get(System.Linq.Expressions.Expression expression);
        Task<CryptonorResultSet> Get(System.Linq.Expressions.Expression expression,long continuationToken);

        Task<CryptonorResultSet> GetAll();
        Task Store(CryptonorObject obj);
        Task Store(string key, object obj);
        Task Store(string key, object obj, System.Collections.Generic.Dictionary<string, object> tags);
        Task Store(string key, object obj, object tags = null);
        Task Delete(string key);
        string BucketName { get; set; }

        
    }
}
