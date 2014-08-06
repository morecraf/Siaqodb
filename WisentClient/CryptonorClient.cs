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
        public CryptonorClient(string uri,string dbName)
        {
            this.uri = uri;
            this.dbName = dbName;
        }
        public IBucket GetBucket(string bucketName)
        {
            return new CryptonorBucket(bucketName,this.uri,this.dbName);
        }
        public IBucket GetLocalBucket(string bucketName,string localFolder)
        {
            return new CryptonorLocalBucket(bucketName,localFolder,uri,dbName);
        }

    }
}
