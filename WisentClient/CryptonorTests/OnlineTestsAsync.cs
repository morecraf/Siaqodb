using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptonorClient;

namespace CryptonorTests
{
    [TestClass]
    public class OnlineTestsAsync:BucketTestsAsync
    {
        public OnlineTestsAsync():base()
        {
           
           
        }
        public override CryptonorClient.IBucket GetBucket()
        {
            CryptonorClient.CryptonorClient client = new CryptonorClient.CryptonorClient(Init.Api_URL, Init.Username, Init.Password);
            IBucket bucket = client.GetBucket("unit_tests");
            return bucket;
        }
       
    }
}
