using Cryptonor;
using Cryptonor.Queries;
using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CryptonorClient
{
    public class CryptonorHttpClient
    {
        string uri;
        string dbName;
        public CryptonorHttpClient(string uri,string dbName)
        {
            this.uri = uri;
            this.dbName = dbName;
        }
        public async Task<CryptonorResultSet> Get(string bucket)
        {
            return await Get(bucket, 0, 0);
        }
        public async Task<CryptonorResultSet> Get(string bucket,int skip,int limit)
        {
            CryptonorResultSet result;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string finalURI = dbName + "/" + bucket;
                if (limit > 0 || skip > 0)
                {
                    if (limit == 0)
                    {
                        limit = 100;
                    }
                    finalURI += "?limit=" + limit;
                    if (skip > 0)
                    {
                        finalURI += "&skip="+skip;
                   
                    }
                }
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(finalURI);
                httpResponseMessage.EnsureSuccessStatusCode();
                List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
                formatters.Add(
                    new JsonMediaTypeFormatter());

                var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorResultSet), formatters);
                result = (CryptonorResultSet)obj;
            }
            return result;
        }
        public async Task<CryptonorObject> Get(string bucket,string key)
        {
            CryptonorObject result;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(dbName + "/" + bucket + "/" + key);
                httpResponseMessage.EnsureSuccessStatusCode();
                List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
                formatters.Add(
                    new JsonMediaTypeFormatter());
		        
                var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorObject), formatters);
                result = (CryptonorObject)obj;
            }
            return result;
        }
      
        public async Task Put(string bucket,CryptonorObject obj)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(dbName + "/" + bucket, obj, formatter);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    string responseBody = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    //throw new HttpException((int)response.StatusCode, responseBody);
                }
                var aa=httpResponseMessage.EnsureSuccessStatusCode();
                string h = ";;";
            }
        }
        public async Task Put(string bucket, IList<CryptonorObject> obj)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(dbName + "/" + bucket+"/batch", obj, formatter);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    string responseBody = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    
                }
                var aa = httpResponseMessage.EnsureSuccessStatusCode();
               
            }
        }
        internal async Task<CryptonorResultSet> GetByTag(string bucket, CryptonorQuery query)
        {
            using (HttpClient httpClient = new HttpClient())
            {
               
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(dbName + "/" + bucket+"/search", query, formatter);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    string responseBody = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    //throw new HttpException((int)response.StatusCode, responseBody);
                }
                var aa = httpResponseMessage.EnsureSuccessStatusCode();
                List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
                formatters.Add(
                    new JsonMediaTypeFormatter());
                var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(CryptonorResultSet), formatters);
                return (CryptonorResultSet)obj;
            }
        }



        internal async Task Delete(string bucket, string key)
        {
            using (HttpClient httpClient = new HttpClient())
            {

                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage httpResponseMessage = await httpClient.DeleteAsync(dbName + "/" + bucket + "/"+key);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    string responseBody = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    //throw new HttpException((int)response.StatusCode, responseBody);
                }
                var aa = httpResponseMessage.EnsureSuccessStatusCode();
              
            }
        }
    }
}
