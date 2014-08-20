using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.Http
{
    class Signature
    {
        private const string ApplicationKeyHeaderName = "X-CNOR-Application";
        private const string TimestampHeaderName = "X-CNOR-Date";
        private const string SignatureHeaderName = "X-CNOR-Signature";
        string appKey;
        string secretKey;
        public Signature(string appKey, string secretKey)
        {
            this.secretKey = secretKey;
            this.appKey = appKey;
        }
        public async Task SignMessage(HttpRequestMessage message)
        {
            message.Headers.Add(ApplicationKeyHeaderName, appKey);
            message.Headers.Add(TimestampHeaderName, DateTime.UtcNow.ToString("o"));
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string signature = await MakeSignature(message,secretKey);
            message.Headers.Add(SignatureHeaderName, signature);
        }
        private async static Task<string> MakeSignature(HttpRequestMessage regMsg, string secretKey)
        {
            var hashedPassword = secretKey;
            var baseString = await BuildBaseString(regMsg);

            return ComputeHash(hashedPassword, baseString);

        }

        private static string GetHttpRequestHeader(HttpHeaders headers, string headerName)
        {
            if (!headers.Contains(headerName))
                return string.Empty;

            return headers.GetValues(headerName)
                            .SingleOrDefault();
        }
        private string GetPrivateKey(string appKey)
        {
            return "mypwd";
        }
        private static async Task<string> BuildBaseString(HttpRequestMessage reqMsg)
        {
            var headers = reqMsg.Headers;
            string date = GetHttpRequestHeader(headers, TimestampHeaderName);

            string methodType = reqMsg.Method.Method;

            var absolutePath = reqMsg.RequestUri.AbsolutePath.ToLower();
            var uri = Uri.UnescapeDataString(absolutePath);

            string parameterMessage = await BuildParameterMessage(reqMsg);
            string message = string.Join("\n", methodType, date, uri, parameterMessage);

            return message;
        }
        private static string ComputeHash(string hashedPassword, string message)
        {
            var key = Encoding.UTF8.GetBytes(hashedPassword.ToUpper());
            string hashString;

            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                hashString = Convert.ToBase64String(hash);
            }

            return hashString;
        }
        private static async Task<string> BuildParameterMessage(HttpRequestMessage reqMsg)
        {
            var parameterCollection = await BuildParameterCollection(reqMsg);
            if (!parameterCollection.Any())
                return string.Empty;

            var keyValueStrings = parameterCollection.Select(pair =>
                string.Format("{0}={1}", pair.Key, pair.Value));

            return string.Join("&", keyValueStrings);
        }
        private static async Task<List<KeyValuePair<string, string>>> BuildParameterCollection(HttpRequestMessage reqMsg)
        {
            // Use the list of keyvalue pair in order to allow the same key instead of dictionary
            var parameterCollection = new List<KeyValuePair<string, string>>();

            var queryStringCollection = reqMsg.RequestUri.ParseQueryString();
            AddNameValuesToCollection(parameterCollection, queryStringCollection);

            if (reqMsg.Content != null)
            {
                var jsonContent = await reqMsg.Content.ReadAsStringAsync();
                parameterCollection.Add(new KeyValuePair<string, string>("content", jsonContent));
            }


            return parameterCollection.OrderBy(pair => pair.Key).ToList();
        }
        private static void AddNameValuesToCollection(List<KeyValuePair<string, string>> parameterCollection,
            NameValueCollection nameValueCollection)
        {
            if (!nameValueCollection.AllKeys.Any())
                return;

            foreach (var key in nameValueCollection.AllKeys)
            {
                var value = nameValueCollection[key];
                var pair = new KeyValuePair<string, string>(key, value);

                parameterCollection.Add(pair);
            }
        }
    }
}
