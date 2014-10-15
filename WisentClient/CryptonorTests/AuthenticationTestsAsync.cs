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

namespace CryptonorTests
{
    [TestClass]
    public class AuthenticationTestsAsync
    {
        private IBucket bucketReadWrite;
        private IBucket bucketRead;
        private IBucket bucketNone;
        private const string documentKey="2a13e21abc054e046617f8613c0025b2";

        public AuthenticationTestsAsync()
        {
            CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.Camellia128, "mysuper_secret");
            var cl1 = new CryptonorClient.CryptonorClient("http://localhost:53411/api/","b8d2f15848b12927d50d0037510013c8", "v8zQGiAjyl");
            bucketReadWrite = cl1.GetBucket("iasi");
            var cl2 = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "b8d2f15848b12927d50d003751001bf9", "lvcrHysPRw");
            bucketRead= cl2.GetBucket("iasi");
            var cl3 = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "b8d2f15848b12927d50d00375100227a", "o5BPwKMv4u");
            bucketNone= cl3.GetBucket("iasi");
        }

        //
        //TEST RIGHT:NONE METHODS:ALL 
        //
        [TestMethod]
        public async Task TestNoneGet()
        {
            var unautorized = false;
            try
            {
                await bucketNone.GetAsync(documentKey);
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestNoneGetByTag()
        {
            var unautorized = false;
            try
            {
                await bucketNone.GetAsync(new Query("age"){Value = "22"});
            }
            catch (HttpRequestException ex)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestNoneGetEntity()
        {
            var unautorized = false;
            try
            {
                await bucketNone.GetAsync<Person>(documentKey);
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

       [TestMethod]
        public async Task TestNoneGetAll()
        {
            var unautorized = false;
            try
            {
                await bucketNone.GetAllAsync();
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

       [TestMethod]
       public async Task TestNoneStoreCryptoObject()
       {
           var unautorized = false;
           try
           {
               //store object(POST)
               await bucketNone.StoreAsync(new CryptonorObject());
           }
           catch (HttpRequestException)
           {
               unautorized = true;
           }
           Assert.IsTrue(unautorized);
       }

       [TestMethod]
       public async Task TestNoneStoreObjectWithKeyAndObjTags()
       {
           var unautorized = false;
           try
           {
               //store object(POST)
               await bucketNone.StoreAsync("dummy", new Person(), new { age = "22" });
           }
           catch (HttpRequestException)
           {
               unautorized = true;
           }
           Assert.IsTrue(unautorized);
       }

       [TestMethod]
       public async Task TestNoneStoreObjectWithKey()
       {
           var unautorized = false;
           try
           {
               //store object(POST)
               await bucketNone.StoreAsync("dummy", new Person());
           }
           catch (HttpRequestException)
           {
               unautorized = true;
           }
           Assert.IsTrue(unautorized);
       }

       [TestMethod]
       public async Task TestNoneStoreObjectWithKeyAndTags()
       {
           var unautorized = false;
           try
           {
               //store object(POST)
               await bucketNone.StoreAsync("dummy", new Person(), new Dictionary<string, object> { { "age", "22" } });
           }
           catch (HttpRequestException)
           {
               unautorized = true;
           }
           Assert.IsTrue(unautorized);
       }

       [TestMethod]
       public async Task TestNoneStoreBatch()
       {
           var unautorized = false;
           try
           {
               //store batch(POST)
               await bucketNone.StoreBatchAsync(new []{new CryptonorObject()});
           }
           catch (HttpRequestException)
           {
               unautorized = true;
           }
           Assert.IsTrue(unautorized);
       }

       [TestMethod]
       public async Task TestNoneDeleteByKey()
       {
           var unautorized = false;
           try
           {
               //delete by key(DeleteAsync)
               await bucketNone.DeleteAsync("dummy");
           }
           catch (HttpRequestException)
           {
               unautorized = true;
           }
           Assert.IsTrue(unautorized);
       }

       [TestMethod]
       public async Task TestNoneDeleteCryptoObject()
       {
           var unautorized = false;
           try
           {
               //delete crypto object(DELETE)
               await bucketNone.DeleteAsync(new CryptonorObject());
           }
           catch (HttpRequestException)
           {
               unautorized = true;
           }
           Assert.IsTrue(unautorized);
       }


        //
        //TEST RIGHT:READ METHODS:ALL THAT SHOULD BE AUTHORIZED
        //
        [TestMethod]
        public async Task TestReadAuthorized()
        {
            var unautorized = false;
            try
            {
                //get all(GET)
                await bucketRead.GetAllAsync();

                //get by querry(POST)
                await bucketRead.GetAsync(new Query("age"){Value = "22"});

                //get document(GET)
                await bucketRead.GetAsync(documentKey);

                // get entity(GET)
                await bucketRead.GetAsync<Person>(documentKey);
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsFalse(unautorized);
        }
        //
        //TEST RIGHT:READ METHODS:ALL THAT SHOULDN 'T BE AUTHORIZED
        //
        [TestMethod]
        public async Task TestReadStoreCryptoObject()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                await bucketRead.StoreAsync(new CryptonorObject());
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestReadStoreObjectWithKeyAndObjTags()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                await bucketRead.StoreAsync("dummy", new Person(), new { age = "22" });
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestReadStoreObjectWithKey()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                await bucketRead.StoreAsync("dummy", new Person());
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestReadStoreObjectWithKeyAndTags()
        {
            var unautorized = false;
            try
            {
                //store object(POST)
                await bucketRead.StoreAsync("dummy", new Person(), new Dictionary<string, object> { { "age", "22" } });
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestReadStoreBatch()
        {
            var unautorized = false;
            try
            {
                //store batch(POST)
                await bucketRead.StoreBatchAsync(new[] { new CryptonorObject() });
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestReadDeleteByKey()
        {
            var unautorized = false;
            try
            {
                //delete by key(DeleteAsync)
                await bucketRead.DeleteAsync("dummy");
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        [TestMethod]
        public async Task TestReadDeleteCryptoObject()
        {
            var unautorized = false;
            try
            {
                //delete crypto object(DELETE)
                await bucketRead.DeleteAsync(new CryptonorObject());
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsTrue(unautorized);
        }

        //
        //TEST RIGHT:READWRITE METHODS:ALL
        //
        [TestMethod]
        public async Task TestReadWrite()
        {
            var person = new Person
            {
                UserName = "irinel",
                BirthDate = new DateTime(1992,4,30)
            };
            var cryptoObj = new CryptonorObject{Key = "key0"};
            cryptoObj.SetValue(person);
            var unautorized = false;
            try
            {
                //get all(GET)
                await bucketReadWrite.GetAllAsync();

                //get by querry(POST)
                await bucketReadWrite.GetAsync(new Query("age") { Value = "22" });

                //get document(GET)
                await bucketReadWrite.GetAsync(documentKey);

                // get entity(GET)
                await bucketReadWrite.GetAsync<Person>(documentKey);

                //store object(POST)
                await bucketReadWrite.StoreAsync(cryptoObj);

                //store object(POST)
                await bucketReadWrite.StoreAsync("key1",person,new {age="22"});

                //store object(POST)
                await bucketReadWrite.StoreAsync("key2",person);

                //store object(POST)
                await bucketReadWrite.StoreAsync("key3", person,new Dictionary<string, object>{{"age","22"}});

                //store batch(POST)
                await bucketReadWrite.StoreBatchAsync(new []{new CryptonorObject{Key = "key4"},new CryptonorObject{ Key = "key5"}});

                //delete by key(DELTE)
                await bucketReadWrite.DeleteAsync("key1");

                //delete other documents that were stored in this test
                await bucketReadWrite.DeleteAsync("key2");
                await bucketReadWrite.DeleteAsync("key3");
                await bucketReadWrite.DeleteAsync("key4");
                await bucketReadWrite.DeleteAsync("key5");

                //delete obj(DELETE)
                await bucketReadWrite.DeleteAsync(await bucketReadWrite.GetAsync("key0"));
            }
            catch (HttpRequestException)
            {
                unautorized = true;
            }
            Assert.IsFalse(unautorized);
        }

        //
        //PERFOEMANCE TEST
        //

        [TestMethod]
        public async Task PerformanceGetTest()
        {
            var start =  DateTime.Now;

            for (int i = 0; i <10; i++)
            {
                await bucketReadWrite.GetAsync(documentKey);
            }

            var elapsed = DateTime.Now - start;
            Console.WriteLine("A document was retrived 1000 times in "+elapsed+" seconds");
        }

        [TestMethod]
        public async Task PerformanceStoreTest()
        {
            var start = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                await bucketReadWrite.StoreAsync(new CryptonorObject{Key = i.ToString()});
            }

            var elapsed = DateTime.Now - start;
            Console.WriteLine("1000 objects were stored in "+elapsed+" seconds");

            start = DateTime.Now;

        
            await bucketReadWrite.GetAllAsync();
           

            elapsed = DateTime.Now - start;
            Console.WriteLine("The objects were retrived in "+elapsed+" seconds");

            start = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                await bucketReadWrite.DeleteAsync( i.ToString() );
            }

            elapsed = DateTime.Now - start;
            Console.WriteLine("The objects were deleted in " + elapsed+" seconds");
        }
    }
}
