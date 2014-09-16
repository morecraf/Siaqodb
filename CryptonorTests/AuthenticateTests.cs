using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Cryptonor;
using Cryptonor.Queries;
using CryptonorClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo;
using System.Net;

namespace CryptonorTests
{
    [TestClass]
    public class AuthenticationTests
    {
        private IBucket bucketReadWrite;
        private IBucket bucketRead;
        private IBucket bucketNone;
        private const string documentKey = "2a13e21abc054e046617f8613c0025b2";

        public AuthenticationTests()
        {
            CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.Camellia128, "mysuper_secret");
            var cl1 = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "excelsior", "b8d2f15848b12927d50d0037510013c8", "v8zQGiAjyl");
            bucketReadWrite = cl1.GetBucket("iasi");
            var cl2 = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "excelsior", "b8d2f15848b12927d50d003751001bf9", "lvcrHysPRw");
            bucketRead = cl2.GetBucket("iasi");
            var cl3 = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "excelsior", "b8d2f15848b12927d50d00375100227a", "o5BPwKMv4u");
            bucketNone = cl3.GetBucket("iasi");
        }

        //
        //TEST RIGHT:NONE METHODS:ALL 
        //
        [TestMethod]
        public void TestNoneGet()
        {
            var unautorized = false;
            try
            {
                  bucketNone.Get(documentKey);
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("Unauthorized"))
                {
                    unautorized = true;
                }
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneGetByTag()
        {
            var unautorized = false;
            try
            {
                  bucketNone.Get(new CryptonorQuery("age") { Value = "22" });
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneGetEntity()
        {
            var unautorized = false;
            try
            {
                  bucketNone.Get<Person>(documentKey);
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneGetAll()
        {
            var unautorized = false;
            try
            {
                  bucketNone.GetAll();
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneStoreCryptoObject()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketNone.Store(new CryptonorObject());
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneStoreObjectWithKeyAndObjTags()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketNone.Store("dummy", new Person(), new { age = "22" });
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneStoreObjectWithKey()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketNone.Store("dummy", new Person());
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneStoreObjectWithKeyAndTags()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketNone.Store("dummy", new Person(), new Dictionary<string, object> { { "age", "22" } });
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneStoreBatch()
        {
            var unautorized = false;
            try
            {
                //store batch(POST)
                  bucketNone.StoreBatch(new[] { new CryptonorObject() });
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneDeleteByKey()
        {
            var unautorized = false;
            try
            {
                //delete by key(Delete)
                  bucketNone.Delete("dummy");
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestNoneDeleteCryptoObject()
        {
            var unautorized = false;
            try
            {
                //delete crypto object(DELETE)
                  bucketNone.Delete(new CryptonorObject());
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }


        //
        //TEST RIGHT:READ METHODS:ALL THAT SHOULD BE AUTHORIZED
        //
        [TestMethod]
        public void TestReadAuthorized()
        {
            var unautorized = false;
            try
            {
                //get all(GET)
                  bucketRead.GetAll();

                //get by querry(POST)
                  bucketRead.Get(new CryptonorQuery("age") { Value = "22" });

                //get document(GET)
                  bucketRead.Get(documentKey);

                // get entity(GET)
                  bucketRead.Get<Person>(documentKey);
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsFalse(unautorized);
        }
        //
        //TEST RIGHT:READ METHODS:ALL THAT SHOULDN 'T BE AUTHORIZED
        //
        [TestMethod]
        public void TestReadStoreCryptoObject()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketRead.Store(new CryptonorObject());
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestReadStoreObjectWithKeyAndObjTags()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketRead.Store("dummy", new Person(), new { age = "22" });
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestReadStoreObjectWithKey()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketRead.Store("dummy", new Person());
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestReadStoreObjectWithKeyAndTags()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                  bucketRead.Store("dummy", new Person(), new Dictionary<string, object> { { "age", "22" } });
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestReadStoreBatch()
        {
            var unautorized = false;
            try
            {
                //store batch(POST)
                  bucketRead.StoreBatch(new[] { new CryptonorObject() });
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestReadDeleteByKey()
        {
            var unautorized = false;
            try
            {
                //delete by key(Delete)
                  bucketRead.Delete("dummy");
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public void TestReadDeleteCryptoObject()
        {
            var unautorized = false;
            try
            {
                //delete crypto object(DELETE)
                  bucketRead.Delete(new CryptonorObject());
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        //
        //TEST RIGHT:READWRITE METHODS:ALL
        //
        [TestMethod]
        public void TestReadWrite()
        {
            var person = new Person
            {
                UserName = "irinel",
                BirthDate = new DateTime(1992, 4, 30)
            };
            var cryptoObj = new CryptonorObject { Key = "key0" };
            cryptoObj.SetValue(person);
            var unautorized = false;
            try
            {
                //get all(GET)
                  bucketReadWrite.GetAll();

                //get by querry(POST)
                  bucketReadWrite.Get(new CryptonorQuery("age") { Value = "22" });

                //get document(GET)
                  bucketReadWrite.Get(documentKey);

                // get entity(GET)
                  bucketReadWrite.Get<Person>(documentKey);

                //store object(POST)
                  bucketReadWrite.Store(cryptoObj);

                //store object(POST)
                  bucketReadWrite.Store("key1", person, new { age = "22" });

                //store object(POST)
                  bucketReadWrite.Store("key2", person);

                //store object(POST)
                  bucketReadWrite.Store("key3", person, new Dictionary<string, object> { { "age", "22" } });

                //store batch(POST)
                  bucketReadWrite.StoreBatch(new[] { new CryptonorObject { Key = "key4" }, new CryptonorObject { Key = "key5" } });

                //delete by key(DELTE)
                  bucketReadWrite.Delete("key1");

                //delete other documents that were stored in this test
                  bucketReadWrite.Delete("key2");
                  bucketReadWrite.Delete("key3");
                  bucketReadWrite.Delete("key4");
                  bucketReadWrite.Delete("key5");

                //delete obj(DELETE)
                  bucketReadWrite.Delete(  bucketReadWrite.Get("key0"));
            }
            catch (WebException ex)
            {
                unautorized = true;
            }
            Assert.IsFalse(unautorized);
        }

        //
        //PERFOEMANCE TEST
        //

        [TestMethod]
        public void PerformanceGetTest()
        {
            var start = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                  bucketReadWrite.Get(documentKey);
            }

            var elapsed = DateTime.Now - start;
            Console.WriteLine("A document was retrived 1000 times in " + elapsed + " seconds");
        }

        [TestMethod]
        public void PerformanceStoreTest()
        {
            var start = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                  bucketReadWrite.Store(new CryptonorObject { Key = i.ToString() });
            }

            var elapsed = DateTime.Now - start;
            Console.WriteLine("1000 objects were stored in " + elapsed + " seconds");

            start = DateTime.Now;


              bucketReadWrite.GetAll();


            elapsed = DateTime.Now - start;
            Console.WriteLine("The objects were retrived in " + elapsed + " seconds");

            start = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                  bucketReadWrite.Delete(i.ToString());
            }

            elapsed = DateTime.Now - start;
            Console.WriteLine("The objects were deleted in " + elapsed + " seconds");
        }
    }
}
