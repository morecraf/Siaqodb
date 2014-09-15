using Cryptonor;
using Cryptonor.Queries;
using CryptonorClient.Http;
using Sqo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CryptonorClient
{
    public class CryptonorHttpClient : IDisposable
    {
        string uri;
        string dbName;
        HttpClient httpClient;
        RequestBuilder requestBuilder;
        Signature signature;
        public CryptonorHttpClient(string uri, string dbName,string appKey,string secretKey)
        {
            this.uri = uri.TrimEnd('/').TrimEnd('\\');
            this.dbName = dbName;
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(this.uri);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
          
            this.requestBuilder = new RequestBuilder(this.uri);
            this.signature = new Signature(appKey, secretKey);
        }
        public async Task<CryptonorResultSet> Get(string bucket)
        {
            return await Get(bucket, 0, 0);
        }
        public async Task<CryptonorResultSet> Get(string bucket, int skip, int limit)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (limit > 0)
            {
                parameters.Add("limit", limit.ToString());
            }
            if (skip > 0)
            {
                parameters.Add("skip", skip.ToString());
            }
            HttpRequestMessage request = requestBuilder.BuildGetRequest(bucket, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorResultSet), GetDefaultFormatter());

            return (CryptonorResultSet)obj;
        }
        public async Task<CryptonorObject> Get(string bucket, string key)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            HttpRequestMessage request = requestBuilder.BuildGetRequest(uriFragment, null);
           
            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorObject), GetDefaultFormatter());

            return (CryptonorObject)obj;
        }

        public async Task<CryptonorWriteResponse> Put(string bucket, CryptonorObject obj)
        {
            string uriFragment = bucket;
            HttpRequestMessage request = requestBuilder.BuildPostRequest(uriFragment, obj);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var objResp = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorWriteResponse), GetDefaultFormatter());
            return (CryptonorWriteResponse)objResp;
            
        }
        public async Task<CryptonorBatchResponse> Put(string bucket, CryptonorChangeSet batch)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "batch");

            HttpRequestMessage request = requestBuilder.BuildPostRequest(uriFragment, batch);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorBatchResponse), GetDefaultFormatter());
            return (CryptonorBatchResponse)obj;
            
        }
        internal async Task<CryptonorResultSet> GetByTag(string bucket, CryptonorQuery query)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "search");

            HttpRequestMessage request = requestBuilder.BuildPostRequest(uriFragment, query);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorResultSet), GetDefaultFormatter());

            return (CryptonorResultSet)obj;
          
        }
        public async Task<CryptonorChangeSet> GetChanges(string bucket, CryptonorQuery query, int limit,string anchor)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "changes");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (limit > 0)
            {
                parameters.Add("limit", limit.ToString());
            }
            if (!string.IsNullOrEmpty(anchor))
            {
                parameters.Add("anchor", anchor);
            }
            HttpRequestMessage request = requestBuilder.BuildPostRequest(uriFragment, query,parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorChangeSet), GetDefaultFormatter());

            return (CryptonorChangeSet)obj;

        }
        public async Task<CryptonorChangeSet> GetChanges(string bucket, int limit, string anchor)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "changes");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (limit > 0)
            {
                parameters.Add("limit", limit.ToString());
            }
            if (!string.IsNullOrEmpty(anchor))
            {
                parameters.Add("anchor", anchor);
            }
            HttpRequestMessage request = requestBuilder.BuildGetRequest(uriFragment, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorChangeSet), GetDefaultFormatter());

            return (CryptonorChangeSet)obj;

        }

        internal async Task Delete(string bucket, string key, string version)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (version !=null )
            {
                parameters.Add("version", version);
            }
            HttpRequestMessage request = requestBuilder.BuildDeleteRequest(uriFragment, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
        }
        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            await signature.SignMessage(request);
            return await httpClient.SendAsync(request);
        }
        public IEnumerable<MediaTypeFormatter> GetDefaultFormatter()
        {
            List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
            formatters.Add(
                new JsonMediaTypeFormatter());
            return formatters;
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/> for
        /// derived classes to use.
        /// </summary>
        /// <param name="disposing">
        /// Indicates if being called from the Dispose() method
        /// or the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                this.httpClient.Dispose();
            }
        }
    }
}
