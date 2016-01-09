using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using SiaqodbCloudService.Repository;

namespace SiaqodbCloudService.Filters
{
    public class AuthenticateAttribute : ActionFilterAttribute
    {
       
        private const string Access_Key_Id = "X-SQO-ACCESS_KEY_ID";
        private const string TimestampHeaderName = "X-SQO-DATE";
        private const string Signature = "X-SQO-SIGNATURE";
        IRepository repo;
        public AuthenticateAttribute()
        {
            repo = RepositoryFactory.GetRepository();
           
        }
        private  string GetHttpRequestHeader(HttpHeaders headers, string headerName)
        {
            if (!headers.Contains(headerName))
                return string.Empty;

            return headers.GetValues(headerName)
                            .SingleOrDefault();
        }

        public  string GetHttpRequestAccessKeyIdHeader(HttpHeaders headers)
        {
            return GetHttpRequestHeader(headers, Access_Key_Id);
        }
        public  string GetHttpRequestTimestampHeader(HttpHeaders headers)
        {
            return GetHttpRequestHeader(headers, TimestampHeaderName);
        }
        public  string GetHttpRequestSignatureHeader(HttpHeaders headers)
        {
            return GetHttpRequestHeader(headers, Signature);
        }
        private string ComputeHash(string hashedPassword, string message)
        {
            var key = Encoding.UTF8.GetBytes(hashedPassword);
            string hashString;

            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                hashString = Convert.ToBase64String(hash);
            }

            return hashString;
        }

        private void AddNameValuesToCollection(List<KeyValuePair<string, string>> parameterCollection,
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

        private async Task<List<KeyValuePair<string, string>>> BuildParameterCollection(HttpActionContext actionContext)
        {
            // Use the list of keyvalue pair in order to allow the same key instead of dictionary
            var parameterCollection = new List<KeyValuePair<string, string>>();

            var queryStringCollection = actionContext.Request.RequestUri.ParseQueryString();
            AddNameValuesToCollection(parameterCollection, queryStringCollection);
            // var formCollection = HttpContext.Current.Request.Form;
            HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream);
            string content = await reader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(content))
            {
                parameterCollection.Add(new KeyValuePair<string, string>("content", content));

            }

            return parameterCollection.OrderBy(pair => pair.Key).ToList();
        }

        private async Task<string> BuildParameterMessage(HttpActionContext actionContext)
        {
            var parameterCollection = await BuildParameterCollection(actionContext);
            if (!parameterCollection.Any())
                return string.Empty;

            var keyValueStrings = parameterCollection.Select(pair =>
                string.Format("{0}={1}", pair.Key, pair.Value));

            return string.Join("&", keyValueStrings);
        }

        private async Task<string> BuildBaseString(HttpActionContext actionContext)
        {
            var headers = actionContext.Request.Headers;
            string date = GetHttpRequestTimestampHeader(headers);

            string methodType = actionContext.Request.Method.Method;

            var absolutePath = actionContext.Request.RequestUri.AbsolutePath.ToLower();
            var uri = HttpContext.Current.Server.UrlDecode(absolutePath);

            string parameterMessage = await BuildParameterMessage(actionContext);
            string message = string.Join("\n", methodType, date, uri, parameterMessage);

            return message;
        }

        private bool AreSignaturesEqual(string hashedPassword, string message, string signature)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            var verifiedHash = ComputeHash(hashedPassword, message);
            if (signature != null && signature.Equals(verifiedHash))
                return true;

            return false;
        }

        private bool IsDateValidated(string timestampString)
        {
            DateTime timestamp;

            bool isDateTime = DateTime.TryParseExact(timestampString, "o", null,
                DateTimeStyles.AdjustToUniversal, out timestamp);

            if (!isDateTime)
                return false;

            var now = DateTime.UtcNow;

            // TimeStamp should not be in 5 minutes behind
            if (timestamp < now.AddMinutes(-5))
                return false;

            if (timestamp > now.AddMinutes(5))
                return false;

            return true;
        }

        private bool IsSignatureValidated(string signature)
        {
            var cache= MemoryCache.Default;
           
            if (cache.Contains(signature))
                return false;

            return true;
        }

        private void AddToMemoryCache(string signature)
        {
            var cache = MemoryCache.Default;

            if (!cache.Contains(signature))
            {
                var now = DateTimeOffset.UtcNow;
                var expiration = now.AddMinutes(5);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = expiration;
            }
        }

        private async Task<bool> IsAuthenticated(HttpActionContext actionContext)
        {
            var headers = actionContext.Request.Headers;

            var timeStampString = GetHttpRequestTimestampHeader(headers);
            if (!IsDateValidated(timeStampString))
                return false;

            var accessKeyId = GetHttpRequestAccessKeyIdHeader(headers);
            if (string.IsNullOrEmpty(accessKeyId))
                return false;

            var signature = GetHttpRequestSignatureHeader(headers);
            if (string.IsNullOrEmpty(signature))
                return false;

            if (!IsSignatureValidated(signature))
                return false;

            AddToMemoryCache(signature);

            var pwd = await repo.GetSecretAccessKey(accessKeyId);
            if (pwd == null)
                return false;
            var baseString = await BuildBaseString(actionContext);
            return AreSignaturesEqual(pwd, baseString, signature);

        }

       
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {

            bool isAuthenticated = await IsAuthenticated(actionContext);

            if (!isAuthenticated)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                response.Content = new StringContent("Unauthorized");
                actionContext.Response = response;

                return;
            }
            else
            {
                await base.OnActionExecutingAsync(actionContext, cancellationToken);
            }

        }
       
    }
}