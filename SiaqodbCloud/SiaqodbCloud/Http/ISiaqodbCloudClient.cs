using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Sqo.Documents;
using Sqo.Documents.Sync;
using System;

namespace SiaqodbCloud
{
    internal interface ISiaqodbCloudClient:IDisposable
    {
       
       
        Document Get(string bucket, string key, string version = null);
        Task<Document> GetAsync(string bucket, string key, string version = null);
        
        ChangeSet GetChanges(string bucket, int limit, string anchor, string uploadAnchor);
        ChangeSet GetChanges(string bucket, Filter query, int limit, string anchor, string uploadAnchor);
        Task<ChangeSet> GetChangesAsync(string bucket, int limit, string anchor, string uploadAnchor);
        Task<ChangeSet> GetChangesAsync(string bucket, Filter query, int limit, string anchor, string uploadAnchor);
       
        StoreResponse Put(string bucket, Document obj);
        BatchResponse Put(string bucket, ChangeSet batch);
        Task<StoreResponse> PutAsync(string bucket, Document obj);
        Task<BatchResponse> PutAsync(string bucket, ChangeSet batch);

        Task DeleteAsync(string bucket, string key, string version);
        void Delete(string bucket, string key, string version);
    }
}