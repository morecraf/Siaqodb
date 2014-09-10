using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public class CryptonorClient
    {
        string uri;
        string dbName;
        string appKey;
        string secretKey;

        public CryptonorClient(string uri,string dbName,string appKey,string secretKey)
        {
            this.uri = uri;
            this.dbName = dbName;
            this.appKey = appKey;
            this.secretKey = secretKey;
        }
        public IBucket GetBucket(string bucketName)
        {
            return new CryptonorBucket(this.uri,this.dbName,bucketName,this.appKey,this.secretKey);
            
        }
        public IBucket GetLocalBucket(string bucketName,string localFolder)
        {
            return new CryptonorLocalBucket(uri, dbName, bucketName, localFolder, this.appKey, this.secretKey);
        }

    }
}
