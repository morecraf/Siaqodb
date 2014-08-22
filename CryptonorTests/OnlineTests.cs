using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptonorClient;

namespace CryptonorTests
{
    [TestClass]
    public class OnlineTests:BucketTests
    {
        public OnlineTests():base()
        {
           
           
        }
        public override CryptonorClient.IBucket GetBucket()
        {

            CryptonorClient.CryptonorClient client = new CryptonorClient.CryptonorClient("http://cryptonordb.cloudapp.net/cnor/", "excelsior", "mykey", "mypwd");
            IBucket bucket = client.GetBucket("unit_tests");
            return bucket;
        }
       
    }
}
