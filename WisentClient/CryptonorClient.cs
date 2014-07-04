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

namespace Cryptonor
{
    public class CryptonorClient
    {
        public async Task<DotissiObject> Get(string bucket,string key)
        {
            DotissiObject result;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://localhost:53411/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/excelsior/"+bucket+"/"+key);
                httpResponseMessage.EnsureSuccessStatusCode();
                List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
                formatters.Add(
                    new JsonMediaTypeFormatter());
		        
                var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(DotissiObject), formatters);
                result = (DotissiObject)obj;
            }
            return result;
        }
        public async Task<IEnumerable<DotissiObject>> GetByTag(string bucket,string tagName,string op,object value)
        {
            IEnumerable<DotissiObject> result;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://localhost:53411/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string uri = "/excelsior/" + bucket + "/" + Mapper.GetTagByType(value.GetType()) + "/" + tagName + "/" + op + "/" + Mapper.URLEncode(value);
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(uri);
                httpResponseMessage.EnsureSuccessStatusCode();
                List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
                formatters.Add(
                    new JsonMediaTypeFormatter());

                var obj = await httpResponseMessage.Content.ReadAsAsync(typeof(IEnumerable<DotissiObject>), formatters);
                result = (IEnumerable<DotissiObject>)obj;
            }
            return result;
        }
        public async Task Put(string bucket,DotissiObject obj)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://localhost:53411/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("excelsior/"+bucket, obj, formatter);
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
