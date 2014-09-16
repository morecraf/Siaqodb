using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptonorClient;

namespace CryptonorTests
{
    [TestClass]
    public class OnlineTestsSync:BucketTestsSync
    {
        public OnlineTestsSync():base()
        {
           
           
        }
        public override CryptonorClient.IBucket GetBucket()
        {
            CryptonorClient.CryptonorClient client = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "excelsior", "b8d2f15848b12927d50d0037510013c8", "v8zQGiAjyl");
            IBucket bucket = client.GetBucket("iasi");
            return bucket;
        }
       
    }
}
