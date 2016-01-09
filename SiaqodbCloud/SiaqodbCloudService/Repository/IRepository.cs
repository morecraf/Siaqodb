using SiaqodbCloudService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SiaqodbCloudService.Repository
{
    public interface IRepository
    {
        Task<string> GetSecretAccessKey(string appKeyString);
        Task<SiaqodbDocument> Get(string bucketName, string key, string version);
        Task<BatchSet> GetAllChanges(string bucketName, int limit, string anchor,string uploadAnchor);
        Task<BatchResponse> Store(string bucketName, BatchSet value);
        Task<StoreResponse> Store(string bucketName, SiaqodbDocument document);
        Task<StoreResponse> Delete(string bucketName, string key, string version);
        Task<BatchSet> GetChanges(string bucketName, Filter query, int limit, string anchor, string uploadAnchor);
    }
}