using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.Http
{
    class RequestBuilder
    {
        string uriBase;
        public RequestBuilder(string uri, string dbName)
        {
            this.uriBase = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uri, dbName);
           
        }
        public HttpRequestMessage BuildGetRequest(string endUriFragment, Dictionary<string, string> parameters)
        {
            HttpRequestMessage messageReq = new HttpRequestMessage();
            messageReq.Method = HttpMethod.Get;
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);
          
           
            string queryString = GetQueryString(parameters);

            messageReq.RequestUri=new Uri(CombinePathAndQuery(uriFragment, queryString));
            return messageReq;
        }
        public HttpRequestMessage BuildPostRequest(string endUriFragment,object content)
        {
            HttpRequestMessage messageReq = new HttpRequestMessage();
            messageReq.Method = HttpMethod.Post;
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);
            string json =JsonConvert.SerializeObject(content);
            StringContent contentReq = new StringContent(json,Encoding.UTF8, "application/json");
            messageReq.Content = contentReq;
            messageReq.RequestUri = new Uri(uriFragment);
            
            return messageReq;
        }
        public HttpRequestMessage BuildDeleteRequest(string endUriFragment, Dictionary<string, string> parameters)
        {
            HttpRequestMessage messageReq = new HttpRequestMessage();
            messageReq.Method = HttpMethod.Delete;
            string uriFragment = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBase, endUriFragment);


            string queryString = GetQueryString(parameters);

            messageReq.RequestUri = new Uri(CombinePathAndQuery(uriFragment, queryString));
            return messageReq;
        }
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
