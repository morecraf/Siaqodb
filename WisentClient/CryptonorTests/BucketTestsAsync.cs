using Cryptonor;
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
    public abstract class BucketTestsAsync
    {
        public abstract IBucket GetBucket();
        Random rnd = new Random();
        public BucketTestsAsync()
        {
            CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.Camellia128, "mysuper_secret");
        }
        [TestMethod]
        public async Task Insert()
        {
            int rndNr=rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName); 
            IBucket bucket = this.GetBucket();
            await bucket.StoreAsync(p.UserName, p, new { Age = 33 });
            var fromDB=await bucket.GetAsync(userName);
            Assert.AreEqual(33, fromDB.GetTag<int>("Age"));
            var value = fromDB.GetValue<Person>();
            Assert.AreEqual(userName, value.UserName);
        }
        [TestMethod]
        public async Task InsertByTags()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
            Dictionary<string,object> tags=new Dictionary<string,object>();
            tags.Add("birth_year",1981);

            await bucket.StoreAsync(p.UserName, p, tags);

            var fromDB = await bucket.GetAsync(userName);
            Assert.AreEqual(1981, fromDB.GetTag<int>("birth_year"));
            var value = fromDB.GetValue<Person>();
            Assert.AreEqual(userName, value.UserName);
        }
        [TestMethod]
        public async Task InsertOnlyObj()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
         
            await bucket.StoreAsync(p.UserName, p);

            var value = await bucket.GetAsync<Person>(userName);
            Assert.AreEqual(userName, value.UserName);
        }
        [TestMethod]
        public async Task InsertCryptonorObject()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
            Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
            obj.SetValue<Person>(p);
            obj.SetTag("birth_year", 1981);
            obj.Key = userName;

            await bucket.StoreAsync(obj);

            var objFromDB= await bucket.GetAsync(userName);
            Assert.AreEqual(1981, objFromDB.GetTag<int>("birth_year"));
            Assert.AreEqual(userName, objFromDB.GetValue<Person>().UserName);
        }
        [TestMethod]
        public async Task InsertBatch()
        {
             IBucket bucket = this.GetBucket();
             List<CryptonorObject> list = new List<CryptonorObject>();
            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("birth_year", 1980);
                obj.Key = userName;

                list.Add(obj);

                
            }
            await bucket.StoreBatchAsync(list);
            foreach (CryptonorObject co in list)
            {
                var coDB = await bucket.GetAsync(co.Key);
                Person p=co.GetValue<Person>();
                Person pDB=coDB.GetValue<Person>();
                Assert.AreEqual(p.UserName, pDB.UserName);
                Assert.AreEqual(p.Age, pDB.Age);
                Assert.AreEqual(p.BirthDate, pDB.BirthDate);
                Assert.AreEqual(p.Email, pDB.Email);
                Assert.AreEqual(p.FirstName, pDB.FirstName);
                Assert.AreEqual(p.LastName, pDB.LastName);
                

                Assert.AreEqual(1980, coDB.GetTag<int>("birth_year"));
            }
        }
        [TestMethod]
        public async Task TagsAllTypes()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
            Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
            obj.SetValue<Person>(p);
            obj.SetTag("myint", 34);
            obj.SetTag("mydouble", 34.45);
            obj.SetTag("mydatetime", new DateTime(2014,8,8));
            obj.SetTag("mystring", "varsta");
            obj.SetTag("mybool", true);
            obj.Key = userName;

            await bucket.StoreAsync(obj);

            var objFromDB = await bucket.GetAsync(userName);
            
            Assert.AreEqual(34,objFromDB.GetTag<int>("myint"));
            Assert.AreEqual(34.45, objFromDB.GetTag<double>("mydouble"));
            Assert.AreEqual(new DateTime(2014, 8, 8), objFromDB.GetTag<DateTime>("mydatetime"));
            Assert.AreEqual("varsta", objFromDB.GetTag<string>("mystring"));
            Assert.AreEqual(true, objFromDB.GetTag<bool>("mybool"));

        }
        [TestMethod]
        public async Task Update()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
            await bucket.StoreAsync(p.UserName, p, new { Age = 33 });
            
            var fromDB = await bucket.GetAsync(userName);
            var value = fromDB.GetValue<Person>();
            value.Age = 44;
            fromDB.SetValue<Person>(value);
            fromDB.SetTag("Age", 44);
            await bucket.StoreAsync(fromDB);

            fromDB = await bucket.GetAsync(userName);
            Assert.AreEqual(44, fromDB.GetTag<int>("Age"));
            value = fromDB.GetValue<Person>();
            Assert.AreEqual(userName, value.UserName);
            Assert.AreEqual(44,value.Age);
        }
        [TestMethod]
        public async Task UpdateWithConflict()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
            await bucket.StoreAsync(p.UserName, p, new { Age = 33 });

            var fromDB = await bucket.GetAsync(userName);
            var value = fromDB.GetValue<Person>();
            value.Age = 44;
            bool conflict = false;
            try
            {
                await bucket.StoreAsync(value.UserName, value, new { Age = 44 });
                //treat conflict here because Version is not set
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("conflict"))
                    conflict = true;
            }

            Assert.IsTrue(conflict);
            fromDB = await bucket.GetAsync(userName);
            Assert.AreEqual(33, fromDB.GetTag<int>("Age"));
            value = fromDB.GetValue<Person>();
            Assert.AreEqual(userName, value.UserName);
            Assert.AreEqual(33, value.Age);
        }
        [TestMethod]
        public async Task DeleteAsync()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
            await bucket.StoreAsync(p.UserName, p, new { Age = 33 });

            var fromDB = await bucket.GetAsync(userName);
            await bucket.DeleteAsync(fromDB);

            fromDB = await bucket.GetAsync(userName);
            Assert.IsNull(fromDB);
        }
        [TestMethod]
        public async Task DeleteByKey()
        {
            int rndNr = rnd.Next(100000);
            string userName = "userName" + rndNr;
            Person p = GetPerson(userName);
            IBucket bucket = this.GetBucket();
            await bucket.StoreAsync(p.UserName, p, new { Age = 33 });

            var fromDB = await bucket.GetAsync(userName);
            await bucket.DeleteAsync(fromDB.Key);

            fromDB = await bucket.GetAsync(userName);
            Assert.IsNull(fromDB);
        }
        [TestMethod]
        public async Task SearchByTags()
        {
             IBucket bucket = this.GetBucket();
             List<CryptonorObject> list = new List<CryptonorObject>();
             int myint = rnd.Next(100000);
             double mydouble = myint + 0.34;
             string mystr = myint + "str";
             bool mybool = true;
             DateTime mydate = new DateTime(2014, (myint % 11)+1, (myint % 27) + 1);
             for (int i = 0; i < 3; i++)
             {
                 int rndNr = rnd.Next(100000);
                 string userName = "userName" + rndNr;
                 Person p = GetPerson(userName);
                 Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                 obj.SetValue<Person>(p);
                 obj.SetTag("myint", myint);
                 obj.SetTag("mydouble", mydouble);
                 obj.SetTag("mydatetime", mydate);
                 obj.SetTag("mystring", mystr);
                 obj.SetTag("mybool", mybool);
                 obj.Key = userName;
                 list.Add(obj);
             }
             await bucket.StoreBatchAsync(list);

             Query query1 = new Query("myint");
             query1.Setup(a => a.Value(myint));
             var result = await bucket.GetAsync(query1);
             Assert.AreEqual(3, result.Objects.Count);
             foreach (CryptonorObject co in list)
             {
                 CryptonorObject objFromDB=result.Objects.FirstOrDefault(a => a.Key == co.Key);
                 Assert.IsNotNull(objFromDB);
             }
             Query query2 = new Query("mydouble");
             query2.Setup(a => a.Value(mydouble));
             var result2 = await bucket.GetAsync(query2);
             Assert.AreEqual(3, result2.Objects.Count);
             foreach (CryptonorObject co in list)
             {
                 CryptonorObject objFromDB = result2.Objects.FirstOrDefault(a => a.Key == co.Key);
                 Assert.IsNotNull(objFromDB);
             }
             Query query3 = new Query("mydatetime");
             query3.Setup(a => a.Value(mydate));
             var result3 = await bucket.GetAsync(query3);
             Assert.AreEqual(3, result3.Objects.Count);
             foreach (CryptonorObject co in list)
             {
                 CryptonorObject objFromDB = result3.Objects.FirstOrDefault(a => a.Key == co.Key);
                 Assert.IsNotNull(objFromDB);
             }
             Query query4 = new Query("mystring");
             query4.Setup(a => a.Value(mystr));
             var result4 = await bucket.GetAsync(query4);
             Assert.AreEqual(3, result4.Objects.Count);
             foreach (CryptonorObject co in list)
             {
                 CryptonorObject objFromDB = result4.Objects.FirstOrDefault(a => a.Key == co.Key);
                 Assert.IsNotNull(objFromDB);
             }
             Query query5 = new Query("mybool");
             query5.Setup(a => a.Value(mybool));
             var result5 = await bucket.GetAsync(query5);
             foreach (CryptonorObject co in list)
             {
                 CryptonorObject objFromDB = result5.Objects.FirstOrDefault(a => a.Key == co.Key);
                 Assert.IsNotNull(objFromDB);
             }

        }
        [TestMethod]
        public async Task SearchByTagsStartInt()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint = rnd.Next(100000);
           
            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("myint", myint+i);
               
                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("myint");
            query1.Setup(a => a.Start(myint));
            var result = await bucket.GetAsync(query1);
            int j=0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(myint+j,objFromDB.GetTag<int>("myint"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("myint");
            query2.Setup(a => a.Start(myint+2).Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(myint+2 - j, objFromDB.GetTag<int>("myint"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsStartString()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();

            string s = GetRandomString(10);

            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mystr", s+i.ToString());

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mystr");
            query1.Setup(a => a.Start(s+"0"));
            var result = await bucket.GetAsync(query1);
            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(s + j, objFromDB.GetTag<string>("mystr"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mystr");
            query2.Setup(a => a.Start(s + "2").Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(s + (2 - j).ToString(), objFromDB.GetTag<string>("mystr"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsStartDouble()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint22 = rnd.Next(100000);
            double mydouble = myint22 + 0.23;
            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mydouble", mydouble + i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mydouble");
            query1.Setup(a => a.Start(mydouble));
            var result = await bucket.GetAsync(query1);
            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(mydouble + j, objFromDB.GetTag<double>("mydouble"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mydouble");
            query2.Setup(a => a.Start(mydouble + 2).Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(mydouble + 2 - j, objFromDB.GetTag<double>("mydouble"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsStartDateTime()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint22 = rnd.Next(1900,2200);
            DateTime mydate = new DateTime(myint22, (myint22 % 11) + 1, (myint22 % 23) + 1);
          
            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mydate", mydate.AddDays(i));

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mydate");
            query1.Setup(a => a.Start(mydate));
            var result = await bucket.GetAsync(query1);
            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(mydate.AddDays( j), objFromDB.GetTag<DateTime>("mydate"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mydate");
            query2.Setup(a => a.Start(mydate.AddDays(2)).Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(mydate.AddDays(2 - j), objFromDB.GetTag<DateTime>("mydate"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsEndInt()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint = rnd.Next(100000);

            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("myint", myint + i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("myint");
            query1.Setup(a => a.End(myint+2));
            var result = await bucket.GetAsync(query1);
            int j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[result.Objects.Count-1-j];
                Assert.AreEqual(myint + 2 - j, objFromDB.GetTag<int>("myint"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("myint");
            query2.Setup(a => a.End(myint).Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[result2.Objects.Count - 1 - j];
                Assert.AreEqual(myint +j, objFromDB.GetTag<int>("myint"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
        }
        [TestMethod]
        public async Task SearchByTagsEndString()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            string s = GetRandomString(10);

            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mystr", s + i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mystr");
            query1.Setup(a => a.End(s + 2));
            var result = await bucket.GetAsync(query1);
            int j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[result.Objects.Count - 1 - j];
                Assert.AreEqual(s + (2 - j), objFromDB.GetTag<string>("mystr"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mystr");
            query2.Setup(a => a.End(s).Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[result2.Objects.Count - 1 - j];
                Assert.AreEqual(s + j, objFromDB.GetTag<string>("mystr"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
        }
        [TestMethod]
        public async Task SearchByTagsEndDateTime()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint22 = rnd.Next(1900, 2200);
            DateTime mydate = new DateTime(myint22, (myint22 % 11) + 1, (myint22 % 23) + 1);
          
            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mydate", mydate.AddDays(i));

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mydate");
            query1.Setup(a =>a.End(  mydate.AddDays( 2)));
            var result = await bucket.GetAsync(query1);
            int j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[result.Objects.Count - 1 - j];
                Assert.AreEqual(mydate.AddDays(2 - j), objFromDB.GetTag<DateTime>("mydate"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mydate");
            query2.Setup(a => a.End(mydate).Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[result2.Objects.Count - 1 - j];
                Assert.AreEqual(mydate.AddDays( j), objFromDB.GetTag<DateTime>("mydate"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
        }
        [TestMethod]
        public async Task SearchByTagsEndDouble()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint22 = rnd.Next(100000);
            double mydouble = myint22 + 0.23;

            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mydouble", mydouble+i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mydouble");
            query1.Setup(a => a.End(mydouble+2));
            var result = await bucket.GetAsync(query1);
            int j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[result.Objects.Count - 1 - j];
                Assert.AreEqual(mydouble + (2 - j), objFromDB.GetTag<double>("mydouble"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mydouble");
            query2.Setup(a => a.End(mydouble).Descending());
            var result2 = await bucket.GetAsync(query2);
            j = 0;
            list.Reverse();
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[result2.Objects.Count - 1 - j];
                Assert.AreEqual(mydouble + j, objFromDB.GetTag<double>("mydouble"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
        }
        [TestMethod]
        public async Task SearchByTagsStartEndInt()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint = rnd.Next(100000);

            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("myint", myint + i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("myint");
            query1.Setup(a => a.Start(myint).End(myint+2));
            var result = await bucket.GetAsync(query1);
            Assert.AreEqual(result.Objects.Count, list.Count);

            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(myint + j, objFromDB.GetTag<int>("myint"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("myint");
            query2.Setup(a => a.Start(myint + 2).End(myint).Descending());
            var result2 = await bucket.GetAsync(query2);
            Assert.AreEqual(result2.Objects.Count, list.Count);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(myint + 2 - j, objFromDB.GetTag<int>("myint"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsStartEndString()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            string s = GetRandomString(10);

            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mystr", s + i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mystr");
            query1.Setup(a => a.Start(s).End(s + 2));
            var result = await bucket.GetAsync(query1);
            Assert.AreEqual(result.Objects.Count, list.Count);

            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(s + j, objFromDB.GetTag<string>("mystr"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mystr");
            query2.Setup(a => a.Start(s + 2).End(s).Descending());
            var result2 = await bucket.GetAsync(query2);
            Assert.AreEqual(result2.Objects.Count, list.Count);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(s + (2 - j), objFromDB.GetTag<string>("mystr"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsStartEndDateTime()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint22 = rnd.Next(1900, 2200);
            DateTime mydate = new DateTime(myint22, (myint22 % 11) + 1, (myint22 % 23) + 1);
          
            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mydate",mydate.AddDays( i));

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mydate");
            query1.Setup(a => a.Start(mydate).End(mydate.AddDays( 2)));
            var result = await bucket.GetAsync(query1);
            Assert.AreEqual(result.Objects.Count, list.Count);

            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(mydate.AddDays( j), objFromDB.GetTag<DateTime>("mydate"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mydate");
            query2.Setup(a => a.Start(mydate.AddDays(2)).End(mydate).Descending());
            var result2 = await bucket.GetAsync(query2);
            Assert.AreEqual(result2.Objects.Count, list.Count);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(mydate.AddDays(2 - j), objFromDB.GetTag<DateTime>("mydate"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsStartEndDouble()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint22 = rnd.Next(100000);
            double mydouble = myint22 + 0.23;
            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("mydouble", mydouble + i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("mydouble");
            query1.Setup(a => a.Start(mydouble).End(mydouble + 2));
            var result = await bucket.GetAsync(query1);
            Assert.AreEqual(result.Objects.Count, list.Count);

            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(mydouble + j, objFromDB.GetTag<double>("mydouble"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("mydouble");
            query2.Setup(a => a.Start(mydouble + 2).End(mydouble).Descending());
            var result2 = await bucket.GetAsync(query2);
            Assert.AreEqual(result2.Objects.Count, list.Count);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(mydouble + 2 - j, objFromDB.GetTag<double>("mydouble"));
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
        }
        [TestMethod]
        public async Task SearchByTagsSkipTake()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            int myint = rnd.Next(100000);

            for (int i = 0; i < 3; i++)
            {
                int rndNr = rnd.Next(100000);
                string userName = "userName" + rndNr;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
                obj.SetTag("myint", myint + i);

                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("myint");
            query1.Setup(a => a.Start(myint).End(myint + 2).Skip(1).Take(1));
            var result = await bucket.GetAsync(query1);
            Assert.AreEqual(1, result.Objects.Count);
            CryptonorObject objFromDB = result.Objects[0];
            Assert.AreEqual(myint + 1, objFromDB.GetTag<int>("myint"));
            Assert.AreEqual(list[1].Key, objFromDB.Key);
           
            Query query2 = new Query("myint");
            query2.Setup(a => a.Start(myint + 2).End(myint).Skip(2).Take(1).Descending());
            var result2 = await bucket.GetAsync(query2);
            Assert.AreEqual(1,result2.Objects.Count);
            CryptonorObject objFromDB2 = result2.Objects[0];
            Assert.AreEqual(myint  , objFromDB2.GetTag<int>("myint"));
            Assert.AreEqual(list[0].Key, objFromDB2.Key);

          
        }
        [TestMethod]
        public async Task SearchByKeyQuery()
        {
            IBucket bucket = this.GetBucket();
            List<CryptonorObject> list = new List<CryptonorObject>();
            string s = GetRandomString(10);

            for (int i = 0; i < 3; i++)
            {
                string userName = "userName" + s+i;
                Person p = GetPerson(userName);
                Cryptonor.CryptonorObject obj = new Cryptonor.CryptonorObject();
                obj.SetValue<Person>(p);
             
                obj.Key = userName;
                list.Add(obj);
            }
            await bucket.StoreBatchAsync(list);

            Query query1 = new Query("key");
            query1.Setup(a => a.Start("userName" + s).End("userName" + s + 2));
            var result = await bucket.GetAsync(query1);
            Assert.AreEqual(result.Objects.Count, list.Count);

            int j = 0;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result.Objects[j];
                Assert.AreEqual(co.Key, objFromDB.Key);
                j++;
            }
            Query query2 = new Query("KEy");
            query2.Setup(a => a.Start("userName" + s + 2).End("userName" + s).Descending());
            var result2 = await bucket.GetAsync(query2);
            Assert.AreEqual(result2.Objects.Count, list.Count);
            j = 2;
            foreach (CryptonorObject co in list)
            {
                CryptonorObject objFromDB = result2.Objects[j];
                Assert.AreEqual(co.Key, objFromDB.Key);
                j--;
            }
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
            p.Age = 33;
            p.BirthDate = new DateTime(1981, 1, 4);
            p.FirstName = "Cristi";
            p.LastName = "Ursachi";
            return p;
        }
    }
}
