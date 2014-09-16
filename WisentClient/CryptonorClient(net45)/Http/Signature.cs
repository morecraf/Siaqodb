using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;

using System.Text;
#if ASYNC
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
#endif

#if WinRT
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.Security.Cryptography.Core;
using System.Runtime.InteropServices.WindowsRuntime;
#else
using System.Security.Cryptography;
#endif
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
 #if NON_ASYNC
        public void SignMessage(HttpWebRequest request, string jsonContent)
        {
            request.Headers.Add(ApplicationKeyHeaderName, appKey);
            request.Headers[TimestampHeaderName] = DateTime.UtcNow.ToString("o");
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.KeepAlive = false;
            string signature = MakeSignature(request, jsonContent, secretKey);
            request.Headers[SignatureHeaderName] = signature;
        }
        public void SignMessage(HttpWebRequest request)
        {
            this.SignMessage(request, "");
        }
#endif

#if ASYNC
        public async Task SignMessageAsync(HttpRequestMessage message)
        {
            message.Headers.Add(ApplicationKeyHeaderName, appKey);
            message.Headers.Add(TimestampHeaderName, DateTime.UtcNow.ToString("o"));
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string signature = await MakeSignatureAsync(message, secretKey);
            message.Headers.Add(SignatureHeaderName, signature);
        }
#endif
#if NON_ASYNC
        private static string MakeSignature(HttpWebRequest request, string jsonContent, string secretKey)
        {
            var hashedPassword = secretKey;
            var baseString = BuildBaseString(request, jsonContent);

            return ComputeHash(hashedPassword, baseString);

        }
#endif

#if ASYNC
        private async static Task<string> MakeSignatureAsync(HttpRequestMessage regMsg, string secretKey)
        {
            var hashedPassword = secretKey;
            var baseString = await BuildBaseStringAsync(regMsg);

            return ComputeHash(hashedPassword, baseString);

        }
#endif
        #if NON_ASYNC
        private static string GetHttpRequestHeader(WebHeaderCollection headers, string headerName)
        {
            if (!headers.AllKeys.Contains(headerName))
                return string.Empty;

            return headers[headerName];
        }
#endif


#if ASYNC
        private static string GetHttpRequestHeader(HttpHeaders headers, string headerName)
        {
            if (!headers.Contains(headerName))
                return string.Empty;

            return headers.GetValues(headerName)
                            .SingleOrDefault();
        }
#endif

        #if NON_ASYNC
        private static string BuildBaseString(HttpWebRequest request, string jsonContent)
        {
            var headers = request.Headers;
            string date = GetHttpRequestHeader(headers,TimestampHeaderName);

            string methodType = request.Method;

            var absolutePath = request.RequestUri.AbsolutePath.ToLower();
            var uri = Uri.UnescapeDataString(absolutePath);

            string parameterMessage = BuildParameterMessage(request, jsonContent);
            string message = string.Join("\n", new[] { methodType, date, uri, parameterMessage });

            return message;
        }

#endif

#if ASYNC
        private static async Task<string> BuildBaseStringAsync(HttpRequestMessage request)
        {
            var headers = request.Headers;
            string date = GetHttpRequestHeader(headers, TimestampHeaderName);

            string methodType = request.Method.Method;

            var absolutePath = request.RequestUri.AbsolutePath.ToLower();
            var uri = Uri.UnescapeDataString(absolutePath);

            string parameterMessage = await BuildParameterMessageAsync(request);
            string message = string.Join("\n", methodType, date, uri, parameterMessage);

            return message;
        }
#endif

        private static string ComputeHash(string hashedPassword, string message)
        {
            var key = Encoding.UTF8.GetBytes(hashedPassword.ToUpper());
            string hashString;
#if WinRT
            MacAlgorithmProvider macAlgorithmProvider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var binaryMessage = CryptographicBuffer.ConvertStringToBinary(message, BinaryStringEncoding.Utf8);
            var binaryKeyMaterial = key.AsBuffer();
            var hmacKey = macAlgorithmProvider.CreateKey(binaryKeyMaterial);
            var binarySignedMessage = CryptographicEngine.Sign(hmacKey, binaryMessage);
            var signedMessage = CryptographicBuffer.EncodeToBase64String(binarySignedMessage);
            return signedMessage;
#else

            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                hashString = Convert.ToBase64String(hash);
            }
#endif
            return hashString;
        }

        #if NON_ASYNC
        private static string BuildParameterMessage(HttpWebRequest request, string jsonContent)
        {
            var parameterCollection = BuildParameterCollection(request, jsonContent);
            if (!parameterCollection.Any())
                return string.Empty;

            var keyValueStrings = parameterCollection.Select(pair =>
                string.Format("{0}={1}", pair.Key, pair.Value));

            return string.Join("&", keyValueStrings.ToArray());
        }
#endif

#if ASYNC
        private static async Task<string> BuildParameterMessageAsync(HttpRequestMessage request)
        {
            var parameterCollection = await BuildParameterCollectionAsync(request);
            if (!parameterCollection.Any())
                return string.Empty;

            var keyValueStrings = parameterCollection.Select(pair =>
                string.Format("{0}={1}", pair.Key, pair.Value));

            return string.Join("&", keyValueStrings);
        }

#endif

        #if NON_ASYNC
        private static List<KeyValuePair<string, string>> BuildParameterCollection(HttpWebRequest request, string jsonContent)
        {
            // Use the list of keyvalue pair in order to allow the same key instead of dictionary
            var parameterCollection = new List<KeyValuePair<string, string>>();

            var queryStringCollection = CompatibilityHelper.ParseQueryString(request.RequestUri);

            AddKeyValuesToCollection(parameterCollection, queryStringCollection);

            if (jsonContent.Length>0)
            {
                parameterCollection.Add(new KeyValuePair<string, string>("content", jsonContent));
            }

            return parameterCollection.OrderBy(pair => pair.Key).ToList();
        }
#endif

#if ASYNC
        private static async Task<List<KeyValuePair<string, string>>> BuildParameterCollectionAsync(HttpRequestMessage request)
        {
            // Use the list of keyvalue pair in order to allow the same key instead of dictionary
            var parameterCollection = new List<KeyValuePair<string, string>>();

            var queryStringCollection = CompatibilityHelper.ParseQueryString(request.RequestUri);
         
            AddKeyValuesToCollection(parameterCollection,queryStringCollection);

            if (request.Content != null)
            {
                var jsonContent = await request.Content.ReadAsStringAsync();
                parameterCollection.Add(new KeyValuePair<string, string>("content", jsonContent));
            }


            return parameterCollection.OrderBy(pair => pair.Key).ToList();
        }
#endif
        private static void AddKeyValuesToCollection(List<KeyValuePair<string, string>> parameterCollection,
           IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        {
            if (!nameValueCollection.Any())
                return;

            foreach (var key in nameValueCollection)
            {
               
                parameterCollection.Add(key);
            }
        }

    }
}
