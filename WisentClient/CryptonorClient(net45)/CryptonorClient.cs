

using System.Collections.Generic;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace CryptonorClient
{
    public class CryptonorClient
    {
        private CryptonorHttpClient httpClient;
        string uri;
        string appKey;
        string secretKey;

        public CryptonorClient(string uri,string appKey,string secretKey)
        {
            httpClient = new CryptonorHttpClient(uri, appKey, secretKey);
            this.uri = uri;
            this.appKey = appKey;
            this.secretKey = secretKey;
        }
        public IBucket GetBucket(string bucketName)
        {
            return new CryptonorBucket(this.uri,bucketName,this.appKey,this.secretKey);
            
        }
        public IBucket GetLocalBucket(string bucketName,string localFolder)
        {
            return new CryptonorLocalBucket(uri, bucketName, localFolder, this.appKey, this.secretKey);
        }
#if ASYNC
        public async Task<List<string>> GetAllBucketsAsync()
        {
            return await httpClient.GetAllAsync();
        }
#endif
    }
}
