using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptonorClient;
using System.Net;

namespace CryptonorTests
{
    [TestClass]
    public class OnlineTests:BucketTestsSync
    {
        public OnlineTests():base()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender1, cert, chain, sslPolicyErrors) => true;

           
        }
        public override CryptonorClient.IBucket GetBucket()
        {
            CryptonorClient.CryptonorClient client = new CryptonorClient.CryptonorClient(Init.Api_URL, Init.Username, Init.Password);
            IBucket bucket = client.GetBucket("unit_tests");
            return bucket;
        }
       
    }
}
