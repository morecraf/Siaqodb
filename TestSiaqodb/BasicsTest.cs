using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Sqo;
using Sqo.Attributes;
using Sqo.Exceptions;
using System.Xml;
using System.IO;
using Sqo.Transactions;
using System.Collections;
using System.Diagnostics;
#if __MOBILE__
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endif


namespace TestSiaqodb
{
	#if TEST_PERF
	public class MyEntity
	{
		[SQLite.AutoIncrement, SQLite.PrimaryKey]
		public int OID { get; set; }
		public int IntValue { get; set; }
		public string StringValue { get; set; }
		public DateTime DateTimeValue { get; set; }
		public Guid GuidValue { get; set; }
		public double DoubleValue { get; set; }
	}
	#endif
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	#if __MOBILE__
	[TestFixture]
	#else
	[TestClass]

	#endif

	public class BasicsTest
	{
		string objPath;
		
		public BasicsTest()
		{
           // SiaqodbConfigurator.EncryptedDatabase = true;
           // SiaqodbConfigurator.VerboseLevel = VerboseLevel.Info;
            SiaqodbConfigurator.LoggingMethod = this.LogWarns;
            //Sqo.SiaqodbConfigurator.SetEncryptor(Sqo.BuildInAlgorithm.AES);
            Sqo.SiaqodbConfigurator.SetLicense(@" vxkmLEjihI7X+S2ottoS2Zaj8cKVLxLozBmFerFg6P8OWQqrY4O2s0tk+UnwGI6z");
#if __MOBILE__
			objPath=Environment.GetFolderPath (Environment.SpecialFolder.Personal);
#else
            objPath =@"c:\work\temp\unitTests_siaqodbLMDB\";
			#endif
		}
        public void LogWarns(string log, VerboseLevel level)    
        {
            Debug.WriteLine(log);
        }
		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion


		#if TEST_PERF
		public static int ENTITY_COUNT=100;

		#if __MOBILE__
		[Test]

		#else
		[TestMethod]

		#endif

	
		public void _TestPerformance()
		{

			Insert ();
			Read ();
		}
		public  void Read()
		{
			using (Siaqodb siaqodb = new Siaqodb())
			{
				siaqodb.Open(objPath, 100 * 1024*1024, 20);
				Console.WriteLine("ReadAllSiaqodb...");
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				var all = siaqodb.LoadAll<MyEntity>();

				stopwatch.Stop();
				Console.WriteLine("ReadAllSiaqodb took:" + stopwatch.Elapsed);

			}

			using (var dbsql = new SQLite.SQLiteConnection(objPath+@"/db.sqlite"))
			{
				Console.WriteLine("ReadAllSQLite...");
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				var all = dbsql.Query<MyEntity>("select * from MyEntity");

				stopwatch.Stop();
				Console.WriteLine("ReadAllSQLite took:" + stopwatch.Elapsed);
			}
		}

		public  void Insert()
		{
			var entities = GetEntities().ToArray();
			using (Siaqodb siaqodb = new Siaqodb())
			{
				siaqodb.Open(objPath, 100 * 1024*1024, 20);

				Console.WriteLine("InsertSiaqodb...");
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				for (int i = 0; i < ENTITY_COUNT; i++)
				{
					siaqodb.StoreObject(entities[i]);
				}

				stopwatch.Stop();
				Console.WriteLine("InsertSiaqodb took:" + stopwatch.Elapsed);

			}

			using (var dbsql = new SQLite.SQLiteConnection(objPath+@"/db.sqlite"))
			{
				dbsql.CreateTable<MyEntity>();
				Console.WriteLine("InsertSQLite...");
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				for (int i = 0; i < ENTITY_COUNT; i++)
				{
					dbsql.Insert(entities[i]);
				}

				stopwatch.Stop();
				Console.WriteLine("InsertSQLite took:" + stopwatch.Elapsed);
			}
		}
		public  IEnumerable<MyEntity> GetEntities()
		{
			var random = new Random(DateTime.Now.Millisecond);

			for (int i = 0; i < ENTITY_COUNT; i++)
			{

				yield return new MyEntity
				{
					IntValue = random.Next(),
					DoubleValue = random.NextDouble(),
					StringValue = Guid.NewGuid().ToString(),
					DateTimeValue = new DateTime(random.Next(1999, 2015), random.Next(1, 12), random.Next(1, 28)),
					GuidValue = Guid.NewGuid()
				};
			}
		}
		#endif


		#if __MOBILE__
		[Test]

		#else
		[TestMethod]

		#endif


		public void TestInsert()
		{
            using (Siaqodb nop = new Siaqodb(objPath, 50 * 1024 * 1024, 100))
            {
               
                nop.DropType<Customer>();

                for (int i = 10; i < 20; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();
                    //c.Vasiel = "momo" + i.ToString();
                    nop.StoreObject(c);
                }
                nop.Flush();
               Siaqodb.Stat dbinfo = nop.DbInfo;

                IObjectList<Customer> listC = nop.LoadAll<Customer>();
                Assert.AreEqual(listC.Count, 10);
                
            }
			
			
		}
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestStringWithoutAttribute()
        {
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();

                for (int i = 10; i < 20; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();
                    c.stringWithoutAtt = "hjqhdlkqwjhedlqkjwhedlkjqhwelkdjhqlwekhdqlkwjehdlkqwjhedlkjqhweljkdhqwlkejdhlqkwjhedlkqjwhedlkjqhwekldjhqlkwejdhlqkjwehdlkqjwhedlkjhwedkljqhweldkjhqwelkhdqlwkjehdlqkjwhedlkjqwhedlkjhqweljdhqwlekjdhlqkwjehdlkjqwhedlkjwq________________________********************************************************************";
                    nop.StoreObject(c);
                }
                nop.Flush();
                IObjectList<Customer> listC = nop.LoadAll<Customer>();

                Assert.AreEqual(100, listC[0].stringWithoutAtt.Length);
               
            }

        }
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		[ExpectedException(typeof(Sqo.Exceptions.TypeChangedException))]
		public void TestSchemaChanged()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            { //nop.DropType<Something32>();

                for (int i = 10; i < 20; i++)
                {
                    Something32 c = new Something32();
                    c.one = i;
                    c.three = i;
                    //c.two = i;

                    nop.StoreObject(c);


                }
                nop.Flush();
                IObjectList<Something32> listC = nop.LoadAll<Something32>();
                Assert.AreEqual(listC.Count, 10);

            }

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestMassInsert()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                DateTime start = DateTime.Now;
                for (int i = 0; i < 100; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();
                string t = (DateTime.Now - start).ToString();
                Console.WriteLine(t);
            }
		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestInsertAllTypeOfFields()
		{

            D40 d = new D40();
            d.b = 10;

            d.bo = true;
            d.c = 'c';
            d.d = 10;
            d.de = 10;
            d.dt = DateTime.Now;
            d.dtsofs = DateTime.Now;
            d.f = 10;
            d.g = Guid.NewGuid();
            d.ID = 10;
            d.iu = 10;
            d.l = 10;
            d.s = 1;
            d.sb = 1;
            d.ts = new TimeSpan();
            d.ul = 10;
            d.us = 1;
            d.enn = myEnum.unu;
            d.str = "Abramé";
            d.Text = "this is a text with unlimitted nr of chars! ";
            Guid g = d.g;
            TimeSpan ts = d.ts;
            DateTime dt = d.dt;
            DateTimeOffset dtsofs = d.dtsofs;
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<D40>();
               
               
                nop.StoreObject(d);

                IObjectList<D40> all1 = nop.LoadAll<D40>();
                foreach (D40 dL in all1)
                {
                    Assert.AreEqual(d.b, dL.b);
                    Assert.AreEqual(d.bo, dL.bo);
                    Assert.AreEqual(d.c, dL.c);
                    Assert.AreEqual(d.d, dL.d);
                    Assert.AreEqual(d.de, dL.de);
                    Assert.AreEqual(DateTime.Now.Month, dL.dt.Month);
                    Assert.AreEqual(DateTime.Now.Day, dL.dt.Day);
                    Assert.AreEqual(DateTime.Now.Year, dL.dt.Year);
                    Assert.AreEqual(dt, dL.dt);
                    Assert.AreEqual(dtsofs, dL.dtsofs);
                    Assert.AreEqual(dtsofs.Offset, dL.dtsofs.Offset);
                    Assert.AreEqual(d.f, dL.f);
                    Assert.AreEqual(g, dL.g);
                    Assert.AreEqual(d.ID, dL.ID);
                    Assert.AreEqual(d.iu, dL.iu);
                    Assert.AreEqual(d.l, dL.l);
                    Assert.AreEqual(d.s, dL.s);
                    Assert.AreEqual(d.sb, dL.sb);
                    Assert.AreEqual(ts, dL.ts);
                    Assert.AreEqual(d.ul, dL.ul);
                    Assert.AreEqual(d.us, dL.us);
                    Assert.AreEqual(myEnum.unu, dL.enn);
                    Assert.AreEqual("Abramé", dL.str);
                    Assert.AreEqual(d.Text, dL.Text);



                }

            }
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                IObjectList<D40> all = nop.LoadAll<D40>();
                foreach (D40 dL in all)
                {
                    Assert.AreEqual(d.b, dL.b);
                    Assert.AreEqual(d.bo, dL.bo);
                    Assert.AreEqual(d.c, dL.c);
                    Assert.AreEqual(d.d, dL.d);
                    Assert.AreEqual(d.de, dL.de);
                    Assert.AreEqual(DateTime.Now.Month, dL.dt.Month);
                    Assert.AreEqual(DateTime.Now.Day, dL.dt.Day);
                    Assert.AreEqual(DateTime.Now.Year, dL.dt.Year);
                    Assert.AreEqual(dt, dL.dt);
                    Assert.AreEqual(dtsofs, dL.dtsofs);
                    Assert.AreEqual(dtsofs.Offset, dL.dtsofs.Offset);

                    Assert.AreEqual(d.f, dL.f);
                    Assert.AreEqual(g, dL.g);
                    Assert.AreEqual(d.ID, dL.ID);
                    Assert.AreEqual(d.iu, dL.iu);
                    Assert.AreEqual(d.l, dL.l);
                    Assert.AreEqual(d.s, dL.s);
                    Assert.AreEqual(d.sb, dL.sb);
                    Assert.AreEqual(ts, dL.ts);
                    Assert.AreEqual(d.ul, dL.ul);
                    Assert.AreEqual(d.us, dL.us);
                    Assert.AreEqual(myEnum.unu, dL.enn);
                    Assert.AreEqual("Abramé", dL.str);
                    Assert.AreEqual(d.Text, dL.Text);

                }
            }

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestUpdate()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();

                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();
                IObjectList<Customer> listC = nop.LoadAll<Customer>();
                Assert.AreEqual(listC.Count, 10);
                listC[0].Name = "UPDATEWORK";

                nop.StoreObject(listC[0]);
            }
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                IObjectList<Customer> listCUpdate = nop.LoadAll<Customer>();
                Assert.AreEqual("UPDATEWORK", listCUpdate[0].Name);

            }

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestUpdateCheckNrRecords()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();

                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();
                IObjectList<Customer> listC = nop.LoadAll<Customer>();
                Assert.AreEqual(listC.Count, 10);
                listC[0].Name = "UPDATEWORK";

                nop.StoreObject(listC[0]);
            }

            using (Siaqodb nop = new Siaqodb(objPath))
            {
                IObjectList<Customer> listCUpdate = nop.LoadAll<Customer>();
                Assert.AreEqual("UPDATEWORK", listCUpdate[0].Name);
                Assert.AreEqual(10, listCUpdate.Count);

            }
			
		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestInsertAfterDrop()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();

                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();
                IObjectList<Customer> listC = nop.LoadAll<Customer>();
                nop.DropType<Customer>();
                nop.StoreObject(listC[0]);

            }
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                IObjectList<Customer> listCUpdate = nop.LoadAll<Customer>();
                Assert.AreEqual(1, listCUpdate.Count);

            }

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestSavingEvent()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();
                nop.SavingObject += new EventHandler<SavingEventsArgs>(nop_SavingObject);
                nop.SavedObject += new EventHandler<SavedEventsArgs>(nop_SavedObject);
                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();

                IObjectList<Customer> listC = nop.LoadAll<Customer>();

                Assert.AreEqual(0, listC.Count);
                Assert.AreEqual(0, nrSaves);

            }
		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestSavedEvent()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();
                nop.SavedObject += new EventHandler<SavedEventsArgs>(nop_SavedObject);
                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();

                IObjectList<Customer> listC = nop.LoadAll<Customer>();

                Assert.AreEqual(10, listC.Count);
                Assert.AreEqual(10, nrSaves);
            }

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestDelete()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();

                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();

                IObjectList<Customer> listC = nop.LoadAll<Customer>();

                nop.Delete(listC[0]);
                nop.Delete(listC[1]);

                IObjectList<Customer> listDeleted = nop.LoadAll<Customer>();

                Assert.AreEqual(8, listDeleted.Count);
                Assert.AreEqual(3, listDeleted[0].OID);
            }
		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestDeleteEvents()
		{
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();
                nop.DeletingObject += new EventHandler<DeletingEventsArgs>(nop_DeletingObject);
                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();

                IObjectList<Customer> listC = nop.LoadAll<Customer>();

                nop.Delete(listC[0]);
                nop.Delete(listC[1]);

                IObjectList<Customer> listDeleted = nop.LoadAll<Customer>();

                Assert.AreEqual(10, listDeleted.Count);
                Assert.AreEqual(1, listDeleted[0].OID);
            }

		}
        //removed for safety reason
        //#if __MOBILE__
		
        //public void TestDeleteByOID()
        //{
        //    Siaqodb nop = new Siaqodb(objPath);
        //    nop.DropType<Customer>();

        //    for (int i = 0; i < 10; i++)
        //    {
        //        Customer c = new Customer();
        //        c.ID = i;
        //        c.Name = "ADH" + i.ToString();

        //        nop.StoreObject(c);
        //    }
        //    nop.Flush();

        //    IObjectList<Customer> listC = nop.LoadAll<Customer>();

        //    nop.DeleteByOID<Customer>(listC[0].OID);
        //    nop.DeleteByOID <Customer>(listC[1].OID);

        //    IObjectList<Customer> listDeleted = nop.LoadAll<Customer>();

        //    Assert.AreEqual(8, listDeleted.Count);
        //    Assert.AreEqual(3, listDeleted[0].OID);

        //}

		void nop_DeletingObject(object sender, DeletingEventsArgs e)
		{
			e.Cancel = true;
		}
		int nrSaves = 0;
		void nop_SavedObject(object sender, SavedEventsArgs e)
		{
			nrSaves++;
		}

		void nop_SavingObject(object sender, SavingEventsArgs e)
		{
			e.Cancel = true;
			
		}
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestCount()
        {
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();
                for (int i = 0; i < 160; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();

                IObjectList<Customer> listC = nop.LoadAll<Customer>();
                nop.Delete(listC[0]);
                int count = nop.Count<Customer>();
                Assert.AreEqual(160, listC.Count);
                Assert.AreEqual(159, count);
            }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(SiaqodbException))]
        public void TestSaveDeletedObject()
        {
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();
                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();

                    nop.StoreObject(c);
                }
                nop.Flush();

                IObjectList<Customer> listC = nop.LoadAll<Customer>();
                nop.Delete(listC[0]);

                nop.StoreObject(listC[0]);
            }

          
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(SiaqodbException))]
        public void TestDeleteUnSavedObject()
        {
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<Customer>();

                Customer cu = new Customer();
                cu.ID = 78;
                nop.Delete(cu);
                nop.Close();
            }
        }
        //LMDB add back when ready
       /* #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestXMLExportImport()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Customer>();
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = null;

                sq.StoreObject(c);
            }
            sq.Flush();
            IObjectList<Customer> cust = sq.LoadAll<Customer>();
            StringBuilder sb = new StringBuilder();
            XmlWriter xmlSer = XmlWriter.Create(sb);
            sq.ExportToXML<Customer>(xmlSer);
            xmlSer.Close();

            XmlReader xmlSerRea = XmlReader.Create(new StringReader(sb.ToString()));
            IObjectList<Customer> l = sq.ImportFromXML<Customer>(xmlSerRea);

            xmlSerRea.Close();
            Assert.AreEqual(10, l.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(cust[i].ID, l[i].ID);
                Assert.AreEqual(cust[i].Name, l[i].Name);
            }
          }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestXMLExportImportCompleteType()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<D40>();
            for (int i = 0; i < 10; i++)
            {
                D40 d = new D40();
                d.b = 10;

                d.bo = true;
                d.c = 'c';
                d.d = 10;
                d.de = 10;
                d.dt = DateTime.Now;
                d.f = 10;
                d.g = Guid.NewGuid();
                d.ID = 10;
                d.iu = 10;
                d.l = 10;
                d.s = 1;
                d.sb = 1;
                d.ts = new TimeSpan();
                d.ul = 10;
                d.us = 1;
                d.enn = myEnum.doi;
                d.str = "Abramé";

                Guid g = d.g;
                TimeSpan ts = d.ts;
                DateTime dt = d.dt;

                sq.StoreObject(d);
            }
            sq.Flush();
            IObjectList<D40> cust = sq.LoadAll<D40>();
            StringBuilder sb = new StringBuilder();
            XmlWriter xmlSer = XmlWriter.Create(sb);
            sq.ExportToXML<D40>(xmlSer);
            xmlSer.Close();

            XmlReader xmlSerRea = XmlReader.Create(new StringReader(sb.ToString()));
            IObjectList<D40> l = sq.ImportFromXML<D40>(xmlSerRea);

            xmlSerRea.Close();
            Assert.AreEqual(10, l.Count);
            for (int i = 0; i < 10; i++)
            {
                D40 d = cust[i];
                D40 dL = l[i];
                Assert.AreEqual(d.b, dL.b);
                Assert.AreEqual(d.bo, dL.bo);
                Assert.AreEqual(d.c, dL.c);
                Assert.AreEqual(d.d, dL.d);
                Assert.AreEqual(d.de, dL.de);
                Assert.AreEqual(DateTime.Now.Month, dL.dt.Month);
                Assert.AreEqual(DateTime.Now.Day, dL.dt.Day);
                Assert.AreEqual(DateTime.Now.Year, dL.dt.Year);
                Assert.AreEqual(d.dt, dL.dt);
                Assert.AreEqual(d.f, dL.f);
                Assert.AreEqual(d.g, dL.g);
                Assert.AreEqual(d.ID, dL.ID);
                Assert.AreEqual(d.iu, dL.iu);
                Assert.AreEqual(d.l, dL.l);
                Assert.AreEqual(d.s, dL.s);
                Assert.AreEqual(d.sb, dL.sb);
                Assert.AreEqual(d.ts, dL.ts);
                Assert.AreEqual(d.ul, dL.ul);
                Assert.AreEqual(d.us, dL.us);
                Assert.AreEqual(myEnum.doi, dL.enn);
                Assert.AreEqual("Abramé", dL.str);
            }
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestXMLExportImportCompleteTypeNullable()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<D40Nullable>();
            for (int i = 0; i < 10; i++)
            {
                D40Nullable d = new D40Nullable();
                d.b = 10;

               

                sq.StoreObject(d);
            }
            sq.Flush();
            IObjectList<D40Nullable> cust = sq.LoadAll<D40Nullable>();
            StringBuilder sb = new StringBuilder();
            XmlWriter xmlSer = XmlWriter.Create(sb);
            sq.ExportToXML<D40Nullable>(xmlSer);
            xmlSer.Close();

            XmlReader xmlSerRea = XmlReader.Create(new StringReader(sb.ToString()));
            IObjectList<D40Nullable> l = sq.ImportFromXML<D40Nullable>(xmlSerRea);

            xmlSerRea.Close();
            Assert.AreEqual(10, l.Count);
           }
            
        }*/
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(UniqueConstraintException))]
        public void TestUniqueExceptionInsert()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ItemUnique>();

            ItemUnique c = new ItemUnique();
            c.Age = 10;
            c.S = "ceva";

            sq.StoreObject(c);
            c.S = "cevaa";
            sq.StoreObject(c);
            sq.Flush();

            ItemUnique c1 = new ItemUnique();
            c1.Age = 11;
            c1.S = "cevaa";

            sq.StoreObject(c1);

           }
            
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(UniqueConstraintException))]
        public void TestUniqueExceptionInsertTransaction()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ItemUnique>();

            ItemUnique c = new ItemUnique();
            c.Age = 10;
            c.S = "ceva";

            sq.StoreObject(c);
            c.S = "cevaa";
            sq.StoreObject(c);
            sq.Flush();

            ItemUnique c1 = new ItemUnique();
            c1.Age = 11;
            c1.S = "cevaa";

            ITransaction tr = sq.BeginTransaction();
            sq.StoreObject(c1,tr);
            tr.Commit();

           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(UniqueConstraintException))]
        public void TestUniqueExceptionUpdate()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ItemUnique>();

            ItemUnique c = new ItemUnique();
            c.Age = 10;
            c.S = "ceva";

            sq.StoreObject(c);
            c.S = "ceva";
            sq.StoreObject(c);
            sq.Flush();

            ItemUnique c1 = new ItemUnique();
            c1.Age = 11;
            c1.S = "ceva1";

            sq.StoreObject(c1);

            IObjectList<ItemUnique> list = sq.LoadAll<ItemUnique>();
            list[1].S = "ceva";
            sq.StoreObject(list[1]);//should throw exception

           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestUpdateObjectBy()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ItemUnique>();

            ItemUnique c = new ItemUnique();
            c.Age = 10;
            c.S = "some";
            sq.StoreObject(c);

            ItemUnique c1 = new ItemUnique();
            c1.Age = 11;
            c1.S = "some1";

            sq.StoreObject(c1);

            ItemUnique it = new ItemUnique();
            it.Age = 11;
            it.S="someNew";
            bool stored= sq.UpdateObjectBy("Age", it);
            Assert.IsTrue(stored);

            IObjectList<ItemUnique> list = sq.LoadAll<ItemUnique>();
            
            Assert.AreEqual( "someNew", list[1].S) ;
          

            it = new ItemUnique();
            it.Age = 13;
            it.S = "someNew";
            stored = sq.UpdateObjectBy("Age", it);
            Assert.IsFalse(stored);
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(SiaqodbException))]
        public void TestUpdateObjectByDuplicates()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;

            sq.StoreObject(emp);

            emp = new Employee();
            emp.ID = 100;
            sq.StoreObject(emp);

            emp = new Employee();
            emp.ID = 100;

            sq.UpdateObjectBy("ID", emp);

           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(SiaqodbException))]
        public void TestUpdateObjectByFieldNotExists()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;

            sq.StoreObject(emp);

            sq.UpdateObjectBy("IDhh", emp);

           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(SiaqodbException))]
        public void TestUpdateObjectByManyFieldsDuplicates()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;

            sq.StoreObject(emp);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;

            sq.StoreObject(emp);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;

            sq.UpdateObjectBy(emp,"ID","CustomerID");
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestUpdateObjectByManyFields()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);

         
            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";

            bool s=sq.UpdateObjectBy(emp, "ID", "CustomerID","Name");

            Assert.IsTrue(s);
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestDeleteObjectBy()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);


            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";

            bool s = sq.DeleteObjectBy(emp, "ID", "CustomerID", "Name");

            Assert.IsTrue(s);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);

            emp = new Employee();
            emp.ID = 100;

            s = sq.DeleteObjectBy("ID",emp);

            Assert.IsTrue(s);
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(UniqueConstraintException))]
        public void TestUpdateObjectByManyFieldsConstraints()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ItemUnique>();

            ItemUnique emp = new ItemUnique();
            emp.Age = 100;
            emp.integ = 10;
            emp.S = "g";
            sq.StoreObject(emp);

            emp = new ItemUnique();
            emp.Age = 110;
            emp.integ = 10;
            emp.S = "gg";
            sq.StoreObject(emp);

            emp = new ItemUnique();
            emp.Age = 100;
            emp.integ = 10;
            emp.S = "gge";

            
            bool s = sq.UpdateObjectBy(emp, "Age", "integ");
            Assert.IsTrue(s);

            emp = new ItemUnique();
            emp.Age = 100;
            emp.integ = 10;
            emp.S = "gg";

            s = sq.UpdateObjectBy(emp, "Age", "integ");
           }
        }

        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestEventsVariable()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            //sq.DropType<ClassWithEvents>();

            ClassWithEvents c = new ClassWithEvents();
            c.one = 10;


            sq.StoreObject(c);
            IObjectList<ClassWithEvents> ll = sq.LoadAll<ClassWithEvents>();

           }


        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestIndexFirstInsert()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                sq.StoreObject(cls);
            }
            var q = from ClassIndexes clss in sq
                    where clss.one == 9
                    select clss;

            
            Assert.AreEqual(10, q.Count<ClassIndexes>());

            
           }
          using (Siaqodb sq = new Siaqodb(objPath))
          {
              var q = from ClassIndexes clss in sq
                  where clss.two == 10
                  select clss;


              Assert.AreEqual(10, q.Count<ClassIndexes>());
          }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestIndexUpdate()
        {
            using (Siaqodb sq = new Siaqodb(objPath))
            {
                sq.DropType<ClassIndexes>();
                for (int i = 0; i < 100; i++)
                {
                    ClassIndexes cls = new ClassIndexes();
                    cls.one = i % 10;
                    cls.two = i % 10 + 1;
                    sq.StoreObject(cls);
                }
            }
            using (Siaqodb sq = new Siaqodb(objPath))
            {
                var q = from ClassIndexes clss in sq
                        where clss.one == 9
                        select clss;


                q.ToList<ClassIndexes>()[0].one = 5;

                sq.StoreObject(q.ToList<ClassIndexes>()[0]);

                sq.StoreObject(q.ToList<ClassIndexes>()[1]);//just update nothing change
            }
            using (Siaqodb sq = new Siaqodb(objPath))
            {

                var q = from ClassIndexes clss in sq
                    where clss.one == 9
                    select clss;


                Assert.AreEqual(9, q.Count<ClassIndexes>());

                 q = from ClassIndexes clss in sq
                    where clss.one == 5
                    select clss;


                Assert.AreEqual(11, q.Count<ClassIndexes>());
            }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestIndexSaveAndClose()
        {
            using (Siaqodb sq = new Siaqodb(objPath))
            {
                sq.DropType<ClassIndexes>();
                for (int i = 0; i < 100; i++)
                {
                    ClassIndexes cls = new ClassIndexes();
                    cls.one = i % 10;
                    cls.two = i % 10 + 1;
                    sq.StoreObject(cls);
                }

            }
            using (Siaqodb sq = new Siaqodb(objPath))
            {
                var q = from ClassIndexes clss in sq
                        where clss.one == 9
                        select clss;


                Assert.AreEqual(10, q.Count<ClassIndexes>());
            }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestIndexAllOperations()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                sq.StoreObject(cls);
            }
           }
            using(Siaqodb sq = new Siaqodb(objPath))
            {
            var q = from ClassIndexes clss in sq
                    where clss.one <=2
                    select clss;


            Assert.AreEqual(30, q.Count<ClassIndexes>());
            
             q = from ClassIndexes clss in sq
                    where clss.one < 2
                    select clss;


            Assert.AreEqual(20, q.Count<ClassIndexes>());
            q = from ClassIndexes clss in sq
                where clss.one >= 2
                select clss;


            Assert.AreEqual(80, q.Count<ClassIndexes>());
            q = from ClassIndexes clss in sq
                where clss.one > 2
                select clss;


            Assert.AreEqual(70, q.Count<ClassIndexes>());
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestIndexUpdateObjectBy()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                cls.ID=i;
                cls.ID2=i;
                sq.StoreObject(cls);
            }
          }

            using(  Siaqodb sq = new Siaqodb(objPath)) {
            var q = from ClassIndexes clss in sq
                    where clss.two == 4
                    select clss;

            q.ToList<ClassIndexes>()[0].two = 5;
            sq.UpdateObjectBy("ID", q.ToList<ClassIndexes>()[0]);

            q = from ClassIndexes clss in sq
                where clss.two == 4
                select clss;

            Assert.AreEqual(9, q.Count<ClassIndexes>());

            q = from ClassIndexes clss in sq
                where clss.two == 5
                select clss;
            Assert.AreEqual(11, q.Count<ClassIndexes>());
            
            q.ToList<ClassIndexes>()[0].two = 6;
            sq.UpdateObjectBy("ID2", q.ToList<ClassIndexes>()[0]);

            q = from ClassIndexes clss in sq
                where clss.two == 5
                select clss;
            Assert.AreEqual(10, q.Count<ClassIndexes>());

            q = from ClassIndexes clss in sq
                where clss.two == 6
                select clss;
            Assert.AreEqual(11, q.Count<ClassIndexes>());

           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestIndexDelete()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                cls.ID = i;
                cls.ID2 = i;
                sq.StoreObject(cls);
            }
          }
          using (Siaqodb sq = new Siaqodb(objPath))
          {

            var q = from ClassIndexes clss in sq
                    where clss.two == 7
                    select clss;


            sq.Delete(q.ToList<ClassIndexes>()[0]);
          }
          using (Siaqodb sq = new Siaqodb(objPath))
          {
            var q = from ClassIndexes clss in sq
                where clss.two == 7
                select clss;

            Assert.AreEqual(9, q.Count<ClassIndexes>());

            sq.DeleteObjectBy("ID", q.ToList<ClassIndexes>()[0]);
          }
          using (Siaqodb sq = new Siaqodb(objPath))
          {
            var q = from ClassIndexes clss in sq
                where clss.two == 7
                select clss;

            Assert.AreEqual(8, q.Count<ClassIndexes>());


            sq.DeleteObjectBy("ID2", q.ToList<ClassIndexes>()[0]);

            q = from ClassIndexes clss in sq
                where clss.two == 7
                select clss;

            Assert.AreEqual(7, q.Count<ClassIndexes>());
           }
        }
#if __MOBILE__
		[Test]
#else
        [TestMethod]

#endif
        public void TestIndexAllFieldTypes()
        {
            DateTime dt = new DateTime(2010, 1, 1);
            Guid guid = Guid.NewGuid();
            TimeSpan tspan = new TimeSpan();
            using (Siaqodb sq = new Siaqodb(objPath))
            {
                sq.DropType<D40WithIndexes>();


                for (int i = 0; i < 1000; i++)
                {
                    D40WithIndexes d = new D40WithIndexes();
                    d.b = Convert.ToByte(i % 128);

                    d.bo = i % 2 == 0;
                    d.c = 'c';
                    d.d = i;
                    d.de = i;
                    d.dt = dt.AddDays(i);
                    d.f = i;
                    d.g = guid;
                    d.ID = i;
                    d.iu =(uint) i;
                    d.l = i;
                    d.s = (short)i;
                    d.sb = (sbyte)Convert.ToByte(i % 128);
                    d.ts = tspan;
                    d.ul = (ulong)i;
                    d.us = (ushort)i;
                    d.enn = myEnum.unu;
                    d.str = "Abramé"+i;


                    sq.StoreObject(d);
                }
                sq.DropType<ClassIndexes>();
                for (int i = 0; i < 100; i++)
                {
                    ClassIndexes cls = new ClassIndexes();
                    cls.one = i % 10;
                    cls.two = i % 10 + 1;
                    cls.ID = i;
                    cls.ID2 = i;
                    sq.StoreObject(cls);
                }
            }
            using (Siaqodb sq = new Siaqodb(objPath))
            {
                byte byt = 5;
                var q1 = from D40WithIndexes di in sq
                         where di.b == byt
                         select di;


                Assert.AreEqual(8, q1.ToList().Count);

                var q2 = from D40WithIndexes di in sq
                         where di.bo == true
                         select di;

                Assert.AreEqual(500, q2.ToList().Count);

                var q3 = from D40WithIndexes di in sq
                         where di.c == (char)'c'
                         select di;

                  Assert.AreEqual(1000, q3.ToList().Count);

                var q4 = from D40WithIndexes di in sq
                         where di.d == 700
                         select di;

                Assert.AreEqual(1, q4.ToList().Count);
                var q4r = from D40WithIndexes di in sq
                          where di.d <= 800 && di.d > 100
                          select di;

                Assert.AreEqual(700, q4r.ToList().Count);

                var q5 = from D40WithIndexes di in sq
                         where di.de == 512
                         select di;

                Assert.AreEqual(1, q5.ToList().Count);
                var q5r = from D40WithIndexes di in sq
                         where di.de > 500 && di.de<=900
                         select di;

                Assert.AreEqual(400, q5r.ToList().Count);

                var mytestdate=new DateTime(2010, 1, 30);
                var q6 = from D40WithIndexes di in sq
                         where di.dt == mytestdate
                         select di;

                Assert.AreEqual(1, q6.ToList().Count);
                var mytestdate2 = new DateTime(2010, 1, 30);
                var mytestdate3 = new DateTime(2010, 2, 15);
                var q6r = from D40WithIndexes di in sq
                         where di.dt > mytestdate2 & di.dt< mytestdate3
                          select di;

                Assert.AreEqual(15, q6r.ToList().Count);

                var q7 = from D40WithIndexes di in sq
                         where di.enn == myEnum.unu
                         select di;

                Assert.AreEqual(1000, q7.ToList().Count);

                var q8 = from D40WithIndexes di in sq
                         where di.f == 680
                         select di;

                Assert.AreEqual(1, q8.ToList().Count);
                var q8r = from D40WithIndexes di in sq
                          where di.f >= 10 && di.f < 500
                          select di;

                Assert.AreEqual(490, q8r.ToList().Count);

                var q9 = from D40WithIndexes di in sq
                         where di.g == guid
                         select di;

               // Assert.AreEqual(1000, q9.ToList().Count);

                var q10 = from D40WithIndexes di in sq
                          where di.iu == 794
                          select di;

                Assert.AreEqual(1, q10.ToList().Count);

                var q10r = from D40WithIndexes di in sq
                          where di.iu >= 700 && di.iu<1000
                          select di;

                Assert.AreEqual(300, q10r.ToList().Count);

                var q11 = from D40WithIndexes di in sq
                          where di.l == 7
                          select di;

                Assert.AreEqual(1, q11.ToList().Count);

                var q11r = from D40WithIndexes di in sq
                          where di.l >50 && di.l<=100
                          select di;

                Assert.AreEqual(50, q11r.ToList().Count);

                
                var q12 = (from D40WithIndexes di in sq
                          where di.s == 567
                          select di).ToList();

                Assert.AreEqual(1, q12.ToList().Count);

                var q12r = (from D40WithIndexes di in sq
                           where di.s > 200 && di.s<=700
                           select di).ToList();

                Assert.AreEqual(500, q12r.ToList().Count);

                var q13 = from D40WithIndexes di in sq
                          where di.sb == (sbyte)67
                          select di;

                Assert.AreEqual(8, q13.ToList().Count);

                var q14 = from D40WithIndexes di in sq
                          where di.str.StartsWith("Abr")
                          select di;

                Assert.AreEqual(1000, q14.ToList().Count);

                var q15 = from D40WithIndexes di in sq
                          where di.ts == tspan
                          select di;

                Assert.AreEqual(1000, q15.ToList().Count);

                var q16 = from D40WithIndexes di in sq
                          where di.ul == 10
                          select di;

                Assert.AreEqual(1, q16.ToList().Count);

                var q16r = from D40WithIndexes di in sq
                          where di.ul >=300 && di.ul<1000
                          select di;

                Assert.AreEqual(700, q16r.ToList().Count);


                var q17 = from D40WithIndexes di in sq
                          where di.us == 435
                          select di;

               Assert.AreEqual(1, q17.ToList().Count);

                var q17r = from D40WithIndexes di in sq
                          where di.us >= 10 && di.us<900
                          select di;

                Assert.AreEqual(890, q17r.ToList().Count);


                var q18 = from ClassIndexes clss in sq
                          where clss.two == 7
                          select clss;

                Assert.AreEqual(10, q18.ToList().Count);

                var q19 = from D40WithIndexes di in sq
                          where di.Text == "text longgg"
                          select di;

                Assert.AreEqual(1000, q19.ToList().Count);
            }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestAttributesOnProps()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ClassWithPropertiesAtt>();
            for (int i = 0; i < 10; i++)
            {
                ClassWithPropertiesAtt cls = new ClassWithPropertiesAtt();
                cls.ID = i % 2;
                cls.MyProperty = i+1 ;
                cls.Stringss = "dsdsdsds";
                cls.Uniq = i;
                sq.StoreObject(cls);
            }


            var q = from ClassWithPropertiesAtt clss in sq
                    where clss.ID == 1
                    select clss;

            Assert.AreEqual(5, q.Count<ClassWithPropertiesAtt>());
            //check ignore work
            Assert.AreEqual(0, q.ToList<ClassWithPropertiesAtt>()[0].MyProperty);

            Assert.AreEqual(3, q.ToList<ClassWithPropertiesAtt>()[0].Stringss.Length);

            q.ToList<ClassWithPropertiesAtt>()[0].Uniq = 0;
            bool except = false;
            try
            {

                sq.StoreObject(q.ToList<ClassWithPropertiesAtt>()[0]);
            }
            catch (UniqueConstraintException ex)
            {
                except = true;
            }
            Assert.AreEqual(true, except);
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestPOCO()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<POCO>();
            for (int i = 0; i < 10; i++)
            {
                POCO cls = new POCO();
                cls.ID = i % 2;
                cls.MyProperty = i + 1;
                cls.Stringss = "dsdsdsds";
                cls.Uniq = i;
                sq.StoreObject(cls);
            }


            var q = from POCO clss in sq
                    where clss.ID == 1
                    select clss;

            Assert.AreEqual(5, q.Count<POCO>());
            //check ignore work
            Assert.AreEqual(0, q.ToList<POCO>()[0].MyProperty);

            Assert.AreEqual(3, q.ToList<POCO>()[0].Stringss.Length);

            q.ToList<POCO>()[0].Uniq = 0;
            bool except = false;
            try
            {

                sq.StoreObject(q.ToList<POCO>()[0]);
            }
            catch (UniqueConstraintException ex)
            {
                except = true;
            }
            Assert.AreEqual(true, except);
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestRealPOCO()
        {
            SiaqodbConfigurator.AddIndex("ID", typeof(RealPOCO));
            SiaqodbConfigurator.AddIndex("ID", typeof(RealPOCO1));
            
            SiaqodbConfigurator.AddUniqueConstraint("UID", typeof(RealPOCO));
            SiaqodbConfigurator.AddUniqueConstraint("UID", typeof(RealPOCO1));
           
            SiaqodbConfigurator.AddIgnore("ignoredField", typeof(RealPOCO));
            SiaqodbConfigurator.AddIgnore("ignoredField", typeof(RealPOCO1));

            SiaqodbConfigurator.AddIgnore("IgnoredProp", typeof(RealPOCO));
            SiaqodbConfigurator.AddIgnore("IgnoredProp", typeof(RealPOCO1));
            
            SiaqodbConfigurator.AddMaxLength("MyStr",3, typeof(RealPOCO));
            SiaqodbConfigurator.AddMaxLength("MyStr", 3, typeof(RealPOCO1));

            SiaqodbConfigurator.AddMaxLength("mystr", 3, typeof(RealPOCO));
            SiaqodbConfigurator.AddMaxLength("mystr", 3, typeof(RealPOCO1));

            SiaqodbConfigurator.PropertyUseField("MyStrProp", "mystr", typeof(RealPOCO));
            SiaqodbConfigurator.PropertyUseField("MyStrProp", "mystr", typeof(RealPOCO1));

          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<RealPOCO>();
            for (int i = 0; i < 10; i++)
            {
                RealPOCO cls = new RealPOCO();
                cls.ID = i % 2;
                cls.Test = i + 1;
                cls.UID = Guid.NewGuid();
                cls.ignoredField = i;
                cls.IgnoredProp = i;
                cls.mystr = "dqwsdasdasdas";
                cls.MyStr = "dqwqwdqad";
                sq.StoreObject(cls);
            }


            var q = from RealPOCO clss in sq
                    where clss.ID == 1
                    select clss;

            Assert.AreEqual(5, q.Count<RealPOCO>());

           }

            using(  Siaqodb sq = new Siaqodb(objPath)) {
             var q = from RealPOCO clss in sq
                    where clss.ID == 1
                    select clss;

            Assert.AreEqual(5, q.Count<RealPOCO>());

            RealPOCO o1 = q.ToList<RealPOCO>()[0];
            RealPOCO o2 = q.ToList<RealPOCO>()[1];
            
            //check if ignore work
            Assert.AreEqual(0, o1.ignoredField);
            Assert.AreEqual(0, o1.IgnoredProp);

            //check maxLength work
            Assert.AreEqual(3, o1.MyStr.Length);
            Assert.AreEqual(3, o1.mystr.Length);


            o2.UID = o1.UID;
            bool excp=false;
            try
            {
                sq.StoreObject(o2);
            }
            catch (UniqueConstraintException ex)
            {
                excp = true;
            }

            Assert.AreEqual(true, excp);

            //check if mapping works
            q = from RealPOCO clss in sq
                where clss.MyStrProp == "dqw"
                select clss;

            Assert.AreEqual(10, q.ToList<RealPOCO>().Count);
            sq.DropType<RealPOCO>();
            sq.DropType<RealPOCO1>();
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestOptimisticConcurency()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ForConcurencyClass>();
            List<ForConcurencyClass> lis = new List<ForConcurencyClass>();
            for (int i = 0; i < 10; i++)
            {
                ForConcurencyClass c = new ForConcurencyClass();
                c.integ = i+1;
                c.test = "test";
                sq.StoreObject(c);
                lis.Add(c);
            }
            var q = from ForConcurencyClass cc in sq
                    select cc;
            ForConcurencyClass de = q.ToList<ForConcurencyClass>()[0];

            de.test = "d";
            sq.StoreObject(de);
            int exCatched = 0;
            try
            {
                sq.StoreObject(lis[0]);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            sq.StoreObject(de);

             q = from ForConcurencyClass cc in sq
                    select cc;
            ForConcurencyClass de2 = q.ToList<ForConcurencyClass>()[0];

            sq.StoreObject(de2);

            ForConcurencyClass newObj = new ForConcurencyClass();
            newObj.integ = 1;

            sq.UpdateObjectBy("integ", newObj);
            exCatched = 0;
            try
            {

                sq.StoreObject(de2);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            sq.StoreObject(newObj);

            q = from ForConcurencyClass cc in sq
                select cc;
            ForConcurencyClass de3 = q.ToList<ForConcurencyClass>()[0];

            sq.Delete(newObj);

            exCatched = 0;
            try
            {

                sq.StoreObject(de3);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            q = from ForConcurencyClass cc in sq
                select cc;
            ForConcurencyClass de4 = q.ToList<ForConcurencyClass>()[0];

            ForConcurencyClass de4bis = q.ToList<ForConcurencyClass>()[1];

            var q1 = from ForConcurencyClass cc in sq
                select cc;

            ForConcurencyClass de5 = q1.ToList<ForConcurencyClass>()[0];

            sq.StoreObject(de4);

            exCatched = 0;
            try
            {

                sq.Delete(de5);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            ForConcurencyClass de6 = new ForConcurencyClass();
            de6.integ = 3;


            sq.DeleteObjectBy("integ",de6);


            exCatched = 0;
            try
            {

                sq.StoreObject(de4bis);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);
           }



        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestTransactionInsert()
        { 
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();
            try
            {
                
                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.Name = "GTA" + i.ToString();
                    sq.StoreObject(c, transact);
                }
               
                list = sq.LoadAll<Customer>(transact);
                Assert.AreEqual(10, list.Count);


                transact.Commit();

            }
            catch (Exception ex)
            {
                transact.Rollback();
            }
            list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);
           }
            using(  Siaqodb sq = new Siaqodb(objPath)) {
            var list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);


            var transact = sq.BeginTransaction();
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.Name = "GTA" + i.ToString();
                    sq.StoreObject(c, transact);
                    if (i == 9)
                    {
                        throw new Exception("fsdfsd");
                    }
                }
                transact.Commit();
            }
            catch(Exception ex)
            {
                transact.Rollback();
            }

            list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);

           }
            
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestTransactionUpdateInsert()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c,transact);//without transact
            }

            list = sq.LoadAll<Customer>(transact);
            Assert.AreEqual(10, list.Count);
            transact.Commit();
            transact = sq.BeginTransaction();
            foreach(Customer c in list)
            {
                c.Name = "updated";
                sq.StoreObject(c, transact);
            }
            transact.Rollback();
            
            list = sq.LoadAll<Customer>();
            
            foreach (Customer c in list)
            {
                Assert.AreEqual("GTA", c.Name.Substring(0, 3));
            }
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestTransactionDelete()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c, transact);
            }
            list = sq.LoadAll<Customer>(transact);
            sq.Delete(list[0], transact);
            sq.Delete(list[1], transact);
            bool rollback = false;
            try
            {
                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                list = sq.LoadAll<Customer>();
                Assert.AreEqual(10, list.Count);
                rollback = true;

            }
            if (!rollback)
            {
                list = sq.LoadAll<Customer>();
                Assert.AreEqual(8, list.Count);
                
            }
           }
            
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestUpdateObjectByManyFieldsTransaction()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);


            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "shuhu";
            
            bool s = sq.UpdateObjectBy(emp, "ID", "CustomerID");

            Assert.IsTrue(s);
            IList<Employee> list = sq.LoadAll<Employee>();
            Assert.AreEqual(list[0].Name ,emp.Name);

            emp.Name = "ANOTHER";
            ITransaction tr = sq.BeginTransaction();
            sq.UpdateObjectBy(emp, tr, "ID", "CustomerID");

            tr.Commit();
            list = sq.LoadAll<Employee>();
            Assert.AreEqual(list[0].Name, emp.Name);

            tr = sq.BeginTransaction();
            emp.Name = "test";
            
            sq.UpdateObjectBy(emp, tr, "ID", "CustomerID");

            tr.Rollback();
            list = sq.LoadAll<Employee>();
            Assert.AreEqual(list[0].Name, "ANOTHER");
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestDeleteObjectByTransactions()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);


            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;

            ITransaction trans = sq.BeginTransaction();

            bool s = sq.DeleteObjectBy(emp,trans, "ID", "CustomerID");
            Assert.IsTrue(s);
            trans.Commit();
            IList<Employee> list = sq.LoadAll<Employee>();
            Assert.AreEqual(list.Count, 0);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);

            
            trans=sq.BeginTransaction();
            s = sq.DeleteObjectBy(emp, trans, "ID", "CustomerID");
            trans.Rollback();

            list = sq.LoadAll<Employee>();
            Assert.AreEqual(list.Count, 1);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);
           
 
            trans = sq.BeginTransaction();
            try
            {
                s = sq.DeleteObjectBy(emp, trans, "ID", "CustomerID");

            
                trans.Commit();
            }
            catch 
            {
                trans.Rollback();    
            }

            list = sq.LoadAll<Employee>();
            Assert.AreEqual(list.Count, 2);
           }
            
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestTransactionCrash()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {

            IList<Customer> list = sq.LoadAll<Customer>();
            IList<Employee> list2 = sq.LoadAll<Employee>();

            sq.DropType<Customer>();
            sq.DropType<Employee>();

            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c,transact);
                Employee e = new Employee();
                e.Name = "EMP" + i.ToString();
                sq.StoreObject(e,transact);
            }

            transact.Commit();


            list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);

             list2 = sq.LoadAll<Employee>();
            Assert.AreEqual(10, list2.Count);

            ITransaction transac2t = sq.BeginTransaction();

            sq.Delete(list[5], transac2t);
            sq.Delete(list2[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name="updated";
                list2[i].Name="updatedE";
                sq.StoreObject(list[i], transac2t);
                sq.StoreObject(list2[i], transac2t);
                sq.StoreObject(new Customer(),transac2t);
                sq.StoreObject(new Employee(),transac2t);
            }


            transac2t.Commit();//here do debug and stop after a few commits to be able to simulate crash recovery
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestTransactionManyTypes()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {

            
            sq.DropType<Customer>();
            sq.DropType<Employee>();
            sq.DropType<D40>();
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c, transact);
                Employee e = new Employee();
                e.Name = "EMP" + i.ToString();
                sq.StoreObject(e, transact);

                D40 d = new D40();
                sq.StoreObject(d,transact);
            }

            transact.Commit();


            IList<Customer> list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);

            IList<Employee> list2 = sq.LoadAll<Employee>();
            Assert.AreEqual(10, list2.Count);

            IList<D40> list3 = sq.LoadAll<D40>();
            Assert.AreEqual(10, list3.Count);

            ITransaction transac2t = sq.BeginTransaction();

            sq.Delete(list[5], transac2t);
            sq.Delete(list2[5], transac2t);
            sq.Delete(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updated";
                list2[i].Name = "updatedE";
                sq.StoreObject(list[i], transac2t);
                sq.StoreObject(list2[i], transac2t);
                sq.StoreObject(new Customer(), transac2t);
                sq.StoreObject(new Employee(), transac2t);
            }


            transac2t.Commit();

            list = sq.LoadAll<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = sq.LoadAll<Employee>();
            Assert.AreEqual(13, list2.Count);

            list3 = sq.LoadAll<D40>();
            Assert.AreEqual(9, list3.Count);
            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");

            transac2t = sq.BeginTransaction();

            sq.Delete(list[5], transac2t);
            sq.Delete(list2[5], transac2t);
            sq.Delete(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updatedRoll";
                list2[i].Name = "updatedERoll";
                sq.StoreObject(list[i], transac2t);
                sq.StoreObject(list2[i], transac2t);
                sq.StoreObject(new Customer(), transac2t);
                sq.StoreObject(new Employee(), transac2t);
            }

            transac2t.Rollback();

            list = sq.LoadAll<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = sq.LoadAll<Employee>();
            Assert.AreEqual(13, list2.Count);

            list3 = sq.LoadAll<D40>();
            Assert.AreEqual(9, list3.Count);

            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");
           }

        }
         #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestTransactionLists()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {


            sq.DropType<Customer>();
            sq.DropType<Employee>();
            sq.DropType<D40WithLists>();
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c, transact);
                Employee e = new Employee();
                e.Name = "EMP" + i.ToString();
                sq.StoreObject(e, transact);

                D40WithLists d = new D40WithLists();
                d.dt = new List<DateTime>();
                d.dt.Add ( DateTime.Now);
                sq.StoreObject(d, transact);
            }

            transact.Commit();


            IList<Customer> list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);

            IList<Employee> list2 = sq.LoadAll<Employee>();
            Assert.AreEqual(10, list2.Count);

            IList<D40WithLists> list3 = sq.LoadAll<D40WithLists>();
            Assert.AreEqual(10, list3.Count);

            ITransaction transac2t = sq.BeginTransaction();

            sq.Delete(list[5], transac2t);
            sq.Delete(list2[5], transac2t);
            sq.Delete(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updated";
                list2[i].Name = "updatedE";
                list3[i].dt[0] = new DateTime(2007, 1, 1);
                sq.StoreObject(list[i], transac2t);
                sq.StoreObject(list2[i], transac2t);
                sq.StoreObject(list3[i], transac2t);
                sq.StoreObject(new Customer(), transac2t);
                sq.StoreObject(new Employee(), transac2t);
                sq.StoreObject(new D40WithLists(), transac2t);
            }


            transac2t.Commit();

            list = sq.LoadAll<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = sq.LoadAll<Employee>();
            Assert.AreEqual(13, list2.Count);

            list3 = sq.LoadAll<D40WithLists>();
            Assert.AreEqual(13, list3.Count);
            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");
            Assert.AreEqual(list3[0].dt[0], new DateTime(2007, 1, 1));

            transac2t = sq.BeginTransaction();

            sq.Delete(list[5], transac2t);
            sq.Delete(list2[5], transac2t);
            sq.Delete(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updatedRoll";
                list2[i].Name = "updatedERoll";
                list3[i].dt[0] = new DateTime(2008, 3, 3);
                sq.StoreObject(list[i], transac2t);
                sq.StoreObject(list2[i], transac2t);
                sq.StoreObject(new Customer(), transac2t);
                sq.StoreObject(new Employee(), transac2t);
                
                sq.StoreObject(list3[i], transac2t);
            }

            transac2t.Rollback();

            list = sq.LoadAll<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = sq.LoadAll<Employee>();
            Assert.AreEqual(13, list2.Count);

            list3 = sq.LoadAll<D40WithLists>();
            Assert.AreEqual(13, list3.Count);

            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");
            Assert.AreEqual(list3[0].dt[0], new DateTime(2007, 1, 1));
           }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestIndexStringStartWith()
        {
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<ClassIndexesString>();
            ClassIndexesString cls = new ClassIndexesString() { Name = "aaa" };
            sq.StoreObject(cls);
            
            cls = new ClassIndexesString() { Name = "bbabyb" };
            sq.StoreObject(cls);

            cls = new ClassIndexesString() { Name = "bba" };
            sq.StoreObject(cls);

            cls = new ClassIndexesString() { Name = "bbazz" };
            sq.StoreObject(cls);

            cls = new ClassIndexesString() { Name = "ab" };
            sq.StoreObject(cls);

            cls = new ClassIndexesString() { Name = "rere" };
            sq.StoreObject(cls);
            cls = new ClassIndexesString() { Name = "abbb" };
            sq.StoreObject(cls);
            //sq.Close();
            //sq = new Siaqodb(objPath);
            var q = (from ClassIndexesString clss in sq
                    where clss.Name.StartsWith("bb")
                     select clss).ToList < ClassIndexesString>();

            Assert.AreEqual(3, q.Count);
            foreach (var hu in q)
            {
                Assert.IsTrue(hu.Name.StartsWith("bb"));
            }
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestListsAllTypes()
        {
            DateTime dt = new DateTime(2010, 1, 1);
            Guid guid = Guid.NewGuid();
            TimeSpan tspan = new TimeSpan();
            using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.DropType<D40WithLists>();

          
            for (int i = 0; i < 10; i++)
            {
                D40WithLists d = new D40WithLists();
                d.b = new List<byte>();d.b.Add(Convert.ToByte(i));

                d.bo = new bool[] { true, false };
                d.c = new char[]{'c','d'};
                d.d =new double[]{i,i};
                d.de = new decimal[]{i,i};
                d.dt = new List<DateTime>();d.dt.Add(dt);
                d.f = new float[]{i,i};
                d.g = new List<Guid>();d.g.Add(guid);
                d.ID = i;
                d.iu = new List<uint>(); d.iu.Add(10);
                d.l = null;
                d.s = new List<short>(); d.s.Add(1);
                d.sb = new List<sbyte>(); d.sb.Add(1);
                d.ts = new List<TimeSpan>();d.ts.Add( tspan);
                d.ul = new List<ulong>();d.ul.Add( 10);
                d.us = new List<ushort>();
                d.enn = new List<myEnum>();d.enn.Add( myEnum.unu);
                d.str = new List<string>();d.str.Add( "Abramé");
                d.textList = new List<string>(); d.textList.Add("aaaaaaaaaaaaaaaaaaaaaaaaaalllllllbbbbbbbbbbbbbbbbbbcccccccccccccccccccccccc44444444444444444444477777777777777777777777");
                sq.StoreObject(d);
            }
            IObjectList<D40WithLists> all1 = sq.LoadAll<D40WithLists>();
            int ii = 0;
            foreach (D40WithLists dL in all1)
            {
                
                Assert.AreEqual(Convert.ToByte(ii), dL.b[0]);
                Assert.AreEqual(true, dL.bo[0]);
                Assert.AreEqual(false, dL.bo[1]);
                Assert.AreEqual('c', dL.c[0]);
                Assert.AreEqual('d', dL.c[1]);
                Assert.AreEqual(ii, dL.d[1]);
                Assert.AreEqual(ii, dL.de[0]);
              
                Assert.AreEqual(dt, dL.dt[0]);
                Assert.AreEqual(ii, dL.f[0]);
                Assert.AreEqual(guid, dL.g[0]);
                Assert.AreEqual((uint)10, dL.iu[0]);
                Assert.AreEqual(null, dL.l);
                Assert.AreEqual((short)1, dL.s[0]);
                Assert.AreEqual((sbyte)1, dL.sb[0]);
                Assert.AreEqual(tspan, dL.ts[0]);
                Assert.AreEqual((ulong)10, dL.ul[0]);
                Assert.AreEqual(0, dL.us.Count);
                Assert.AreEqual(myEnum.unu, dL.enn[0]);
                Assert.AreEqual("Abramé", dL.str[0]);
                Assert.AreEqual("aaaaaaaaaaaaaaaaaaaaaaaaaalllllllbbbbbbbbbbbbbbbbbbcccccccccccccccccccccccc44444444444444444444477777777777777777777777", dL.textList[0]);
                ii++;

            }

            var q21 = (from D40WithLists dll in sq
                       where dll.g.Contains(guid)
                       select dll).ToList();

            Assert.AreEqual(10, q21.Count);
           }
            using(  Siaqodb sq = new Siaqodb(objPath)) {
            var all1 = sq.LoadAll<D40WithLists>();
            var ii = 0;
            foreach (D40WithLists dL in all1)
            {

                Assert.AreEqual(Convert.ToByte(ii), dL.b[0]);
                Assert.AreEqual(true, dL.bo[0]);
                Assert.AreEqual(false, dL.bo[1]);
                Assert.AreEqual('c', dL.c[0]);
                Assert.AreEqual('d', dL.c[1]);
                Assert.AreEqual(ii, dL.d[1]);
                Assert.AreEqual(ii, dL.de[0]);

                Assert.AreEqual(dt, dL.dt[0]);
                Assert.AreEqual(ii, dL.f[0]);
                Assert.AreEqual(guid, dL.g[0]);
                Assert.AreEqual((uint)10, dL.iu[0]);
                Assert.AreEqual(null, dL.l);
                Assert.AreEqual((short)1, dL.s[0]);
                Assert.AreEqual((sbyte)1, dL.sb[0]);
                Assert.AreEqual(tspan, dL.ts[0]);
                Assert.AreEqual((ulong)10, dL.ul[0]);
                Assert.AreEqual(0, dL.us.Count);
                Assert.AreEqual(myEnum.unu, dL.enn[0]);
                Assert.AreEqual("Abramé", dL.str[0]);
                Assert.AreEqual("aaaaaaaaaaaaaaaaaaaaaaaaaalllllllbbbbbbbbbbbbbbbbbbcccccccccccccccccccccccc44444444444444444444477777777777777777777777", dL.textList[0]);
                ii++;

            }
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestOpen2Databases()
        {
            Siaqodb s1 = new Siaqodb(@"F:\demo\s1\");
            s1.DropType<POCO>();

            for (int i = 0; i < 10; i++)
            {
                POCO pp=new POCO();
                pp.Uniq=i;
                s1.StoreObject(pp);
            }
            s1.Flush();

            Siaqodb s2 = new Siaqodb(@"F:\demo\s2\");
            
            IList<POCO> poc1 = s1.LoadAll<POCO>();

            Assert.AreEqual(10, poc1.Count);
            IList<POCO> poc2 = s2.LoadAll<POCO>();
            
            Assert.AreEqual(0, poc2.Count);
            s1.Close();
            s2.Close();
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestLoadingEvents()
        {
            //SiaqodbConfigurator.SetRaiseLoadEvents(true);
          using(  Siaqodb sq = new Siaqodb(objPath)) {
            sq.LoadingObject += new EventHandler<LoadingObjectEventArgs>(sq_LoadingObject);
            sq.LoadedObject += new EventHandler<LoadedObjectEventArgs>(sq_LoadedObject);
            sq.DropType<POCO>();
            for (int i = 0; i < 10; i++)
            {
                POCO cls = new POCO();
                cls.ID = i % 2;
                cls.MyProperty = i + 1;
                cls.Stringss = "dsdsdsds";
                cls.Uniq = i;
                sq.StoreObject(cls);
            }
            IList<POCO> all = sq.LoadAll<POCO>();
           }
        }

        void sq_LoadedObject(object sender, LoadedObjectEventArgs e)
        {
            
        }

        void sq_LoadingObject(object sender, LoadingObjectEventArgs e)
        {
            
        }

        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestNestedSelfObject()
        {
            //SiaqodbConfigurator.SetRaiseLoadEvents(true);
          using(  Siaqodb sq = new Siaqodb(objPath)) {
           
            sq.DropType<Person>();
            for (int i = 0; i < 10; i++)
            {
                Person p = new Person();
                p.Name = i.ToString();
                p.friend = new Person();
                p.friend.Name = (i + 10).ToString();
                sq.StoreObject(p);
            }
            IList<Person> all = sq.LoadAll<Person>();
            Assert.AreEqual(20, all.Count);
            int j=0;
            for (int i = 0; i < 20; i++)
            {
                if (i % 2 == 0)
                {
                    Assert.AreEqual(j.ToString(), all[i].Name);
                    Assert.AreEqual((j + 10).ToString(), all[i].friend.Name);
                    j++;
                }
                else
                {
                    Assert.IsNull(all[i].friend);
                }

            }
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void TestDateTimeKind()
        {
            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Utc);
          using(  Siaqodb sq = new Siaqodb(objPath)) {

            sq.DropType<D40>();

            D40 p = new D40();
            p.dt = DateTime.Now;
            sq.StoreObject(p);

            IList<D40> lis = sq.LoadAll<D40>();
            Assert.AreEqual(DateTimeKind.Utc, lis[0].dt.Kind);

            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Local);
            p = new D40();
            p.dt = DateTime.Now;
            sq.StoreObject(p);

            lis = sq.LoadAll<D40>();
            Assert.AreEqual(DateTimeKind.Local, lis[0].dt.Kind);
            Assert.AreEqual(DateTimeKind.Local, lis[1].dt.Kind);

            SiaqodbConfigurator.SpecifyStoredDateTimeKind(null);
            p = new D40();
            p.dt = DateTime.Now;
            sq.StoreObject(p);

            lis = sq.LoadAll<D40>();
            Assert.AreEqual(DateTimeKind.Unspecified, lis[0].dt.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, lis[1].dt.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, lis[2].dt.Kind);
           }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void ReadTransactionable()
        {
            using (Siaqodb _database = new Siaqodb(objPath))
            {
                ITransaction transaction = _database.BeginTransaction();
                try
                {
                    Customer data = new Customer();
                    data.ID = 30;
                    Customer existingData;
                    if (TryGetDocument(30, out existingData,_database))
                    {
                        _database.UpdateObjectBy(data, transaction, "ID");
                       // Notify(Updated, data);
                    }
                    else
                    {
                        _database.StoreObject(data, transaction);
                       // Notify(Added, data);
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    //Logger.Debug("Failed to store data with id {0}. Rolling back transaction. Details: {1}", id, e.Message);
                    transaction.Rollback();
                }
            }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void ReadFirstOrDefault()
        {
            using (Siaqodb _database = new Siaqodb(objPath))
            {
                //var all=_database.LoadAll<Customer>();
                var query = _database.Query<Customer>().FirstOrDefault();
                string s = "";
            }
        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void MultiThreading()
        {
            using (Siaqodb _database = new Siaqodb(objPath))
            {
                Stopwatch stopWa = new Stopwatch();
                stopWa.Start();
                System.Threading.Tasks.Task[] tasks = new System.Threading.Tasks.Task[10];
                for (int i = 0; i < 10; i++)
                {
                    tasks[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            Customer c = new Customer();
                            c.Name = "sssd";
                            _database.StoreObject(c);

                            var all = _database.LoadAll<Customer>();

                            var q = (from Customer ct in _database
                                     where ct.Name == "sssd"
                                     select ct).ToList();
                        });
                }
                System.Threading.Tasks.Task.WaitAll(tasks);
                stopWa.Stop();
                Console.WriteLine("done in: {0} seconds", stopWa.Elapsed.TotalSeconds);
                
            }
        }
        private readonly object _syncRoot = new object();
#if __MOBILE__
		[Test]
#else
        [TestMethod]

#endif
       
        public void MultiThreadingBeginTrans()
        {
            using (Siaqodb _database = new Siaqodb(objPath))
            {
                Stopwatch stopWa = new Stopwatch();
                stopWa.Start();
                System.Threading.Tasks.Task[] tasks = new System.Threading.Tasks.Task[10];
                for (int i = 0; i < 10; i++)
                {
                    tasks[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        Customer c = new Customer();
                        c.Name = "sssd";
                        lock (_syncRoot)
                        {
                            var t = _database.BeginTransaction();
                            _database.StoreObject(c,t);
                            t.Commit();
                        }
                        var all = _database.LoadAll<Customer>();

                        var q = (from Customer ct in _database
                                 where ct.Name == "sssd"
                                 select ct).ToList();
                    });
                }
                System.Threading.Tasks.Task.WaitAll(tasks);
                stopWa.Stop();
                Console.WriteLine("done in: {0} seconds", stopWa.Elapsed.TotalSeconds);

            }
        }
#if __MOBILE__
		[Test]
#else
        [TestMethod]

		#endif
        public void MultiThreadingComplexObj()
        {
            using (Siaqodb _database = new Siaqodb(objPath))
            {
                Stopwatch stopWa = new Stopwatch();
                stopWa.Start();
                System.Threading.Tasks.Task[] tasks = new System.Threading.Tasks.Task[10];
                for (int i = 0; i < 10; i++)
                {
                    tasks[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        Person c = new Person();
                        c.Name = "sssd";
                        c.friend = new Person();
                        c.friend.Name = "aass";

                        _database.StoreObject(c);

                        var all = _database.LoadAll<Person>();

                        var q = (from Person ct in _database
                                 where ct.Name == "sssd"
                                 select ct).ToList();
                    });
                }
                System.Threading.Tasks.Task.WaitAll(tasks);
                stopWa.Stop();
                Console.WriteLine("done in: {0} seconds", stopWa.Elapsed.TotalSeconds);

            }
        }
         #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
         public void TestLazyLoad()
         {
             using (Siaqodb _database = new Siaqodb(objPath))
             {
                 _database.DropType<ClsWithLazyLoadFields>();

                 for (int i = 0; i < 10; i++)
                 {
                     ClsWithLazyLoadFields cls = new ClsWithLazyLoadFields();
                     cls.MyName = "mmm"+i;
                     cls.MyPerson = new Person();
                     cls.MyPerson.Name = "firstPers"+i;
                     cls.MyPerson.friend = new Person();
                     cls.MyPerson.friend.Name = "friend" + i;

                     _database.StoreObject(cls);
                 }
                 var q = (from ClsWithLazyLoadFields c in _database
                          where c.MyName.StartsWith("mmm")
                          select c).ToList();
                 foreach (var item in q)
                     Assert.IsNull(item.MyPerson);

                 var q2 = (from ClsWithLazyLoadFields c in _database
                          where c.MyName.StartsWith("mmm")
                          select c).Include("MyPerson").ToList();
                 foreach (var item in q2)
                 {
                     Assert.IsNotNull(item.MyPerson);
                     Assert.IsNotNull(item.MyPerson.friend);
                     Assert.IsNull(item.MyPerson.friend.friend);
                 }
             }
         }
#if __MOBILE__
		[Test]
#else
        [TestMethod]

#endif
        public void TestRollback()
        {
           using (Siaqodb _database = new Siaqodb(objPath))
            {
                _database.DropType<Person>();

                ITransaction transaction = _database.BeginTransaction();

                for (var i = 0; i < 10; i++)
                    _database.StoreObject(new Person(), transaction);

                transaction.Rollback();

                int couunt= _database.Query<Person>().ToList().Count();
                Assert.AreEqual(0, couunt);
            }
        }

        public bool TryGetDocument(int id, out Customer data, Siaqodb _database)
        {
            var document = from Customer identifiable in _database
                           where identifiable.ID == id
                           select identifiable;
            if (document.SqoAny())
            {
               // Debug.Assert(document.Count() == 1, String.Format("Expected 1 instance, but got {0}.", document.Count()));

                data = document.SqoFirst();
                return true;
            }
            data = null;
            return false;
        }
#if __MOBILE__
		[Test]
#else
        [TestMethod]

#endif
        public void TestPrivateCtor()
        {
            using (Siaqodb sq = new Siaqodb(objPath))
            {
                sq.DropType<MyPrivate>();
                for (int i = 0; i < 5; i++)
                {
                    MyPrivate cls = new MyPrivate("AA" + i);
                   
                    sq.StoreObject(cls);
                }



                Assert.AreEqual(5, sq.LoadAll<MyPrivate>().Count);
            }
        }
                /* TODO LMDB uncomment
                 #if __MOBILE__
                [Test]
                #else
                [TestMethod]

                #endif
                public void TestShrink()
                {
                    DateTime dt = new DateTime(2010, 1, 1);
                    Guid guid = Guid.NewGuid();
                    TimeSpan tspan = new TimeSpan();
                    using(  Siaqodb sq = new Siaqodb(objPath)) {
                    sq.DropType<D40WithLists>();


                    for (int i = 0; i < 10; i++)
                    {
                        D40WithLists d = new D40WithLists();
                        d.b = new List<byte>(); d.b.Add(Convert.ToByte(i));

                        d.bo = new bool[] { true, false };
                        d.c = new char[] { 'c', 'd' };
                        d.d = new double[] { i, i };
                        d.de = new decimal[] { i, i };
                        d.dt = new List<DateTime>(); d.dt.Add(dt);
                        d.f = new float[] { i, i };
                        d.g = new List<Guid>(); d.g.Add(guid);
                        d.ID = i;
                        d.iu = new List<uint>(); d.iu.Add(10);
                        d.l = null;
                        d.s = new List<short>(); d.s.Add(1);
                        d.sb = new List<sbyte>(); d.sb.Add(1);
                        d.ts = new List<TimeSpan>(); d.ts.Add(tspan);
                        d.ul = new List<ulong>(); d.ul.Add(10);
                        d.us = new List<ushort>();
                        d.enn = new List<myEnum>(); d.enn.Add(myEnum.unu);
                        d.str = new List<string>(); d.str.Add("Abramé");

                        sq.StoreObject(d);
                    }

                    IObjectList<D40WithLists> all = sq.LoadAll<D40WithLists>();
                    for (int i = 5; i < 10; i++)
                    {
                        sq.Delete(all[i]);
                    }
                   }

                    SiaqodbUtil.Shrink(objPath, ShrinkType.Normal);
                    SiaqodbUtil.Shrink(objPath, ShrinkType.ForceClaimSpace);

                    using (Siaqodb sq = new Siaqodb(objPath))
                    {
                    for (int i = 0; i < 10; i++)
                    {
                        D40WithLists d = new D40WithLists();
                        d.b = new List<byte>(); d.b.Add(Convert.ToByte(i));

                        d.bo = new bool[] { true, false };
                        d.c = new char[] { 'c', 'd' };
                        d.d = new double[] { i, i };
                        d.de = new decimal[] { i, i };
                        d.dt = new List<DateTime>(); d.dt.Add(dt);
                        d.f = new float[] { i, i };
                        d.g = new List<Guid>(); d.g.Add(guid);
                        d.ID = i;
                        d.iu = new List<uint>(); d.iu.Add(10);
                        d.l = null;
                        d.s = new List<short>(); d.s.Add(1);
                        d.sb = new List<sbyte>(); d.sb.Add(1);
                        d.ts = new List<TimeSpan>(); d.ts.Add(tspan);
                        d.ul = new List<ulong>(); d.ul.Add(10);
                        d.us = new List<ushort>();
                        d.enn = new List<myEnum>(); d.enn.Add(myEnum.unu);
                        d.str = new List<string>(); d.str.Add("Abramé");

                        sq.StoreObject(d);
                    }
                    IObjectList<D40WithLists> all1 = sq.LoadAll<D40WithLists>();


                    int ii = 0;
                    bool firstTime = false;
                    foreach (D40WithLists dL in all1)
                    {
                        if (ii == 5 && !firstTime)
                        {
                            ii = 0;
                            firstTime = true;
                        }
                        Assert.AreEqual(Convert.ToByte(ii), dL.b[0]);
                        Assert.AreEqual(true, dL.bo[0]);
                        Assert.AreEqual(false, dL.bo[1]);
                        Assert.AreEqual('c', dL.c[0]);
                        Assert.AreEqual('d', dL.c[1]);
                        Assert.AreEqual(ii, dL.d[1]);
                        Assert.AreEqual(ii, dL.de[0]);

                        Assert.AreEqual(dt, dL.dt[0]);
                        Assert.AreEqual(ii, dL.f[0]);
                        Assert.AreEqual(guid, dL.g[0]);
                        Assert.AreEqual((uint)10, dL.iu[0]);
                        Assert.AreEqual(null, dL.l);
                        Assert.AreEqual((short)1, dL.s[0]);
                        Assert.AreEqual((sbyte)1, dL.sb[0]);
                        Assert.AreEqual(tspan, dL.ts[0]);
                        Assert.AreEqual((ulong)10, dL.ul[0]);
                        Assert.AreEqual(0, dL.us.Count);
                        Assert.AreEqual(myEnum.unu, dL.enn[0]);
                        Assert.AreEqual("Abramé", dL.str[0]);

                        ii++;

                    }

                    var q21 = (from D40WithLists dll in sq
                               where dll.g.Contains(guid)
                               select dll).ToList();

                    Assert.AreEqual(15, q21.Count);
                   }

                }

                #if __MOBILE__
                [Test]
                #else
                [TestMethod]

                #endif
                public void TestIndexShrink()
                {
                  using(  Siaqodb sq = new Siaqodb(objPath)) {
                    sq.DropType<D40WithIndexes>();

                    DateTime dt = new DateTime(2010, 1, 1);
                    Guid guid = Guid.NewGuid();
                    TimeSpan tspan = new TimeSpan();
                    for (int i = 0; i < 10; i++)
                    {
                        D40WithIndexes d = new D40WithIndexes();
                        d.b = Convert.ToByte(i);

                        d.bo = true;
                        d.c = 'c';
                        d.d = i;
                        d.de = i;
                        d.dt = dt;
                        d.f = i;
                        d.g = guid;
                        d.ID = i;
                        d.iu = 10;
                        d.l = i;
                        d.s = 1;
                        d.sb = 1;
                        d.ts = tspan;
                        d.ul = 10;
                        d.us = 1;
                        d.enn = myEnum.unu;
                        d.str = "Abramé";


                        sq.StoreObject(d);
                    }
                    sq.DropType<ClassIndexes>();
                    for (int i = 0; i < 100; i++)
                    {
                        ClassIndexes cls = new ClassIndexes();
                        cls.one = i % 10;
                        cls.two = i % 10 + 1;
                        cls.ID = i;
                        cls.ID2 = i;
                        sq.StoreObject(cls);
                    }
                    IList<D40WithIndexes> all30 = sq.LoadAll<D40WithIndexes>();
                    for (int i = 5; i < 10; i++)
                    {
                        sq.Delete(all30[i]);  
                    }
                   }

                    SiaqodbUtil.Shrink(objPath, ShrinkType.Normal);
                    SiaqodbUtil.Shrink(objPath, ShrinkType.ForceClaimSpace);

                    sq = new Siaqodb(objPath);
                    byte byt = 3;
                    var q1 = from D40WithIndexes di in sq
                             where di.b == byt
                             select di;

                    Assert.AreEqual(1, q1.ToList().Count);

                    var q2 = from D40WithIndexes di in sq
                             where di.bo == true
                             select di;

                    Assert.AreEqual(5, q2.ToList().Count);

                    var q3 = from D40WithIndexes di in sq
                             where di.c == 'c'
                             select di;

                    Assert.AreEqual(5, q3.ToList().Count);

                    var q4 = from D40WithIndexes di in sq
                             where di.d == 3
                             select di;

                    Assert.AreEqual(1, q4.ToList().Count);

                    var q5 = from D40WithIndexes di in sq
                             where di.de == 3
                             select di;

                    Assert.AreEqual(1, q5.ToList().Count);

                    var q6 = from D40WithIndexes di in sq
                             where di.dt == dt
                             select di;

                    Assert.AreEqual(5, q6.ToList().Count);

                    var q7 = from D40WithIndexes di in sq
                             where di.enn == myEnum.unu
                             select di;

                    Assert.AreEqual(5, q7.ToList().Count);

                    var q8 = from D40WithIndexes di in sq
                             where di.f == 3
                             select di;

                    Assert.AreEqual(1, q8.ToList().Count);

                    var q9 = from D40WithIndexes di in sq
                             where di.g == guid
                             select di;

                    Assert.AreEqual(5, q9.ToList().Count);

                    var q10 = from D40WithIndexes di in sq
                              where di.iu == 10
                              select di;

                    Assert.AreEqual(5, q10.ToList().Count);

                    var q11 = from D40WithIndexes di in sq
                              where di.l == 2
                              select di;

                    Assert.AreEqual(1, q11.ToList().Count);

                    var q12 = from D40WithIndexes di in sq
                              where di.s == 1
                              select di;

                    Assert.AreEqual(5, q12.ToList().Count);

                    var q13 = from D40WithIndexes di in sq
                              where di.sb == 1
                              select di;

                    Assert.AreEqual(5, q13.ToList().Count);

                    var q14 = from D40WithIndexes di in sq
                              where di.str.StartsWith("Abr")
                              select di;

                    Assert.AreEqual(5, q14.ToList().Count);

                    var q15 = from D40WithIndexes di in sq
                              where di.ts == tspan
                              select di;

                    Assert.AreEqual(5, q15.ToList().Count);

                    var q16 = from D40WithIndexes di in sq
                              where di.ul == 10
                              select di;

                    Assert.AreEqual(5, q16.ToList().Count);

                    var q17 = from D40WithIndexes di in sq
                              where di.us == 1
                              select di;

                    Assert.AreEqual(5, q17.ToList().Count);

                    var q18 = from ClassIndexes clss in sq
                              where clss.two == 7
                              select clss;

                    Assert.AreEqual(10, q18.ToList().Count);

                    var q19 = from D40WithIndexes di in sq
                              where di.Text == "text longgg"
                              select di;

                    Assert.AreEqual(5, q19.ToList().Count);
                   }
                }

                #if __MOBILE__
                [Test]
                #else
                [TestMethod]

                #endif
                public void TestInsertBufferChunk()
                {
                    SiaqodbConfigurator.BufferingChunkPercent = 2;
                    Siaqodb nop = new Siaqodb(objPath);
                    nop.DropType<Customer>();

                    for (int i = 0; i < 10000; i++)
                    {
                        Customer c = new Customer();
                        c.ID = i;
                        c.Name = "NOR" + i.ToString();

                        nop.StoreObject(c);
                    }
                    nop.Flush();
                    IObjectList<Customer> listC = nop.LoadAll<Customer>();
                    Assert.AreEqual(listC.Count, 10000);

                    nop.Close();

                }*/
            }
    public class RealPOCO
    {
        public int ID { get; set; }
        public int Test;

        public Guid UID { get; set; }

        

        public int IgnoredProp { get; set; }
        public int ignoredField;
        
        
        public string MyStr { get; set; }
        public string mystr;

        public string MyStrProp 
        {
            get
            {
                Console.WriteLine("dsds");
                if (1 == 2)
                {
                    return null;
                }
                return mystr;
            }
            set
            {
                Console.WriteLine("dsds");
                if (1 == 2)
                {
                    mystr = "d";
                }
                mystr = value;
            }
        }
        private ulong tickCount;
    }
    public class RealPOCO1
    {
        public int ID { get; set; }
        public int Test;

        public Guid UID { get; set; }

       
        public int IgnoredProp { get; set; }
        public int ignoredField;


        public string MyStr { get; set; }
        public string mystr;

        public string MyStrProp
        {
            get
            {
                Console.WriteLine("dsds");
                if (1 == 2)
                {
                    return null;
                }
                return mystr;
            }
            set
            {
                Console.WriteLine("dsds");
                if (1 == 2)
                {
                    mystr = "d";
                }
                mystr = value;
            }
        }
        private ulong tickCount;
    }
    public class POCO
    {
        [Index]
        public int ID { get; set; }
        [Sqo.Attributes.Ignore]
        public int MyProperty { get; set; }
        [UniqueConstraint]
        public int Uniq { get; set; }
        [MaxLength(3)]
        public string Stringss { get; set; }

        public int Test;
        
       
        private ulong tickCount;
    }
    public class ClassWithPropertiesAtt : SqoDataObject
    {
        [Index]
        public int ID { get; set; }
        [Sqo.Attributes.Ignore]
        public int MyProperty { get; set; }
        [UniqueConstraint]
        public int Uniq { get; set; }
        [MaxLength(3)]
        public string Stringss { get; set; }
        private ulong tickCount;
    }
    public class ClassIndexesString 
    {
       
        [Index]
        public string Name;
    }
    public class ClassIndexes 
    {
        [Index]
        public int one;
        
        [Index]
        public int two;
        [Index]
        public int ID;
        
        public int ID2;
        
        private ulong tickCount;
    }
    public class ClassWithEvents 
    {
        public int one;
        public event EventHandler<EventArgs> MyCustomEvent;
        public MyDelegate myDelegateMember;

        public delegate void MyDelegate();

        
        private ulong tickCount;

    }
    public class ItemUnique 
    {
        [Sqo.Attributes.UniqueConstraint]
        public int Age;
        
        [UniqueConstraint]
        public string S;

        public int integ;
        private int oid;
       
        private ulong tickCount;
    }
	public class Employee 
	{
		[MaxLength(20)]
		public string Name;
		public int ID;
		public int CustomerID;
		public string ENameProp { get { return Name; } }

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;
	}
    public class EmployeeLite 
    {
        [MaxLength(20)]
        public string Name;
        public int ID;
        public int CustomerID;
        public TestEnum EmpEnum;

        
        private ulong tickCount;
    }
    public class MyPrivate
    {
        private MyPrivate()
        {

        }
        private string a;
        public MyPrivate(string A)
        {
            this.a = A;
        }
    }
	public class Customer :SqoDataObject
	{
      
		[Index]
        public int ID;
		[UseVariable("ID")]
		public int IDProp { get { return ID; } }
		
		public int IDPropWithoutAtt { get { return ID; } }

		[UseVariable("IDs")]
        public int IDPropWithNonExistingVar { get { if (ID > 9) return 1; else return -1; } }
		[MaxLength(20)]
		public string Name;
		public bool IsTrue(string s)
		{
			return s=="ADH3";
		}
		public string stringWithoutAtt;

       
        private ulong tickCount;
		
	}
    public class CustomerLite 
    {
        public string Name { get; set; }
        public int Age { get; set; }
        bool active = true;
        public bool Active { get { return active; } set { active = value; } }
        public TestEnum TEnum { get; set; }
        private ulong tickCount;

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
    }
    public enum TestEnum { Unu,Doi,Trei}
	public class Order 
	{
		[MaxLength(20)]
		public string Name;
		public int ID;
		public int EmployeeID;

       
        private ulong tickCount;
	}
	public class EmpCust
	{
		public string CName;
		public string EName;
        private ulong tickCount;


		
	}
	public class Something
	{
		public int one;
		public int two;
        private ulong tickCount;

	}
	public class Something32
	{
		public int one;
		
		public long three;
        public int three1;
        public int three2;

        
        private ulong tickCount;
      

	}
	public class EmpCustOID
	{
		public string CName;
		public string EName;
		public int EOID;
        private ulong tickCount;
	}
	public class D40 
	{
		public D40()
		{

		}
		public int ID;
		public int i;
		public uint iu;
		
		public short s;
		[MaxLength(20)]
		public string str = "test";
		public ushort us;
		public byte b;
		public sbyte sb;
		public long l;
		public ulong ul;
		public float f;
		public double d;
		public decimal de;
		public char c;
		public bool bo;
		public TimeSpan ts;
        public DateTimeOffset dtsofs;
		public DateTime dt;
		public Guid g;
		public myEnum enn = myEnum.doi;
        [Text]
        public string Text;
        
        private ulong tickCount;
	}
    public class D40Nullable 
    {
        public D40Nullable()
        {

        }
        public int? ID;
        public int? i;
        public uint? iu;

        public short? s;
        [MaxLength(20)]
        public string str = "test";
        public ushort? us;
        public byte? b;
        public sbyte? sb;
        public long? l;
        public ulong? ul;
        public float? f;
        public double? d;
        public decimal? de;
        public char? c;
        public bool? bo;
        public TimeSpan? ts;
        public DateTimeOffset? dtsofs;
        public DateTime? dt;
        public Guid? g;
        public myEnum enn = myEnum.doi;
        [Text]
        public string Text;
        
        private ulong tickCount;
    }
	public enum myEnum { unu = 2, doi };

    public class ForConcurencyClass
    {
        public int integ;
        public string test;
        private ulong tickCount;

        
    
    }
    public class D40WithIndexes 
    {
        public D40WithIndexes()
        {

        }
        public int ID;
        [Index]
        public int i;
        [Index]
        public uint iu;
        [Index]
        public short s;
        [MaxLength(20)]
        [Index]
        public string str = "test";
        [Index]
        public ushort us;
        [Index]
        public byte b;
        [Index]
        public sbyte sb;
        [Index]
        public long l;
        [Index]
        public ulong ul;
        [Index]
        public float f;
        [Index]
        public double d;
        [Index]
        public decimal de;
        [Index]
        public char c;
        [Index]
        public bool bo;
        [Index]
        public TimeSpan ts;
        [Index]
        public DateTime dt;
        [Index]
        public Guid g;
        [Index]
        public myEnum enn = myEnum.doi;

        [Text]
        [Index]
        public string Text = "text longgg";

       
        private ulong tickCount;
    }
    
    public class D40WithLists 
    {
        public D40WithLists()
        {

        }
        public int ID;
        
        public List<int> i;
        
        public List<uint> iu;
        
        public List<short> s;
        [MaxLength(20)]
        public List<string> str;
        [Text]
        public List<string> textList;

        public List<ushort> us;

        public List<byte> b;
        
        public List<sbyte> sb;
        
        public List<long> l;
        
        public List<ulong> ul;
        
        public float[] f;
        
        public double[] d;
        
        public decimal[] de;

        public char[] c;
        
        public bool[] bo;
        
        public List<TimeSpan> ts;
       
        public List<DateTime> dt;
        
        public List<Guid> g;
        
        public List<myEnum> enn ;

        
        private ulong tickCount;

       
    }
    public class Person
    {
        
        public string Name;

        public Person friend;

    }
    public class ClsWithLazyLoadFields
    {
        [LazyLoad]
        public Person MyPerson { get; set; }
        public string MyName { get; set; }
    }
}
