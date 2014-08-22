using Cryptonor;
using Cryptonor.Queries;
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
        
        Task<CryptonorObject> Get(string key);
        Task<T> Get<T>(string key);
        Task<CryptonorResultSet> Get(CryptonorQuery query);
        Task<CryptonorResultSet> GetAll();
        Task<CryptonorResultSet> GetAll(int skip,int limit);
        Task Store(CryptonorObject obj);
        Task Store(string key, object obj);
        Task Store(string key, object obj, System.Collections.Generic.Dictionary<string, object> tags);
        Task Store(string key, object obj, object tags = null);
        Task<CryptonorBatchResponse> StoreBatch(IList<CryptonorObject> obj);
        Task Delete(string key);
        Task Delete(CryptonorObject obj);
        string BucketName { get; set; }

        
    }
}
