using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo.Documents;
using Sqo;
using Newtonsoft.Json;
using SiaqodbCloud;
using System.Linq;
using Sqo.Transactions;

namespace TestSiaqodbBuckets
{
    [TestClass]
    public  class BucketTests
    {
        Siaqodb siaqodb;
        public IBucket GetBucket()
        {
            return siaqodb.Documents["unittests"];
        }
        public void DropBucket()
        {
            siaqodb.Documents.DropBucket("unittests");
        }
        public static readonly object _syncRoot = new object();
       
        Random rnd = new Random();
        public BucketTests()
        {
            Sqo.SiaqodbConfigurator.SetLicense(@" vxkmLEjihI7X+S2ottoS2Zaj8cKVLxLozBmFerFg6P8OWQqrY4O2s0tk+UnwGI6z");
            Sqo.SiaqodbConfigurator.SetSyncableBucket("contacts", true);
            Sqo.SiaqodbConfigurator.SetDocumentSerializer(new MyJsonSerializer());
            this.siaqodb=new Siaqodb(@"c:\work\temp\buk_tests\");
        }
        [TestMethod]
        public void Ping()
        {
            //CryptonorClient client = new CryptonorClient(Init.Api_URL, Init.Username, Init.Password);
           // string pong = client.Ping();
           // Assert.AreEqual("pong", pong);
        }
        [TestMethod]
        public void Insert()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            bucket.Store(p.UserName, p, new { Age = 33 });
            var fromDB = bucket.Load(username);
            Assert.AreEqual(33, fromDB.GetTag<int>("Age"));
            var value = fromDB.GetContent<Person>();
            Assert.AreEqual(username, value.UserName);


        }
        [TestMethod]
        public void InsertByTags()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                Dictionary<string, object> tags = new Dictionary<string, object>();
                tags.Add("birth_year", 1981);

                bucket.Store(p.UserName, p, tags);

                var fromDB = bucket.Load(username);
                Assert.AreEqual(1981, fromDB.GetTag<int>("birth_year"));
                var value = fromDB.GetContent<Person>();
                Assert.AreEqual(username, value.UserName);
            }
        }
        [TestMethod]
        public void InsertOnlyObj()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                bucket.Store(p.UserName, p);

                var value = bucket.Load<Person>(username);
                Assert.AreEqual(username, value.UserName);
            }
        }
        [TestMethod]
        public void InsertDocument()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                Document obj = new Document();
                obj.SetContent<Person>(p);
                obj.SetTag("birth_year", 1981);
                obj.Key = username;

                bucket.Store(obj);

                var objFromDB = bucket.Load(username);
                Assert.AreEqual(1981, objFromDB.GetTag<int>("birth_year"));
                Assert.AreEqual(username, objFromDB.GetContent<Person>().UserName);
            }
        }
        [TestMethod]
        public void InsertBatch()
        {
            IBucket bucket = this.GetBucket();
            {
                List<Document> list = new List<Document>();
                for (int i = 0; i < 3; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                     Document obj = new  Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("birth_year", 1980);
                    obj.Key = username;

                    list.Add(obj);
                }
                bucket.StoreBatch(list);
                foreach (Document co in list)
                {
                    var coDB = bucket.Load(co.Key);
                    Person p = co.GetContent<Person>();
                    Person pDB = coDB.GetContent<Person>();
                    Assert.AreEqual(p.UserName, pDB.UserName);
                    Assert.AreEqual(p.Age, pDB.Age);
                    Assert.AreEqual(p.BirthDate, pDB.BirthDate);
                    Assert.AreEqual(p.Email, pDB.Email);
                    Assert.AreEqual(p.FirstName, pDB.FirstName);
                    Assert.AreEqual(p.LastName, pDB.LastName);

                    Assert.AreEqual(1980, coDB.GetTag<int>("birth_year"));
                }
            }
        }
        [TestMethod]
        public void TagsAllTypes()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                 Document obj = new  Document();
                obj.SetContent<Person>(p);
                obj.SetTag("myint", 34);
                obj.SetTag("mydouble", 34.45);
                obj.SetTag("mydatetime", new DateTime(2014, 8, 8));
                obj.SetTag("mystring", "varsta");
                obj.SetTag("mybool", true);
                obj.Key = username;

                bucket.Store(obj);

                var objFromDB = bucket.Load(username);

                Assert.AreEqual(34, objFromDB.GetTag<int>("myint"));
                Assert.AreEqual(34.45, objFromDB.GetTag<double>("mydouble"));
                Assert.AreEqual(new DateTime(2014, 8, 8), objFromDB.GetTag<DateTime>("mydatetime"));
                Assert.AreEqual("varsta", objFromDB.GetTag<string>("mystring"));
                Assert.AreEqual(true, objFromDB.GetTag<bool>("mybool"));
            }

        }
        [TestMethod]
        public void Update()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                bucket.Store(p.UserName, p, new { Age = 33 });

                var fromDB = bucket.Load(username);
                var value = fromDB.GetContent<Person>();
                value.Age = 44;
                fromDB.SetContent<Person>(value);
                fromDB.SetTag("Age", 44);
                bucket.Store(fromDB);

                fromDB = bucket.Load(username);
                Assert.AreEqual(44, fromDB.GetTag<int>("Age"));
                value = fromDB.GetContent<Person>();
                Assert.AreEqual(username, value.UserName);
                Assert.AreEqual(44, value.Age);
            }
        }
        [TestMethod]
        public void UpdateWithConflict()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                bucket.Store(p.UserName, p, new { Age = 33 });

                var fromDB = bucket.Load(username);
                var value = fromDB.GetContent<Person>();
                value.Age = 44;
                bool conflict = false;
                try
                {
                    bucket.Store(value.UserName, value, new { Age = 44 });
                    //treat conflict here because Version is not set
                }
                catch (ConflictException ex)
                {

                    conflict = true;
                }

                Assert.IsTrue(conflict);
                fromDB = bucket.Load(username);
                Assert.AreEqual(33, fromDB.GetTag<int>("Age"));
                value = fromDB.GetContent<Person>();
                Assert.AreEqual(username, value.UserName);
                Assert.AreEqual(33, value.Age);
            }
        }

      

        [TestMethod]
        public void DocumentNotFound()
        {
            IBucket bucket = this.GetBucket();
            {

                var fromDB = bucket.Load("somethingthatdosentexist");
                Assert.IsNull(fromDB);


                var cobj = new Document { Key = "nothing" };
                cobj.SetContent(new { nothing = "nothing" });
                cobj.SetTag("nothing", "nothing");
                bucket.Delete(cobj);
                //treat conflict here because Version is not set


            }
        }
        [TestMethod]
        public void Delete()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                bucket.Store(p.UserName, p, new { Age = 33 });

                var fromDB = bucket.Load(username);
                bucket.Delete(fromDB);

                fromDB = bucket.Load(username);
                Assert.IsNull(fromDB);
            }
            rndNr = rnd.Next(100000);
            username = "username" + rndNr;
            Person p2 = GetPerson(username);
            IBucket bucket2 = this.GetBucket();
            {
                bucket2.Store(p2.UserName, p2, new { Age = 33 });
                bucket2.Store(p2.UserName + "a", p2, new { Age = 33 });
                ITransaction trans = siaqodb.BeginTransaction();

                var fromDB = bucket2.Load(username);
                bucket2.Delete(fromDB, trans);

                var fromDB3 = bucket2.Load(username + "a");
                bucket2.Delete(fromDB3, trans);
                trans.Commit();

                fromDB = bucket2.Load(username);
                Assert.IsNull(fromDB);
                fromDB3 = bucket2.Load(username + "a");
                Assert.IsNull(fromDB3);
            }

        }
        [TestMethod]
        public void DeleteByKey()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;
            Person p = GetPerson(username);
            IBucket bucket = this.GetBucket();
            {
                bucket.Store(p.UserName, p, new { Age = 33 });
                
                var fromDB = bucket.Load(username);
                bucket.Delete(fromDB.Key);

                fromDB = bucket.Load(username);
                Assert.IsNull(fromDB);
                
            }
        }
        [TestMethod]
        public void SearchByTags()
        {
            IBucket bucket = this.GetBucket();
            {
                List<Document> list = new List<Document>();
                int myint = rnd.Next(100000);
                double mydouble = myint + 0.34;
                string mystr = myint + "str";
                bool mybool = true;
                DateTime mydate = new DateTime(2014, (myint % 11) + 1, (myint % 27) + 1);
                for (int i = 0; i < 3; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                     Document obj = new  Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("myint", myint);
                    obj.SetTag("mydouble", mydouble);
                    obj.SetTag("mydatetime", mydate);
                    obj.SetTag("mystring", mystr);
                    obj.SetTag("mybool", mybool);
                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereEqual("myint", myint);
                var result = bucket.Find(query1);
                Assert.AreEqual(3, result.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = result.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }
                //LINQ
                var linqQ1 = (from Document doc in this.GetBucket()
                             where doc.GetTag<int>("myint") == myint
                             select doc).ToList();
                Assert.AreEqual(3, linqQ1.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = linqQ1.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }
                Query query2 = new Query();
                query2.WhereEqual("mydouble", mydouble);
                var result2 = bucket.Find(query2);
                Assert.AreEqual(3, result2.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = result2.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }
                //LINQ
                var linqQ2 = (from Document doc in this.GetBucket()
                              where doc.GetTag<double>("mydouble") == mydouble
                              select doc).ToList();
                Assert.AreEqual(3, linqQ2.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = linqQ2.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }

                Query query3 = new Query();
                query3.WhereEqual("mydatetime",mydate);
                var result3 = bucket.Find(query3);
                Assert.AreEqual(3, result3.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = result3.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }
                //LINQ
                var linqQ3 = (from Document doc in this.GetBucket()
                              where doc.GetTag<DateTime>("mydatetime") == mydate
                              select doc).ToList();
                Assert.AreEqual(3, linqQ3.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = linqQ3.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }

                Query query4 = new Query();
                query4.WhereEqual("mystring", mystr);
                var result4 = bucket.Find(query4);
                Assert.AreEqual(3, result4.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = result4.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }
                //LINQ
                var linqQ4 = (from Document doc in this.GetBucket()
                              where doc.GetTag<string>("mystring") == mystr
                              select doc).ToList();
                Assert.AreEqual(3, linqQ4.Count);
                foreach (Document co in list)
                {
                    Document objFromDB = linqQ4.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }

                Query query5 = new Query();
                query5.WhereEqual("mybool", mybool);
                var result5 = bucket.Find(query5);
                foreach (Document co in list)
                {
                    Document objFromDB = result5.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }
                //LINQ
                var linqQ5 = (from Document doc in this.GetBucket()
                              where doc.GetTag<bool>("mybool") == mybool
                              select doc).ToList();
                foreach (Document co in list)
                {
                    Document objFromDB = linqQ5.FirstOrDefault(a => a.Key == co.Key);
                    Assert.IsNotNull(objFromDB);
                }
            }
        }
        [TestMethod]
        public void SearchByWhereGreaterThanOrEqual()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {
                
                List<Document> list = new List<Document>();
                int myint = rnd.Next(100000);
                double mydouble = myint + 0.34;
               
                DateTime mydate = new DateTime(2014, (myint % 11) + 1, (myint % 27) + 1);
                for (int i = -1; i < 3; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                     Document obj = new  Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("myint", myint + i);
                    obj.SetTag("mydouble", mydouble+i);
                    obj.SetTag("mydatetime", mydate.AddDays(i));
                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereGreaterThanOrEqual("myint", myint);
                var result = bucket.Find(query1);
                int j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                    Assert.IsTrue(co.GetTag<int>("myint") >= myint);
                }
                //LINQ
                var linqQ1 = (from Document doc in this.GetBucket()
                              where doc.GetTag<int>("myint") >= myint
                              select doc).ToList();
                Assert.AreEqual(3, linqQ1.Count);
                foreach (Document co in linqQ1)
                {
                    Assert.IsTrue(co.GetTag<int>("myint") >= myint);
                }

                Query query2 = new Query();
                query2.WhereGreaterThanOrEqual("mydouble", mydouble);
                 result = bucket.Find(query2);
                 j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                  
                    Assert.IsTrue( co.GetTag<double>("mydouble")>= mydouble);

                }
                //LINQ
                var linqQ2 = (from Document doc in this.GetBucket()
                              where doc.GetTag<double>("mydouble") >= mydouble
                              select doc).ToList();
                Assert.AreEqual(3, linqQ2.Count);
                foreach (Document co in linqQ2)
                {
                    Assert.IsTrue(co.GetTag<double>("mydouble") >= mydouble);
                }

                Query query3 = new Query();
                query3.WhereGreaterThanOrEqual("mydatetime", mydate);
                result = bucket.Find(query3);
                j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                    
                    Assert.IsTrue( co.GetTag<DateTime>("mydatetime")>=mydate);
                   
                    j++;
                }
                //LINQ
                var linqQ3 = (from Document doc in this.GetBucket()
                              where doc.GetTag<DateTime>("mydatetime") >= mydate
                              select doc).ToList();
                Assert.AreEqual(3, linqQ3.Count);
                foreach (Document co in linqQ3)
                {
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime") >= mydate);
                }

            }
        }
        [TestMethod]
        public void SearchByWhereGreaterThan()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {
                var all= bucket.LoadAll();
                List<Document> list = new List<Document>();
                int myint = rnd.Next(100000);
                double mydouble = myint + 0.34;

                DateTime mydate = new DateTime(2014, (myint % 11) + 1, (myint % 27) + 1);
                for (int i = -1; i < 3; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                    Document obj = new Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("myint", myint + i);
                    obj.SetTag("mydouble", mydouble + i);
                    obj.SetTag("mydatetime", mydate.AddDays(i));
                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereGreaterThan("myint", myint);
                var result = bucket.Find(query1);
                int j = 0;
                Assert.AreEqual(2, result.Count);
                foreach (Document co in result)
                {
                    Assert.IsTrue(co.GetTag<int>("myint") > myint);
                }
                //LINQ
                var linqQ1 = (from Document doc in this.GetBucket()
                              where doc.GetTag<int>("myint") > myint
                              select doc).ToList();
                Assert.AreEqual(2, linqQ1.Count);
                foreach (Document co in linqQ1)
                {
                    Assert.IsTrue(co.GetTag<int>("myint") > myint);
                }

                Query query2 = new Query();
                query2.WhereGreaterThan("mydouble", mydouble);
                result = bucket.Find(query2);
                j = 0;
                Assert.AreEqual(2, result.Count);
                foreach (Document co in result)
                {
                    Assert.IsTrue(co.GetTag<double>("mydouble") > mydouble);
                }
                //LINQ
                var linqQ2 = (from Document doc in this.GetBucket()
                              where doc.GetTag<double>("mydouble") > mydouble
                              select doc).ToList();
                Assert.AreEqual(2, linqQ2.Count);
                foreach (Document co in linqQ2)
                {
                    Assert.IsTrue(co.GetTag<double>("mydouble") > mydouble);
                }

                Query query3 = new Query();
                query3.WhereGreaterThan("mydatetime", mydate);
                result = bucket.Find(query3);
                j = 0;
                Assert.AreEqual(2, result.Count);
                foreach (Document co in result)
                {
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime") > mydate);
                }
                //LINQ
                var linqQ3 = (from Document doc in this.GetBucket()
                              where doc.GetTag<DateTime>("mydatetime") > mydate
                              select doc).ToList();
                Assert.AreEqual(2, linqQ3.Count);
                foreach (Document co in linqQ3)
                {
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime") > mydate);
                }
            }
        }
        [TestMethod]
        public void SearchByWhereLessThanOrEqual()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {

                List<Document> list = new List<Document>();
                int myint = rnd.Next(100000);
                double mydouble = myint + 0.34;

                DateTime mydate = new DateTime(2014, (myint % 11) + 1, (myint % 27) + 1);
                for (int i = 0; i < 4; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                    Document obj = new Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("myint", myint + i);
                    obj.SetTag("mydouble", mydouble + i);
                    obj.SetTag("mydatetime", mydate.AddDays(i));
                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereLessThanOrEqual("myint", myint+2);
                var result = bucket.Find(query1);
                int j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                   
                    Assert.IsTrue( co.GetTag<int>("myint")<= myint + 2);
                   
                }
                Query query2 = new Query();
                query2.WhereLessThanOrEqual("mydouble", mydouble+2);
                result = bucket.Find(query2);
                j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                   
                    Assert.IsTrue( co.GetTag<double>("mydouble")<= mydouble + 2);
                }
                Query query3 = new Query();
                query3.WhereLessThanOrEqual("mydatetime", mydate.AddDays(2));
                result = bucket.Find(query3);
                j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                   
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime")<= mydate.AddDays(2));
                 
                    j++;
                }

            }
        }
        [TestMethod]
        public void SearchByWhereLessThan()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {

                List<Document> list = new List<Document>();
                int myint = rnd.Next(100000);
                double mydouble = myint + 0.34;

                DateTime mydate = new DateTime(2014, (myint % 11) + 1, (myint % 27) + 1);
                for (int i = 0; i < 4; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                    Document obj = new Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("myint", myint + i);
                    obj.SetTag("mydouble", mydouble + i);
                    obj.SetTag("mydatetime", mydate.AddDays(i));
                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereLessThan("myint", myint + 2);
                var result = bucket.Find(query1);
                int j = 0;
                Assert.AreEqual(2, result.Count);
                foreach (Document co in result)
                {
                    Assert.IsTrue(co.GetTag<int>("myint") < myint + 2);
                }
                Query query2 = new Query();
                query2.WhereLessThan("mydouble", mydouble + 2);
                result = bucket.Find(query2);
                j = 0;
                Assert.AreEqual(2, result.Count);
                foreach (Document co in result)
                {
                    Assert.IsTrue(co.GetTag<double>("mydouble") < mydouble + 2);
                }
                Query query3 = new Query();
                query3.WhereLessThan("mydatetime", mydate.AddDays(2));
                result = bucket.Find(query3);
                j = 0;
                Assert.AreEqual(2, result.Count);
                foreach (Document co in result)
                {
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime") < mydate.AddDays(2));
                    j++;
                }

            }
        }
        [TestMethod]
        public void SearchByWhereBetween()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {

                List<Document> list = new List<Document>();
                int myint = rnd.Next(100000);
                double mydouble = myint + 0.34;

                DateTime mydate = new DateTime(2014, (myint % 11) + 1, (myint % 27) + 1);
                for (int i = -1; i < 4; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                    Document obj = new Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("myint", myint + i);
                    obj.SetTag("mydouble", mydouble + i);
                    obj.SetTag("mydatetime", mydate.AddDays(i));
                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereBetween("myint", myint,myint+2);
                var result = bucket.Find(query1);
                int j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {

                    Assert.IsTrue(co.GetTag<int>("myint") >= myint);
                    Assert.IsTrue(co.GetTag<int>("myint") <= myint+2);

                }
                //LINQ
                var linqQ1 = (from Document doc in this.GetBucket()
                              where doc.GetTag<int>("myint") >= myint && doc.GetTag<int>("myint") <= myint + 2
                              select doc).ToList();
                Assert.AreEqual(3, linqQ1.Count);
                foreach (Document co in linqQ1)
                {

                    Assert.IsTrue(co.GetTag<int>("myint") >= myint);
                    Assert.IsTrue(co.GetTag<int>("myint") <= myint + 2);

                }

                Query query2 = new Query();
                query2.WhereBetween("mydouble", mydouble,mydouble+2);
                result = bucket.Find(query2);
                j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                    
                    Assert.IsTrue( co.GetTag<double>("mydouble")>= mydouble);
                    Assert.IsTrue(co.GetTag<double>("mydouble") <= mydouble+2);
                    
                    j++;
                }
                //LINQ
                var linqQ2 = (from Document doc in this.GetBucket()
                              where doc.GetTag<double>("mydouble") >= mydouble && doc.GetTag<double>("mydouble") <= mydouble + 2
                              select doc).ToList();
                Assert.AreEqual(3, linqQ2.Count);
                foreach (Document co in linqQ2)
                {

                    Assert.IsTrue(co.GetTag<double>("mydouble") >= mydouble);
                    Assert.IsTrue(co.GetTag<double>("mydouble") <= mydouble + 2);

                    j++;
                }

                Query query3 = new Query();
                query3.WhereBetween("mydatetime", mydate,mydate.AddDays(2));
                result = bucket.Find(query3);
                j = 0;
                Assert.AreEqual(3, result.Count);
                foreach (Document co in result)
                {
                  
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime")>= mydate);
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime") <= mydate.AddDays(2));
                }
                //LINQ
                var linqQ3 = (from Document doc in this.GetBucket()
                              where doc.GetTag<DateTime>("mydatetime") >= mydate && doc.GetTag<DateTime>("mydatetime") <= mydate.AddDays(2)
                              select doc).ToList();

                Assert.AreEqual(3, linqQ3.Count);
                foreach (Document co in linqQ3)
                {

                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime") >= mydate);
                    Assert.IsTrue(co.GetTag<DateTime>("mydatetime") <= mydate.AddDays(2));
                }

            }
        }
        [TestMethod]
        public void SearchByWhereIN()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {

                List<Document> list = new List<Document>();
                int myint = rnd.Next(100000);
                double mydouble = myint + 0.34;

                DateTime mydate = new DateTime(2014, (myint % 11) + 1, (myint % 27) + 1);
                for (int i = -1; i < 4; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                    Document obj = new Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("myint", myint + i);
                    obj.SetTag("mydouble", mydouble + i);
                    obj.SetTag("mydatetime", mydate.AddDays(i));
                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereIN("myint", new object[] { myint, myint + 2 });
                var result = bucket.Find(query1);
              
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(myint, result[0].GetTag<int>("myint"));
                Assert.AreEqual(myint+2, result[1].GetTag<int>("myint"));

                Query query2 = new Query();
                query2.WhereIN("mydouble", new object[] { mydouble, mydouble + 2 });
                result = bucket.Find(query2);
                
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(mydouble, result[0].GetTag<double>("mydouble"));
                Assert.AreEqual(mydouble + 2, result[1].GetTag<double>("mydouble"));

                Query query3 = new Query();
                query3.WhereIN("mydatetime", new object[] { mydate, mydate.AddDays( 2 )});
                result = bucket.Find(query3);

                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(mydate, result[0].GetTag<DateTime>("mydatetime"));
                Assert.AreEqual(mydate.AddDays( 2), result[1].GetTag<DateTime>("mydatetime"));

            }
        }
        [TestMethod]
        public void SearchByTagsStartStringsOperations()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {
                lock (_syncRoot)
                {
                    List<Document> list = new List<Document>();

                    string s = GetRandomString(10);

                    for (int i = 0; i < 3; i++)
                    {
                        int rndNr = rnd.Next(100000);
                        string username = "username" + rndNr;
                        Person p = GetPerson(username);
                         Document obj = new  Document();
                        obj.SetContent<Person>(p);
                        obj.SetTag("mystr", s + i);
                        obj.SetTag("mystrCont", i + s + i);
                        obj.SetTag("mystrEnd", i + s );
                        obj.Key = username;
                        list.Add(obj);
                    }
                    bucket.StoreBatch(list);

                    Query query1 = new Query();
                    query1.WhereStartsWith("mystr", s);
                    var result = bucket.Find(query1);
                    int j = 0;
                    foreach (Document co in result)
                    {
                        Document objFromDB = result[j];
                        Assert.AreEqual(s + j, objFromDB.GetTag<string>("mystr"));
                        Assert.AreEqual(co.Key, objFromDB.Key);
                        j++;
                    }
                    Query query2 = new Query();
                    query2.WhereContains("mystrCont", s);
                    result = bucket.Find(query2);
                    j = 0;
                    foreach (Document co in list)
                    {
                        Document objFromDB = result[j];
                        Assert.AreEqual(j+ s + j, objFromDB.GetTag<string>("mystrCont"));
                        Assert.AreEqual(co.Key, objFromDB.Key);
                        j++;
                    }
                    Query query3 = new Query();
                    query3.WhereContains("mystrEnd", s);
                    result = bucket.Find(query3);
                    j = 0;
                    foreach (Document co in list)
                    {
                        Document objFromDB = result[j];
                        Assert.AreEqual(j + s , objFromDB.GetTag<string>("mystrEnd"));
                        Assert.AreEqual(co.Key, objFromDB.Key);
                        j++;
                    }

                }
            }
        }
       
        [TestMethod]
        public void SearchByKeyQuery()
        {
            IBucket bucket = this.GetBucket();
            {
                List<Document> list = new List<Document>();
                string s = GetRandomString(10);

                for (int i = 0; i < 3; i++)
                {
                    string username = "username" + s + i;
                    Person p = GetPerson(username);
                     Document obj = new  Document();
                    obj.SetContent<Person>(p);

                    obj.Key = username;
                    list.Add(obj);
                }
                bucket.StoreBatch(list);

                Query query1 = new Query();
                query1.WhereBetween("key", "username" + s, "username" + s + 2);
               
                var result = bucket.Find(query1);
                Assert.AreEqual(result.Count, list.Count);

                int j = 0;
                foreach (Document co in list)
                {
                    Document objFromDB = result[j];
                    Assert.AreEqual(co.Key, objFromDB.Key);
                    j++;
                }
                
            }
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StoreNullDoc()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;

            IBucket bucket = this.GetBucket();
            {
                bucket.Store(username, null, new { Age = 33 });
            }
        }
        [TestMethod]
        public void StoreEmptyDoc()
        {
            int rndNr = rnd.Next(100000);
            string username = "username" + rndNr;

            IBucket bucket = this.GetBucket();
            {
                Document cobj = new Document();
                cobj.Key = username;
                bucket.Store(cobj);
                var fromDB = bucket.Load(username);
                var value = fromDB.GetContent<Person>();
                Assert.AreEqual(null, value);
            }
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StoreNullOrEmptyKey()
        {

            IBucket bucket = this.GetBucket();
            {
                Document cobj = new Document();
                bucket.Store(cobj);
            }

        }
        [TestMethod]
        public void GetNonExistentDoc()
        {
            IBucket bucket = this.GetBucket();
            {

                var fromDB = bucket.Load("nontExistnet");
                Assert.IsNull(fromDB);

            }
        }
        [TestMethod]
        public void DeleteNonExistentDoc()
        {
            IBucket bucket = this.GetBucket();
            {

                bucket.Delete("nontExistnet");
                //Assert.IsNull(fromDB);

            }
        }
        [TestMethod]
        public void LoadAll()
        {
            this.DropBucket();
            IBucket bucket = this.GetBucket();
            {
                List<Document> list = new List<Document>();
                for (int i = 0; i < 3; i++)
                {
                    int rndNr = rnd.Next(100000);
                    string username = "username" + rndNr;
                    Person p = GetPerson(username);
                    Document obj = new Document();
                    obj.SetContent<Person>(p);
                    obj.SetTag("birth_year", 1980);
                    obj.Key = username;

                    list.Add(obj);
                }
                bucket.StoreBatch(list);
                var all = bucket.LoadAll();
                Assert.AreEqual(3, all.Count);
                foreach (Document co in all)
                {
                   
                    Person p = co.GetContent<Person>();
                    Assert.IsNotNull(p);
                        
                    
                }
                all = bucket.LoadAll(1,2);
                Assert.AreEqual(2, all.Count);
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
        public Person GetPerson(string username)
        {
            Person p = new Person() { UserName = username };
            p.Email = username + "@gmail.com";
            p.Age = 33;
            p.BirthDate = new DateTime(1981, 1, 4);
            p.FirstName = "Cristi";
            p.LastName = "Ursachi";
            return p;
        }
    }
    public class Person
    {
        public Person()
        {

        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public Person Friend { get; set; }
    }
    internal class MyJsonSerializer : IDocumentSerializer
    {
        #region IDocumentSerializer Members

#if !UNITY3D && !CF
        readonly JsonSerializer serializer = new JsonSerializer();
#endif
        public object Deserialize(Type type, byte[] objectBytes)
        {
#if SILVERLIGHT || CF || WinRT

            string jsonStr = Encoding.UTF8.GetString(objectBytes, 0, objectBytes.Length);

#else
            string jsonStr = Encoding.UTF8.GetString(objectBytes);

#endif
#if !UNITY3D && !CF
            return JsonConvert.DeserializeObject(jsonStr.TrimEnd('\0'), type);
#else
            LitJson.JsonReader reader = new LitJson.JsonReader(jsonStr.TrimEnd('\0'));

            return LitJson.JsonMapper.ReadValue(type, reader);
#endif

        }

        public byte[] Serialize(object obj)
        {
#if !UNITY3D && !CF
            string jsonStr = JsonConvert.SerializeObject(obj, Formatting.Indented);

#else
            string jsonStr = LitJson.JsonMapper.ToJson(obj);

#endif
            return Encoding.UTF8.GetBytes(jsonStr);
        }

        #endregion
    }
}
