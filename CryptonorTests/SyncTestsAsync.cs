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
    public class SyncTestsAsync
    {
          Random rnd = new Random();
          CryptonorClient.CryptonorClient client;
          string localPath = @"c:\work\temp\cryptonor_sync\";
          string localPath2 = @"c:\work\temp\cryptonor_sync2\";
          public SyncTestsAsync()
          {
              CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.Camellia128, "mysuper_secret");
              client = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "excelsior", "9bbaae526db72073e5f23963d1003d35", "O39BZwD2cD");
            
          }
          [TestMethod]
          public async Task Insert()
          {
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.StoreAsync(p.UserName, p, new { Age = 22 });
              await bucket.PushAsync();
              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);
              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));

              await bucket2.PullAsync(query);
              var fromDB = await bucket2.GetAsync(userName);
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
              await bucket.StoreAsync(p.UserName, p, new { Age = 22 });
              await bucket.PushAsync();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.PullAsync(query);

            
              var fromDB1 = await bucket.GetAsync(userName);
              var value = fromDB1.GetValue<Person>();
              value.FirstName ="Alisia";
              value.Age = 44;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 44);
              await bucket.StoreAsync(fromDB1);

              await bucket.PullAsync(query);//also calls PushAsync()

              fromDB1 = await bucket.GetAsync(userName);
              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);
              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);
            
              await bucket2.PullAsync(query);

              fromDB1 = await bucket2.GetAsync(userName);
              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);
          }
          [TestMethod]
          public async Task DeleteAsync()
          {
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.StoreAsync(p.UserName, p, new { Age = 22 });
              await bucket.PushAsync();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.PullAsync(query);

              var fromDB1 = await bucket.GetAsync(userName);
              
              await bucket.DeleteAsync(fromDB1);

              await bucket.PullAsync(query);//also calls PushAsync()

              fromDB1 = await bucket.GetAsync(userName);
              Assert.IsNull(fromDB1);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.PullAsync(query);

              fromDB1 = await bucket2.GetAsync(userName);
              Assert.IsNull(fromDB1);

          }
          [TestMethod]
          public async Task TestConflictUpdateUpdate()
          {//update on C1 and C2 at same time->should be conflict
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.StoreAsync(p.UserName, p, new { Age = 22 });
              await bucket.PushAsync();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.PullAsync(query);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.PullAsync(query);
              //now both databases has same object with same version

              var fromDB1 = await bucket.GetAsync(userName);
              var value = fromDB1.GetValue<Person>();
              value.FirstName = "Alisia";
              value.Age = 44;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 44);
              await bucket.StoreAsync(fromDB1);

              fromDB1 = await bucket2.GetAsync(userName);
              value = fromDB1.GetValue<Person>();
              value.FirstName = "Alisia22";
              value.Age = 6;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 6);
              await bucket2.StoreAsync(fromDB1);

              await bucket.PullAsync(query);//also calls PushAsync()

              bucket2.PushCompleted += bucket2_PushCompletedUpdateUpdate;
              await bucket2.PullAsync(query);//also calls PushAsync()
              
              fromDB1 = await bucket.GetAsync(userName);
              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);

              fromDB1 = await bucket2.GetAsync(userName);//get from bucket2->it must be conflicted

              Assert.AreEqual(44, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(44, value.Age);
              Assert.AreEqual("Alisia", value.FirstName);
          }

          void bucket2_PushCompletedUpdateUpdate(object sender, PushCompletedEventArgs e)
          {
              Assert.IsTrue(e.Conflicts.Count == 1);
          }
          [TestMethod]
          public async Task TestConflictDeleteUpdate()
          {//delete on C1 and update on C2->conflict
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.StoreAsync(p.UserName, p, new { Age = 22 });
              await bucket.PushAsync();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.PullAsync(query);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.PullAsync(query);
              //now both databases has same object with same version

              var fromDB1 = await bucket.GetAsync(userName);

              await bucket.DeleteAsync(fromDB1);
              await bucket.PullAsync(query);//also calls PushAsync()

              fromDB1 = await bucket.GetAsync(userName);
              Assert.IsNull(fromDB1);

              fromDB1 = await bucket2.GetAsync(userName);
              var value = fromDB1.GetValue<Person>();
              value.FirstName = "Alisia22";
              value.Age = 6;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 6);
              await bucket2.StoreAsync(fromDB1);

              bucket2.PushCompleted += bucket2_PushCompletedDeleteUpdate;
              await bucket2.PullAsync(query);//also calls PushAsync()

              fromDB1 = await bucket2.GetAsync(userName);
              Assert.IsNull(fromDB1);

          }

          void bucket2_PushCompletedDeleteUpdate(object sender, PushCompletedEventArgs e)
          {
              Assert.IsTrue(e.Conflicts.Count == 1);
          }
          [TestMethod]
          public async Task TestConflictUpdateDelete()
          {
              //update on C1 and delete on C2->conflict

              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.StoreAsync(p.UserName, p, new { Age = 22 });
              await bucket.PushAsync();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.PullAsync(query);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.PullAsync(query);
              //now both databases has same object with same version

              var fromDB1 = await bucket.GetAsync(userName);
              var value = fromDB1.GetValue<Person>();
              value.FirstName = "Alisia22";
              value.Age = 6;
              fromDB1.SetValue<Person>(value);
              fromDB1.SetTag("Age", 6);
              await bucket.StoreAsync(fromDB1);
              await bucket.PullAsync(query);//also calls PushAsync()

              fromDB1 = await bucket2.GetAsync(userName);
              await bucket2.DeleteAsync(fromDB1);
              bucket2.PushCompleted += bucket2_PushCompletedUpdateDelete;
              await bucket2.PullAsync(query);

              fromDB1 = await bucket2.GetAsync(userName);
              Assert.AreEqual(6, fromDB1.GetTag<int>("Age"));
              value = fromDB1.GetValue<Person>();
              Assert.AreEqual(userName, value.UserName);
              Assert.AreEqual(6, value.Age);
              Assert.AreEqual("Alisia22", value.FirstName);
          }

          void bucket2_PushCompletedUpdateDelete(object sender, PushCompletedEventArgs e)
          {
              Assert.IsTrue(e.Conflicts.Count == 1);
          }
          [TestMethod]
          public async Task TestConflictDeleteDelete()
          {
              //delete on C1 and delete on C2->conflict-> nothing happens on clients
              int rndNr = rnd.Next(100000);
              string userName = "userName" + rndNr;
              Person p = GetPerson(userName);
              CryptonorLocalBucket bucket = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath);
              await bucket.StoreAsync(p.UserName, p, new { Age = 22 });
              await bucket.PushAsync();

              CryptonorQuery query = new CryptonorQuery("key");
              query.Setup(a => a.Value<string>(userName));
              await bucket.PullAsync(query);

              CryptonorLocalBucket bucket2 = (CryptonorLocalBucket)client.GetLocalBucket("dbsync", localPath2);

              await bucket2.PullAsync(query);
              //now both databases has same object with same version

              var fromDB1 = await bucket.GetAsync(userName);

              await bucket.DeleteAsync(fromDB1);
              await bucket.PullAsync(query);//also calls PushAsync()

              fromDB1 = await bucket.GetAsync(userName);
              Assert.IsNull(fromDB1);

              fromDB1 = await bucket2.GetAsync(userName);
              await bucket2.DeleteAsync(fromDB1);
              bucket2.PushCompleted += bucket2_PushCompletedDeleteDelete;
              await bucket2.PullAsync(query);//also calls PushAsync()

              fromDB1 = await bucket2.GetAsync(userName);
              Assert.IsNull(fromDB1);
          }

          void bucket2_PushCompletedDeleteDelete(object sender, PushCompletedEventArgs e)
          {
              Assert.IsTrue(e.Conflicts.Count == 1);
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
