using System.IO;
using System.Net;

using Cryptonor;
using Cryptonor.Queries;
using Newtonsoft.Json;
using Sqo;
using CryptonorClient.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
#if ASYNC
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
#endif
using System.Web;

namespace CryptonorClient
{
    public class CryptonorHttpClient : IDisposable
    {
        string uri;
        string dbName;
#if ASYNC
        HttpClient httpClient;
#endif
        RequestBuilder requestBuilder;
        Signature signature;
        public CryptonorHttpClient(string uri, string dbName,string appKey,string secretKey)
        {
            this.uri = uri.TrimEnd('/').TrimEnd('\\');
            this.dbName = dbName;
#if ASYNC
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(this.uri);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
#endif
            this.requestBuilder = new RequestBuilder(this.uri);
            this.signature = new Signature(appKey, secretKey);
        }

        public CryptonorResultSet Get(string bucket)
        {
            return Get(bucket, 0, 0);
        }

#if ASYNC
        public async Task<CryptonorResultSet> GetAsync(string bucket)
        {
            return await GetAsync(bucket, 0, 0);
        }
#endif

        public CryptonorResultSet Get(string bucket, int skip, int limit)
        {
            var parameters = new Dictionary<string, string>();
            if (limit > 0)
            {
                parameters.Add("limit", limit.ToString());
            }
            if (skip > 0)
            {
                parameters.Add("skip", skip.ToString());
            }
            var request = requestBuilder.BuildGetRequest(bucket, parameters);

            var resp = SendSync(request);


            return DeserializeResponse<CryptonorResultSet>(resp);
        }

#if ASYNC
        public async Task<CryptonorResultSet> GetAsync(string bucket, int skip, int limit)
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
            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(bucket, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorResultSet), GetDefaultFormatter());

            return (CryptonorResultSet)obj;
        }
#endif

        public CryptonorObject Get(string bucket, string key)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            HttpWebRequest request = requestBuilder.BuildGetRequest(uriFragment, null);

            var resp = SendSync(request);


            return DeserializeResponse<CryptonorObject>(resp);
        }

#if ASYNC
        public async Task<CryptonorObject> GetAsync(string bucket, string key)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(uriFragment, null);
           
            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorObject), GetDefaultFormatter());

            return (CryptonorObject)obj;
        }
#endif

        public CryptonorWriteResponse Put(string bucket, CryptonorObject obj)
        {
            string uriFragment = bucket;
            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment);

            var resp = PostSync(request, JsonConvert.SerializeObject(obj));

            return DeserializeResponse<CryptonorWriteResponse>(resp);
        }


#if ASYNC
        public async Task<CryptonorWriteResponse> PutAsync(string bucket, CryptonorObject obj)
        {
            string uriFragment = bucket;
            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, obj);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var objResp = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorWriteResponse), GetDefaultFormatter());
            return (CryptonorWriteResponse)objResp;
            
        }
#endif

        public CryptonorBatchResponse Put(string bucket, CryptonorChangeSet batch)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "batch");
            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment);

            var resp = PostSync(request, JsonConvert.SerializeObject(batch));

            return DeserializeResponse<CryptonorBatchResponse>(resp);
        }

#if ASYNC
        public async Task<CryptonorBatchResponse> PutAsync(string bucket, CryptonorChangeSet batch)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "batch");

            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, batch);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorBatchResponse), GetDefaultFormatter());
            return (CryptonorBatchResponse)obj;
            
        }
#endif

        internal CryptonorResultSet GetByTag(string bucket, CryptonorQuery query)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "search");

            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment);

            var resp = PostSync(request, JsonConvert.SerializeObject(query));

            return DeserializeResponse<CryptonorResultSet>(resp);
        }

#if ASYNC
        internal async Task<CryptonorResultSet> GetByTagAsync(string bucket, CryptonorQuery query)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "search");

            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, query);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorResultSet), GetDefaultFormatter());

            return (CryptonorResultSet)obj;
          
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public CryptonorChangeSet GetChanges(string bucket, CryptonorQuery query, int limit, string anchor)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "changes");
            var parameters = new Dictionary<string, string>();
            if (limit > 0)
            {
                parameters.Add("limit", limit.ToString());
            }
            if (!string.IsNullOrEmpty(anchor))
            {
                parameters.Add("anchor", anchor);
            }

            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment, parameters);
            var resp = PostSync(request, JsonConvert.SerializeObject(query));

            return DeserializeResponse<CryptonorChangeSet>(resp);
        }

        private static T DeserializeResponse<T>(HttpWebResponse resp)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            using (var sr = new StreamReader(resp.GetResponseStream()))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

#if ASYNC
        public async Task<CryptonorChangeSet> GetChangesAsync(string bucket, CryptonorQuery query, int limit,string anchor)
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
            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, query,parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorChangeSet), GetDefaultFormatter());

            return (CryptonorChangeSet)obj;

        }
#endif

        public CryptonorChangeSet GetChanges(string bucket, int limit, string anchor)
        {
            var uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "changes");
            var parameters = new Dictionary<string, string>();
            if (limit > 0)
            {
                parameters.Add("limit", limit.ToString());
            }
            if (!string.IsNullOrEmpty(anchor))
            {
                parameters.Add("anchor", anchor);
            }
            var request = requestBuilder.BuildGetRequest(uriFragment, parameters);

            var resp = SendSync(request);

        

            return DeserializeResponse<CryptonorChangeSet>(resp);
        }


#if ASYNC
        public async Task<CryptonorChangeSet> GetChangesAsync(string bucket, int limit, string anchor)
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
            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(uriFragment, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorChangeSet), GetDefaultFormatter());

            return (CryptonorChangeSet)obj;

        }

#endif

        internal void Delete(string bucket, string key, string version)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (version != null)
            {
                parameters.Add("version", version);
            }
            HttpWebRequest request = requestBuilder.BuildDeleteRequest(uriFragment, parameters);

            var resp = SendSync(request);

        }

        private HttpWebResponse SendSync(HttpWebRequest request)
        {
            signature.SignMessage(request);
            var resp = (HttpWebResponse) request.GetResponse();
            return resp;
        }
        private HttpWebResponse PostSync(HttpWebRequest request, string jsonContent)
        {
            signature.SignMessage(request,jsonContent);

            if (jsonContent != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(jsonContent);
                request.ContentLength = bytes.Length;
                Stream ps = request.GetRequestStream();
                ps.Write(bytes, 0, bytes.Length);
                ps.Close();
            }
            var resp = (HttpWebResponse)request.GetResponse();

            return resp;
        }

#if ASYNC
        internal async Task DeleteAsync(string bucket, string key, string version)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (version !=null )
            {
                parameters.Add("version", version);
            }
            HttpRequestMessage request = requestBuilder.BuildDeleteRequestAsync(uriFragment, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
        }

#endif

#if ASYNC
        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            await signature.SignMessageAsync(request);
            return await httpClient.SendAsync(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        public IEnumerable<MediaTypeFormatter> GetDefaultFormatter()
        {
            var formatters = new List<MediaTypeFormatter> {new JsonMediaTypeFormatter()};
            return formatters;
        }
#endif

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
#if ASYNC
                this.httpClient.Dispose();
#endif
            }
        }
    }
}
