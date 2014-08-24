using Cryptonor.Queries;
using CryptonorClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorTests
{
      [TestClass]
    public class SyncTests
    {
          Random rnd = new Random();
          CryptonorClient.CryptonorClient client;
          string localPath = @"c:\work\temp\cryptonor_sync\";
          string localPath2 = @"c:\work\temp\cryptonor_sync2\";
          public SyncTests()
          {
              CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.Camellia128, "mysuper_secret");
              client = new CryptonorClient.CryptonorClient("http://localhost:53411/", "excelsior", "mykey", "mypwd");
            
          }
          [TestMethod]
          public async Task Insert()
          {
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.Store(p.UserName, p, new { Age = 22 });
              await bucket.Push();
              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);
              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));

              await bucket2.Pull(query);
              var fromDB = await bucket2.Get(userName);
              Assert.AreEqual(22, fromDB.GetTag<int>("Age"));
              var value = fromDB.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(p.FirstName, value.FirstName);
          }
          [TestMethod]
          public async Task Update()
          {
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.Store(p.UserName, p, new { Age = 22 });
              await bucket.Push();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.Pull(query);

            
              var fromDB1 = await bucket.Get(userName);
              var value = fromDB1.GetValue<Person>();
              value.FirstName ="Alisia";
              value.Age = 44;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 44);
              await bucket.Store(fromDB1);

              await bucket.Pull(query);//also calls Push()

              fromDB1 = await bucket.Get(userName);
              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);
              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);
            
              await bucket2.Pull(query);

              fromDB1 = await bucket2.Get(userName);
              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);
          }
          [TestMethod]
          public async Task Delete()
          {
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.Store(p.UserName, p, new { Age = 22 });
              await bucket.Push();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.Pull(query);

              var fromDB1 = await bucket.Get(userName);
              
              await bucket.Delete(fromDB1);

              await bucket.Pull(query);//also calls Push()

              fromDB1 = await bucket.Get(userName);
              Assert.IsNull(fromDB1);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.Pull(query);

              fromDB1 = await bucket2.Get(userName);
              Assert.IsNull(fromDB1);

          }
          [TestMethod]
          public async Task TestConflictUpdateUpdate()
          {//update on C1 and C2 at same time->should be conflict
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.Store(p.UserName, p, new { Age = 22 });
              await bucket.Push();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.Pull(query);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.Pull(query);
              //now both databases has same object with same version

              var fromDB1 = await bucket.Get(userName);
              var value = fromDB1.GetValue<Person>();
              value.FirstName = "Alisia";
              value.Age = 44;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 44);
              await bucket.Store(fromDB1);

              fromDB1 = await bucket2.Get(userName);
              value = fromDB1.GetValue<Person>();
              value.FirstName = "Alisia22";
              value.Age = 6;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 6);
              await bucket2.Store(fromDB1);

              await bucket.Pull(query);//also calls Push()
              await bucket2.Pull(query);//also calls Push()

              fromDB1 = await bucket.Get(userName);
              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);

              fromDB1 = await bucket2.Get(userName);//get from bucket2->it must be conflicted

              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);
          }
          [TestMethod]
          public async Task TestConflictDeleteUpdate()
          {//delete on C1 and update on C2->conflict
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.Store(p.UserName, p, new { Age = 22 });
              await bucket.Push();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.Pull(query);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.Pull(query);
              //now both databases has same object with same version

              var fromDB1 = await bucket.Get(userName);

              await bucket.Delete(fromDB1);
              await bucket.Pull(query);//also calls Push()

              fromDB1 = await bucket.Get(userName);
              Assert.IsNull(fromDB1);

              fromDB1 = await bucket2.Get(userName);
              var value = fromDB1.GetValue<Person>();
              value.FirstName = "Alisia22";
              value.Age = 6;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 6);
              await bucket2.Store(fromDB1);

             
              await bucket2.Pull(query);//also calls Push()

              fromDB1 = await bucket2.Get(userName);
              Assert.IsNull(fromDB1);

          }
          public string GetRandomString(int length)
          {
              const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
              StringBuilder res = new StringBuilder();
              Random rnd = new Random();
              while (0 < length--)
              {
                  res.Append(valid[rnd.Next(valid.Length)]);
              }
              return res.ToString();
          }
          public Person GetPerson(string userName)
          {
              Person p = new Person() { UserName = userName };
              p.Email = userName + "@gmail.com";
              p.Age = 22;
              p.BirthDate = new DateTime(1981, 1, 4);
              p.FirstName = "Cristi";
              p.LastName = "Ursachi";
              return p;
          }
    }
}
