using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using SiaqodbCloudService.Filters;
using SiaqodbCloudService.Repository;
using SiaqodbCloudService.Models;

namespace SiaqodbCloudService.Controllers
{
    [Authenticate]
    [SyncExceptionFilter]
    public class SyncController : ApiController
    {
        private IRepository repository = RepositoryFactory.GetRepository();

        /// <summary>
        /// Checks if the service is alive
        /// </summary>
        /// <returns>pong</returns>
        [Route("v0/ping")]
        [HttpGet]
        public string Ping()
        {
            return "pong";
        }

        /// <summary>
        /// Get an object based on key provided
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="key">Key value</param>
        /// <returns></returns>
        [Route("v0/{bucketName}/{key}")]
        [HttpGet]
        public async Task<SiaqodbDocument> Get(string bucketName, string key, string version = null)
        {

            var result = await repository.Get(bucketName, key, version);
            return result;

        }

        /// <summary>
        /// Get all changes for a certain anchor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="limit">Max number of objects to return</param>
        /// <param name="anchor">The anchor after which the changes occured</param>
        /// <returns></returns>
        [Route("v0/{bucketName}/changes")]
        [HttpGet]
        public async Task<BatchSet> GetChanges(string bucketName, int limit = 100, string anchor = null,string uploadanchor= null)
        {
            if (limit > 10000)
                limit = 10000;
            var result = await repository.GetAllChanges(bucketName, limit, anchor, uploadanchor);

            return result;
        }
        /// <summary>
        /// Get changes based on a Query 
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="query">Query value</param>
        /// <param name="limit">Max number of objects</param>
        /// <param name="anchor">The anchor after which the changes occured</param>
        /// <returns></returns>
        [Route("v0/{bucketName}/changes")]
        [HttpPost]
        public async Task<BatchSet> GetChanges(string bucketName, [FromBody] Filter query, int limit = 100, string anchor = null,string uploadanchor = null)
        {
            if (limit > 1000)
                limit = 1000;
            var result = await repository.GetChanges(bucketName, query, limit, anchor, uploadanchor);


            return result;
        }


        /// <summary>
        /// Store and object within a bucket
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="value">CryptonorObject to be stored </param>
        /// <returns>Response of the operation</returns>
        [Route("v0/{bucketName}")]
        [HttpPost]
        public async Task<StoreResponse> Post(string bucketName, [FromBody]SiaqodbDocument value)
        {
            var result = await repository.Store(bucketName, value);

            return result;
        }
        /// <summary>
        /// Store a batch of objects
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="value">Batch to be stored</param>
        /// <returns>Response of the operation</returns>
        [Route("v0/{bucketName}/batch")]
        [HttpPost]
        public async Task<BatchResponse> Post(string bucketName, [FromBody]BatchSet value)
        {
            var result = await repository.Store(bucketName, value);

            return result;
        }
       
        /// <summary>
        /// Store and object within a bucket
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="key">Key value</param>
        /// <param name="value">CryptonorObject to be stored</param>
        /// <returns></returns>
        [Route("v0/{bucketName}/{key}")]
        [HttpPut]
        public async Task<StoreResponse> Put(string bucketName, string key, [FromBody]SiaqodbDocument value)
        {
            value.Key = key;
            return await repository.Store(bucketName, value);
        }
        /// <summary>
        /// Delete an object from the bucket provided
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="key">Key value</param>
        /// <param name="version">Version of object</param>
        /// <returns></returns>
        [Route("v0/{bucketName}/{key}")]
        [HttpDelete]
        public async Task<StoreResponse> Delete(string bucketName, string key, string version = null)
        {
            return await repository.Delete(bucketName, key, version);
        }
        
    }
}
