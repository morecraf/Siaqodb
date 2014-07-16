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
        public async Task<IEnumerable<CryptonorObject>> Get(string bucket)
        {
            IEnumerable<CryptonorObject> result;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(dbName + "/" + bucket);
                httpResponseMessage.EnsureSuccessStatusCode();
                List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
                formatters.Add(
                    new JsonMediaTypeFormatter());

                var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(IEnumerable<CryptonorObject>), formatters);
                result = (IEnumerable<CryptonorObject>)obj;
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
        public async Task<IEnumerable<CryptonorObject>> GetByTag(string bucket,string tagName,string op,object value)
        {
            IEnumerable<CryptonorObject> result;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(this.uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string uriLocal = dbName + "/" + bucket + "/" + Mapper.GetTagByType(value.GetType()) + "/" + tagName + "/" + op + "/" + Mapper.URLEncode(value);
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(uriLocal);
                httpResponseMessage.EnsureSuccessStatusCode();
                List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
                formatters.Add(
                    new JsonMediaTypeFormatter());

                var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(IEnumerable<CryptonorObject>), formatters);
                result = (IEnumerable<CryptonorObject>)obj;
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


    }
}
