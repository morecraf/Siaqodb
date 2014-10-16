using System.IO;
using System.Net;

using Cryptonor;
using Cryptonor.Queries;
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
#endif
using System.Web;

namespace CryptonorClient
{
    internal class CryptonorHttpClient : IDisposable
    {
        string uri;
       
#if ASYNC
        HttpClient httpClient;
#endif
        RequestBuilder requestBuilder;
        Signature signature;
        public CryptonorHttpClient(string uri,string username,string password)
        {
            this.uri = uri.TrimEnd('/').TrimEnd('\\');
#if ASYNC
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(this.uri);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
#endif
            this.requestBuilder = new RequestBuilder(this.uri);
            this.signature = new Signature(username, password);
        }
#if NON_ASYNC
        public ResultSet Get(string bucket)
        {
            return Get(bucket, 0, 0);
        }
#endif

#if ASYNC
        public async Task<ResultSet> GetAsync(string bucket)
        {
            return await GetAsync(bucket, 0, 0);
        }
#endif
        #if NON_ASYNC
        public ResultSet Get(string bucket, int skip, int limit)
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

            var resp = Send(request);


            return DeserializeResponse<ResultSet>(resp);
        }
#endif

#if ASYNC
        public async Task<ResultSet> GetAsync(string bucket, int skip, int limit)
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
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(ResultSet), GetDefaultFormatter());

            return (ResultSet)obj;
        }
#endif
        #if NON_ASYNC
        public CryptonorObject Get(string bucket, string key)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            HttpWebRequest request = requestBuilder.BuildGetRequest(uriFragment, null);

            var resp = Send(request);


            return DeserializeResponse<CryptonorObject>(resp);
        }
#endif

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
        #if NON_ASYNC
        public WriteResponse Put(string bucket, CryptonorObject obj)
        {
            string uriFragment = bucket;
            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment);

#if !UNITY3D && !CF
            var resp = Post(request, Newtonsoft.Json.JsonConvert.SerializeObject(obj));
#else
            var resp = Post(request, LitJson.JsonMapper.ToJson(obj));
#endif

            return DeserializeResponse<WriteResponse>(resp);
        }
#endif

#if ASYNC
        public async Task<WriteResponse> PutAsync(string bucket, CryptonorObject obj)
        {
            string uriFragment = bucket;
            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, obj);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var objResp = await httpResponseMessage.Content.ReadAsAsync(typeof(WriteResponse), GetDefaultFormatter());
            return (WriteResponse)objResp;
            
        }
#endif
        #if NON_ASYNC
        public BatchResponse Put(string bucket, ChangeSet batch)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "batch");
            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment);
            #if !UNITY3D && !CF
            var resp = Post(request, Newtonsoft.Json.JsonConvert.SerializeObject(batch));
#else
            var resp = Post(request, LitJson.JsonMapper.ToJson(batch));
#endif
            return DeserializeResponse<BatchResponse>(resp);
        }
#endif

#if ASYNC
        public async Task<BatchResponse> PutAsync(string bucket, ChangeSet batch)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "batch");

            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, batch);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(BatchResponse), GetDefaultFormatter());
            return (BatchResponse)obj;
            
        }
#endif
#if NON_ASYNC
        internal ResultSet GetByTag(string bucket, Query query)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "search");

            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment);
#if !UNITY3D && !CF
            var resp = Post(request, Newtonsoft.Json.JsonConvert.SerializeObject(query));
#else
            var resp = Post(request, LitJson.JsonMapper.ToJson(query));
#endif

            return DeserializeResponse<ResultSet>(resp);
        }
#endif

#if ASYNC
        internal async Task<ResultSet> GetByTagAsync(string bucket, Query query)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "search");

            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, query);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(ResultSet), GetDefaultFormatter());

            return (ResultSet)obj;
          
        }
#endif

        #if NON_ASYNC
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public ChangeSet GetChanges(string bucket, Query query, int limit, string anchor)
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
#if !UNITY3D && !CF
            var resp = Post(request, Newtonsoft.Json.JsonConvert.SerializeObject(query));
#else
            var resp = Post(request, LitJson.JsonMapper.ToJson(query));
#endif
            return DeserializeResponse<ChangeSet>(resp);
        }

        private static T DeserializeResponse<T>(HttpWebResponse resp)
        {
           #if !UNITY3D && !CF
            var serializer = new Newtonsoft.Json.JsonSerializer();
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(sr))
                {
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
#else
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                LitJson.JsonReader reader = new LitJson.JsonReader(sr);
                return LitJson.JsonMapper.ToObject<T>(reader);
            }
#endif

        }
#endif

#if ASYNC
        public async Task<ChangeSet> GetChangesAsync(string bucket, Query query, int limit,string anchor)
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
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(ChangeSet), GetDefaultFormatter());

            return (ChangeSet)obj;

        }
#endif
        #if NON_ASYNC
        public ChangeSet GetChanges(string bucket, int limit, string anchor)
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

            var resp = Send(request);

        

            return DeserializeResponse<ChangeSet>(resp);
        }
#endif

#if ASYNC
        public async Task<ChangeSet> GetChangesAsync(string bucket, int limit, string anchor)
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
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(ChangeSet), GetDefaultFormatter());

            return (ChangeSet)obj;

        }

#endif
        #if NON_ASYNC
        internal void Delete(string bucket, string key, string version)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (version != null)
            {
                parameters.Add("version", version);
            }
            HttpWebRequest request = requestBuilder.BuildDeleteRequest(uriFragment, parameters);

            var resp = Send(request);

        }

        private HttpWebResponse Send(HttpWebRequest request)
        {
            signature.SignMessage(request);
            var resp = (HttpWebResponse) request.GetResponse();
            return resp;
        }
        private HttpWebResponse Post(HttpWebRequest request, string jsonContent)
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
#endif

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
#if ASYNC
        public async Task<List<string>> GetBucketsAsync()
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}", "allBuckets");

            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(uriFragment, null);
           
            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(List<string>), GetDefaultFormatter());

            return (List<string>)obj;
        }
#endif
 #if NON_ASYNC
        public List<string> GetBuckets()
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}", "allBuckets");

            HttpWebRequest request = requestBuilder.BuildGetRequest(uriFragment, null);

            var resp = Send(request);

            return DeserializeResponse<List<string>>(resp);
        }
#endif
#if ASYNC
        public async Task<string> Ping()
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}", "ping");

            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(uriFragment, null);
           
            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(string), GetDefaultFormatter());

            return obj.ToString();
        }
#endif
#if NON_ASYNC
        public string Ping()
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}", "ping");

            HttpWebRequest request = requestBuilder.BuildGetRequest(uriFragment, null);

            var resp = Send(request);

            return DeserializeResponse<string>(resp);
        }
#endif
    }
}
