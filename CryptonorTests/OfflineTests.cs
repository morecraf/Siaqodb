using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptonorClient;
namespace CryptonorTests
{
    [TestClass]
    public class OfflineTests:BucketTests
    {
        public OfflineTests():base()
        {

        }
        public override CryptonorClient.IBucket GetBucket()
        {

            CryptonorClient.CryptonorClient client = new CryptonorClient.CryptonorClient("http://cryptonordb.cloudapp.net/cnor/", "excelsior", "mykey", "mypwd");
            IBucket bucket = client.GetLocalBucket("unit_tests", @"c:\work\temp\unitests\");
            return bucket;
        }
    }
}
