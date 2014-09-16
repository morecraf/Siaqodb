
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
#if ASYNC
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
#endif

namespace CryptonorClient.Http
{
    class RequestBuilder
    {
        string uriBase;
        public RequestBuilder(string uri)
        {
            this.uriBase = string.Format(CultureInfo.InvariantCulture, "{0}", uri);
           
        }
         #if NON_ASYNC
        public HttpWebRequest BuildGetRequest(string endUriFragment, Dictionary<string, string> parameters)
        {
            HttpWebRequest messageReq=null;
            var uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);

            var queryString = GetQueryString(parameters);
            messageReq = (HttpWebRequest)WebRequest.Create(CombinePathAndQuery(uriFragment, queryString));
            messageReq.Method = "GET";
            return messageReq;
        }
#endif

#if ASYNC
        public HttpRequestMessage BuildGetRequestAsync(string endUriFragment, Dictionary<string, string> parameters)
        {
            HttpRequestMessage messageReq = new HttpRequestMessage();
            messageReq.Method = HttpMethod.Get;
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);
          
           
            string queryString = GetQueryString(parameters);

            messageReq.RequestUri=new Uri(CombinePathAndQuery(uriFragment, queryString));
            return messageReq;
        }
#endif
         #if NON_ASYNC
        public HttpWebRequest BuildPostRequest(string endUriFragment)
        {
            return BuildPostRequest(endUriFragment,null);
        }
#endif

#if ASYNC
        public HttpRequestMessage BuildPostRequestAsync(string endUriFragment,object content)
        {
            return BuildPostRequestAsync(endUriFragment, content, null);
        }
#endif
         #if NON_ASYNC
        public HttpWebRequest BuildPostRequest(string endUriFragment, Dictionary<string, string> parameters)
        {
            HttpWebRequest request = null;
           
            var queryString = GetQueryString(parameters);
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);
            request = (HttpWebRequest)WebRequest.Create(CombinePathAndQuery(uriFragment, queryString));
            request.Method = "POST";
            return request;
        }
#endif

#if ASYNC
        public HttpRequestMessage BuildPostRequestAsync(string endUriFragment, object content, Dictionary<string, string> parameters)
        {
            HttpRequestMessage messageReq = new HttpRequestMessage();
            messageReq.Method = HttpMethod.Post;
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);
            string json = JsonConvert.SerializeObject(content);
            StringContent contentReq = new StringContent(json, Encoding.UTF8, "application/json");
            messageReq.Content = contentReq;
            string queryString = GetQueryString(parameters);
            messageReq.RequestUri = new Uri(CombinePathAndQuery(uriFragment, queryString));

            return messageReq;
        }
#endif
         #if NON_ASYNC
        public HttpWebRequest BuildDeleteRequest(string endUriFragment, Dictionary<string, string> parameters)
        {
            HttpWebRequest messageReq = null;
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);

            string queryString = GetQueryString(parameters);
            messageReq =(HttpWebRequest) WebRequest.Create(CombinePathAndQuery(uriFragment, queryString));
            messageReq.Method = "DELETE";
            return messageReq;
        }
#endif

#if ASYNC
        public HttpRequestMessage BuildDeleteRequestAsync(string endUriFragment, Dictionary<string, string> parameters)
        {
            HttpRequestMessage messageReq = new HttpRequestMessage();
            messageReq.Method = HttpMethod.Delete;
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);


            string queryString = GetQueryString(parameters);

            messageReq.RequestUri = new Uri(CombinePathAndQuery(uriFragment, queryString));
            return messageReq;
        }
#endif
        public static string GetQueryString(IDictionary<string, string> parameters)
        {
            string parametersString = null;

            if (parameters != null && parameters.Count > 0)
            {
                parametersString = "";
                string formatString = "{0}={1}";
                foreach (var parameter in parameters)
                {
                    string escapedKey = Uri.EscapeDataString(parameter.Key);
                    string escapedValue = Uri.EscapeDataString(parameter.Value);
                    parametersString += string.Format(CultureInfo.InvariantCulture,
                                                      formatString,
                                                      escapedKey,
                                                      escapedValue);
                    formatString = "&{0}={1}";
                }
            }

            return parametersString;
        }
        public static string CombinePathAndQuery(string path, string queryString)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                path = string.Format(CultureInfo.InvariantCulture, "{0}?{1}", path, queryString.TrimStart('?'));
            }

            return path;
        }
    }
}
