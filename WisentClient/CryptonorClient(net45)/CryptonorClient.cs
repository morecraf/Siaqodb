

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
        string username;
        string password;

        public CryptonorClient(string uri,string username,string password)
        {
            httpClient = new CryptonorHttpClient(uri, username, password);
            this.uri = uri;
            this.username = username;
            this.password = password;
        }
        public IBucket GetBucket(string bucketName)
        {
            return new CryptonorBucket(this.uri, bucketName, this.username, this.password);
            
        }
        public IBucket GetLocalBucket(string bucketName,string localFolder)
        {
            return new CryptonorLocalBucket(uri, bucketName, localFolder, this.username, this.password);
        }
#if ASYNC
        public async Task<List<string>> GetAllBucketsAsync()
        {
            return await httpClient.GetAllAsync();
        }
#endif
    
    }
}
