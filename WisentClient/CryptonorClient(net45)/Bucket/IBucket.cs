using Cryptonor;
using Cryptonor.Queries;
using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace CryptonorClient
{
    public interface IBucket
    {
#if ASYNC

        Task<CryptonorObject> GetAsync(string key);
        Task<T> GetAsync<T>(string key);
         Task<CryptonorResultSet> GetAsync(CryptonorQuery query);
         Task<CryptonorResultSet> GetAllAsync();
         Task<CryptonorResultSet> GetAllAsync(int skip, int limit);
        Task StoreAsync(CryptonorObject obj);
        Task StoreAsync(string key, object obj);
        Task StoreAsync(string key, object obj, System.Collections.Generic.Dictionary<string, object> tags);
        Task StoreAsync(string key, object obj, object tags = null);
        Task StoreAsync(object obj, object tags = null);
         Task<CryptonorBatchResponse> StoreBatchAsync(IList<CryptonorObject> obj);
        Task DeleteAsync(string key);
        Task DeleteAsync(CryptonorObject obj);
#endif
#if NON_ASYNC
        CryptonorObject Get(string key);
        T Get<T>(string key);
        CryptonorResultSet Get(CryptonorQuery query);
        CryptonorResultSet GetAll();
        CryptonorResultSet GetAll(int skip, int limit);
        void Store(CryptonorObject obj);
        void Store(string key, object obj);
        void Store(string key, object obj, System.Collections.Generic.Dictionary<string, object> tags);
        void Store(string key, object obj, object tags = null);
        void Store( object obj, object tags = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        CryptonorBatchResponse StoreBatch(IList<CryptonorObject> obj);
        void Delete(string key);
        void Delete(CryptonorObject obj);
#endif
        string BucketName { get; set; }

        
    }
}
