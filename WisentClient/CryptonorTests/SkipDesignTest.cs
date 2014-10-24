using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Cryptonor;
using CryptonorClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CryptonorTests
{
    [TestClass]
    public class SkipDesignTest
    {
        private static int nrOfDocsLeftSide,nrOfDocsRightSide ;
        private IBucket bucketRead;
        private static object[] tags = new object[]
            {
                new {tag1 = 1},
                new {tag2 = 2},
                new {tag3 = 3},
                new {tag4 = 4}
            };

        public SkipDesignTest()
        {
            nrOfDocsLeftSide =4;
            nrOfDocsRightSide = 4;
            CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.Camellia128, "mysuper_secret");
            var cl1 = new CryptonorClient.CryptonorClient("http://localhost:53411/v0/", "adae07ae8345bac710aab525390004fb", "VuH6OD4YWY0vexnlCvOA");
            bucketRead = cl1.GetBucket("testbucket");
        }

        //CASE 1: LIMIT < DESIGN DOCS OFFSET 
        [TestMethod]
        public async Task DesignGroupAtStartTest1()
        {
            await DeleteDocs();
            await InitStartDocuments();

            var result = new ResultSet[3];
            var expected = new[] { new[] { "a0", "a1", "a2" }, new[] { "a3" } };
            int skip = 0, limit = 3;

            await TestExpectedResult(result, skip, limit, expected, nrOfDocsLeftSide);
            await DeleteDocs();
        }
        //CASE 1: LIMIT < DESIGN DOCS OFFSET 
        [TestMethod]
        public async Task DesignGroupAtStartTest2()
        {
            await DeleteDocs();
            await InitStartDocuments();

            var result = new ResultSet[3];
            var expected = new[] { new[] { "a0", "a1" , "a2", "a3" } };
            int skip = 0, limit = 7;

            await TestExpectedResult(result, skip, limit, expected, nrOfDocsLeftSide);
            await DeleteDocs();
        }


        //CASE 1: LIMIT < DESIGN DOCS OFFSET 
        [TestMethod]
        public async Task DesignGroupInTheMiddleTest1()
        {
            await DeleteDocs();
            await InitMiddleDocuments();

            var result = new ResultSet[3];
            var expected = new[] {new[] {"A0","A1","A2"}, new[] {"A3","a0","a1"}, new[] {"a2","a3"}};
            int skip=0,limit = 3;

            await TestExpectedResult(result, skip, limit, expected, nrOfDocsRightSide + nrOfDocsLeftSide);
            await DeleteDocs();
        }

        //CASE 2: LIMIT > DESIGN DOCS OFFSET AND < NR OF DESIGN DOCUMENTS
        [TestMethod]
        public async Task DesignGroupInTheMiddleTest2()
        {
            await DeleteDocs();
            await InitMiddleDocuments();

            var result = new ResultSet[3];
            var expected = new[] { new[] { "A0", "A1", "A2", "A3", "a0" }, new[] { "a1", "a2", "a3" } };
            int skip = 0, limit = 5;

            await TestExpectedResult(result, skip, limit, expected, nrOfDocsRightSide + nrOfDocsLeftSide);
            await DeleteDocs();
        }

        //CASE 3: LIMIT > NR OF DESIGN DOCUMENTS
        [TestMethod]
        public async Task DesignGroupInTheMiddleTest3()
        {
            await DeleteDocs();
            await InitMiddleDocuments();

            var result = new ResultSet[3];
            var expected = new[] { new[] { "A0", "A1", "A2", "A3", "a0" ,"a1", "a2", "a3" } };
            int skip = 0, limit = 10;

            await TestExpectedResult(result, skip, limit, expected,nrOfDocsRightSide+nrOfDocsLeftSide);
            await DeleteDocs();
        }

        //CASE 2: LIMIT > DESIGN DOCS OFFSET AND < NR OF DESIGN DOCUMENTS
        [TestMethod]
        public async Task DesignGroupInTheEndTest2()
        {
            await DeleteDocs();
            await InitEndDocuments();

            var result = new ResultSet[3];
            var expected = new[] { new[] { "A0", "A1", "A2", "A3"} };
            int skip = 0, limit = 5;

            await TestExpectedResult(result, skip, limit, expected,nrOfDocsRightSide);
            await DeleteDocs();
        }

        //CASE 3: LIMIT > NR OF DESIGN DOCUMENTS
        [TestMethod]
        public async Task DesignGroupInTheEndTest3()
        {
            await DeleteDocs();
            await InitEndDocuments();

            var result = new ResultSet[3];
            var expected = new[] { new[] { "A0", "A1", "A2", "A3"} };
            int skip = 0, limit = 10;

            await TestExpectedResult(result, skip, limit, expected,nrOfDocsRightSide);
            await DeleteDocs();
        }

        private async Task TestExpectedResult(ResultSet[] result, int skip, int limit, string[][] expected,int nrOfDocs)
        {
            for (int i = 0; i < Math.Round((double)(nrOfDocs) / limit); i++)
            {
                result[i] = await bucketRead.GetAllAsync(skip, limit);
                string[] array = result[i].Objects.Select(o => o.Key).ToArray();
                CollectionAssert.AreEqual(expected[i], array);
                skip += limit;
            }
        }

        private async Task DeleteDocs()
        {
            ResultSet resultSets = await bucketRead.GetAllAsync();

            foreach (var obj in resultSets.Objects)
                {
                    await bucketRead.DeleteAsync(obj);
                }
        }

        private async Task InitMiddleDocuments()
        {
            await InitEndDocuments();
            await InitStartDocuments();
        }

        private async Task InitEndDocuments()
        {
            for (int i = 0; i < nrOfDocsRightSide; i++)
            {
                await bucketRead.StoreAsync("A" + i, new { }, tags[i]);
            }
        }

        private async Task InitStartDocuments()
        {
            for (int i = 0; i < nrOfDocsLeftSide; i++)
            {
                await bucketRead.StoreAsync("a" + i, new { },tags[i]);
            }
        }
    }
}
