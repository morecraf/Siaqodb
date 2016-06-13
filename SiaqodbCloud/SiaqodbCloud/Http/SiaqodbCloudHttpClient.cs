using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Sqo.Documents;
using Sqo.Documents.Sync;

#if ASYNC
using System.Net.Http.Formatting;

using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
#endif
using System.Web;

namespace SiaqodbCloud
{
    internal class SiaqodbCloudHttpClient : IDisposable, ISiaqodbCloudClient
    {
        string uri;

#if ASYNC
        HttpClient httpClient;
#endif
        RequestBuilder requestBuilder;
        Signature signature;
        public SiaqodbCloudHttpClient(string uri, string username, string password)
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


            ResultSet rset = DeserializeResponse<ResultSet>(resp);
            resp.Close();
            return rset;
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
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(ResultSet), GetDefaultFormatter());

            return (ResultSet)obj;
        }
#endif
#if NON_ASYNC
        public Document Get(string bucket, string key, string version = null)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);
            Dictionary<string, string> parameters = null;
            if (version != null)
            {
                parameters = new Dictionary<string, string>();
                parameters["version"] = version;
            }
            HttpWebRequest request = requestBuilder.BuildGetRequest(uriFragment, parameters);

            var resp = Send(request);
            if (resp == null)
                return null;

            Document crobj = DeserializeResponse<Document>(resp);
            resp.Close();
            return crobj;
        }
#endif

#if ASYNC
        public async Task<Document> GetAsync(string bucket, string key, string version = null)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);
            Dictionary<string, string> parameters = null;

            if (version != null)
            {
                parameters = new Dictionary<string, string>();
                parameters["version"] = version;
            }
            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(uriFragment, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            if (httpResponseMessage == null)//document not found
                return null;

            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(Document), GetDefaultFormatter());

            return (Document)obj;
        }
#endif
#if NON_ASYNC
        public StoreResponse Put(string bucket, Document obj)
        {
            string uriFragment = bucket;
            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment);

#if !UNITY3D && !CF
            var resp = Post(request, Newtonsoft.Json.JsonConvert.SerializeObject(obj));
#else
            var resp = Post(request, LitJson.JsonMapper.ToJson(obj));
#endif

            StoreResponse sres = DeserializeResponse<StoreResponse>(resp);
            resp.Close();
            return sres;

        }
#endif

#if ASYNC
        public async Task<StoreResponse> PutAsync(string bucket, Document obj)
        {
            string uriFragment = bucket;
            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, obj);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);

            var objResp = await httpResponseMessage.Content.ReadAsAsync(typeof(StoreResponse), GetDefaultFormatter());
            return (StoreResponse)objResp;

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
            BatchResponse bres = DeserializeResponse<BatchResponse>(resp);
            resp.Close();
            return bres;
            
        }
#endif

#if ASYNC
        public async Task<BatchResponse> PutAsync(string bucket, ChangeSet batch)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "batch");

            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, batch);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);

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

            ResultSet rset = DeserializeResponse<ResultSet>(resp);
            resp.Close();
            return rset;
        }
#endif

#if ASYNC
        internal async Task<ResultSet> GetByTagAsync(string bucket, Query query)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, "search");

            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, query);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);

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
        public ChangeSet GetChanges(string bucket, Filter query, int limit, string anchor,string uploadAnchor)
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
            if (!string.IsNullOrEmpty(uploadAnchor))
            {
                parameters.Add("uploadanchor", uploadAnchor);
            }

            HttpWebRequest request = requestBuilder.BuildPostRequest(uriFragment, parameters);
#if !UNITY3D && !CF
            var resp = Post(request, Newtonsoft.Json.JsonConvert.SerializeObject(query));
#else
            var resp = Post(request, LitJson.JsonMapper.ToJson(query));
#endif
            ChangeSet cset = DeserializeResponse<ChangeSet>(resp);
            resp.Close();
            return cset;
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
        public async Task<ChangeSet> GetChangesAsync(string bucket, Filter query, int limit, string anchor,string uploadAnchor)
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
            if (!string.IsNullOrEmpty(uploadAnchor))
            {
                parameters.Add("uploadanchor", uploadAnchor);
            }
            HttpRequestMessage request = requestBuilder.BuildPostRequestAsync(uriFragment, query, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(ChangeSet), GetDefaultFormatter());

            return (ChangeSet)obj;

        }
#endif
#if NON_ASYNC
        public ChangeSet GetChanges(string bucket, int limit, string anchor,string uploadAnchor)
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
            if (!string.IsNullOrEmpty(uploadAnchor))
            {
                parameters.Add("uploadanchor", uploadAnchor);
            }
            var request = requestBuilder.BuildGetRequest(uriFragment, parameters);

            var resp = Send(request);



            ChangeSet cset = DeserializeResponse<ChangeSet>(resp);
            resp.Close();
            return cset;
        }
#endif

#if ASYNC
        public async Task<ChangeSet> GetChangesAsync(string bucket, int limit, string anchor,string uploadAnchor)
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
            if (!string.IsNullOrEmpty(uploadAnchor))
            {
                parameters.Add("uploadanchor", uploadAnchor);
            }
            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(uriFragment, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);

            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(ChangeSet), GetDefaultFormatter());

            return (ChangeSet)obj;

        }

#endif
#if NON_ASYNC
        public void Delete(string bucket, string key, string version)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (version != null)
            {
                parameters.Add("version", version);
            }
            HttpWebRequest request = requestBuilder.BuildDeleteRequest(uriFragment, parameters);

            var resp = Send(request);
            if (resp != null)
            {
                resp.Close();
            }
        }

        private HttpWebResponse Send(HttpWebRequest request)
        {
            signature.SignMessage(request);
            try
            {
                request.KeepAlive = true;
                var resp = (HttpWebResponse)request.GetResponse();
                return resp;
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response.StatusCode == HttpStatusCode.NotFound && response.StatusDescription == "document_not_found")
                {
                    return null;
                }
                else
                {
                    ThrowMeaningfulEx(ex);
                }
                throw ex;
            }
        }
        private HttpWebResponse Post(HttpWebRequest request, string jsonContent)
        {
            signature.SignMessage(request, jsonContent);

            if (jsonContent != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(jsonContent);
                request.ContentLength = bytes.Length;
                Stream ps = request.GetRequestStream();
                ps.Write(bytes, 0, bytes.Length);
                ps.Close();
            }
            try
            {
                request.KeepAlive = true;
                var resp = (HttpWebResponse)request.GetResponse();

                return resp;
            }
            catch (WebException ex)
            {
                ThrowMeaningfulEx(ex);
                throw ex;
            }
        }
        private void ThrowMeaningfulEx(WebException ex)
        {
            HttpWebResponse response = (HttpWebResponse)ex.Response;

            if (response.StatusCode == HttpStatusCode.NotFound && response.StatusDescription == "bucket_not_found")
            {
                var errorDesc = ReadResponseContent(response);
                if (errorDesc == null)
                {
                    errorDesc = response.StatusDescription;
                }
                throw new BucketNotFoundException(errorDesc.ToString());

            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorDesc = ReadResponseContent(response);
                if (errorDesc == null)
                {
                    errorDesc = response.StatusDescription;
                }
                throw new ConflictException(errorDesc.ToString());
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest && response.StatusDescription == "version_bad_format")
            {
                throw new InvalidVersionFormatException(response.StatusDescription);
            }

        }
        private string ReadResponseContent(HttpWebResponse response)
        {
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }
#endif

#if ASYNC
        public async Task DeleteAsync(string bucket, string key, string version)
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", bucket, key);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (version != null)
            {
                parameters.Add("version", version);
            }
            HttpRequestMessage request = requestBuilder.BuildDeleteRequestAsync(uriFragment, parameters);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);

        }

#endif

#if ASYNC
        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            await signature.SignMessageAsync(request);
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound && response.ReasonPhrase == "document_not_found")
            {
                return null;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound && response.ReasonPhrase == "bucket_not_found")
            {
                var errorDesc = await response.Content.ReadAsStringAsync();
                if (errorDesc == null)
                {
                    errorDesc = response.ReasonPhrase;
                }
                throw new BucketNotFoundException(errorDesc.ToString());

            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorDesc = await response.Content.ReadAsStringAsync();
                if (errorDesc == null)
                {
                    errorDesc = response.ReasonPhrase;
                }
                throw new ConflictException(errorDesc.ToString());
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest && response.ReasonPhrase == "version_bad_format")
            {
                throw new InvalidVersionFormatException(response.ReasonPhrase);
            }
            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        public IEnumerable<MediaTypeFormatter> GetDefaultFormatter()
        {
            var formatters = new List<MediaTypeFormatter> { new JsonMediaTypeFormatter() };
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
        public async Task<BucketSet> GetBucketsAsync()
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}", "allBuckets");

            HttpRequestMessage request = requestBuilder.BuildGetRequestAsync(uriFragment, null);

            HttpResponseMessage httpResponseMessage = await this.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(BucketSet), GetDefaultFormatter());

            return (BucketSet)obj;
        }
#endif
#if NON_ASYNC
        public BucketSet GetBuckets()
        {
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}", "allBuckets");

            HttpWebRequest request = requestBuilder.BuildGetRequest(uriFragment, null);

            var resp = Send(request);

            BucketSet bset = DeserializeResponse<BucketSet>(resp);
            resp.Close();
            return bset;
        }
#endif
#if ASYNC
        public async Task<string> PingAsync()
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

            string re = DeserializeResponse<string>(resp);
            resp.Close();
            return re;
        }
#endif
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    class ResultSet
    {
        public int Total { get; set; }
        public int Count { get; set; }
        public List<Document> Objects { get; set; }
        public List<T> GetContentValues<T>()
        {
            List<T> list = new List<T>();
            foreach (Document current in Objects)
            {
                list.Add(current.GetContent<T>());
            }
            return list;
        }
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    class StoreResponse
    {
        public string Key { get; set; }
        public string Version { get; set; }
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    class BatchItemResponse
    {
        public string Error { get; set; }
        public string ErrorDesc { get; set; }
        public string Version { get; set; }
        public string Key { get; set; }


    }
    [System.Reflection.Obfuscation(Exclude = true)]
    class BatchResponse
    {

        public List<BatchItemResponse> BatchItemResponses { get; set; }
        public int ItemsWithErrors { get; set; }
        public int Total { get; set; }
        public string UploadAnchor { get; set; }
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    class BucketSet
    {
        public List<string> Buckets;
        public int Total { get; set; }

    }
}
