﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Sqo;
using Sqo.Attributes;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo.Exceptions;
using System.Xml;
using System.IO;
using Sqo.Transactions;
using System.IO.IsolatedStorage;

namespace TestSiaqodbSilver
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class BasicsTest
	{
		string objPath=@"nopdbh212";
		public BasicsTest()
		{
            SiaqodbConfigurator.EncryptedDatabase = false;
              Sqo.SiaqodbConfigurator.SetLicense(@" qU3TtvA4T4L30VSlCCGUTSgbmx5WI47jJrL1WHN2o/gg5hnL45waY5nSxqWiFmnG");
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                long newSpace = isf.Quota + 100000;
                try
                {
                    if (true == isf.IncreaseQuotaTo(newSpace))
                    {
                       // Results.Text = "Quota successfully increased.";
                    }
                    else
                    {
                        //Results.Text = "Quota increase was unsuccessfull.";
                    }
                }
                catch (Exception e)
                {
                   // Results.Text = "An error occured: " + e.Message;
                }
                //SetStorageData();
            }
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

		[TestMethod]
		public void TestInsert()
		{
			Siaqodb nop = new Siaqodb(objPath);
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

			
			
		}
		[TestMethod]
		public void TestStringWithoutAttribute()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestMassInsert()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			for (int i = 0; i < 1000; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				c.Name = "ADH" + i.ToString();

				nop.StoreObject(c);
			}
			nop.Flush();
			
		}
        [TestMethod]
        public void TestInsertAllTypeOfFields()
        {
            Siaqodb nop = new Siaqodb(objPath);
            nop.DropType<D40>();
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
            d.enn = myEnum.unu;
            d.str = "Abramé";

            Guid g = d.g;
            TimeSpan ts = d.ts;
            DateTime dt = d.dt;
            nop.StoreObject(d);
            nop.Flush();

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



            }

            nop.Close();
            nop = new Siaqodb(objPath);
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


            }

        }
		[TestMethod]
		public void TestUpdate()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
			nop.Close();
			nop = new Siaqodb(objPath);
			IObjectList<Customer> listCUpdate = nop.LoadAll<Customer>();
			Assert.AreEqual("UPDATEWORK", listCUpdate[0].Name);



		}
		[TestMethod]
		public void TestUpdateCheckNrRecords()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
			nop.Close();
			nop = new Siaqodb(objPath);
			IObjectList<Customer> listCUpdate = nop.LoadAll<Customer>();
			Assert.AreEqual("UPDATEWORK", listCUpdate[0].Name);
			Assert.AreEqual(10, listCUpdate.Count);



		}
		[TestMethod]
		public void TestUpdateLoadTypesForPrivateFields()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
			nop.Close();
			nop = new Siaqodb(objPath);
			IObjectList<Customer> listCUpdate = nop.LoadAll<Customer>();
			Assert.AreEqual("UPDATEWORK", listCUpdate[0].Name);
			Assert.AreEqual(10, listCUpdate.Count);



		}
		[TestMethod]
		public void TestInsertAfterDrop()
		{
			Siaqodb nop = new Siaqodb(objPath);
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

			nop.Close();
			nop = new Siaqodb(objPath);
			IObjectList<Customer> listCUpdate = nop.LoadAll<Customer>();
			Assert.AreEqual(1, listCUpdate.Count);



		}
		[TestMethod]
		public void TestSavingEvent()
		{
			nrSaves = 0;
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestSavedEvent()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestDelete()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestDeleteEvents()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
        //removed for safety reason
        //[TestMethod]
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
        //    nop.DeleteByOID<Customer>(listC[1].OID);

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
        [TestMethod]
        public void TestCount()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
            nop.Flush();
            int count = nop.Count<Customer>();
            Assert.AreEqual(160, listC.Count);
            Assert.AreEqual(159, count);
        }
        [TestMethod]
        [ExpectedException(typeof(SiaqodbException))]
        public void TestSaveDeletedObject()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
        [TestMethod]
        [ExpectedException(typeof(SiaqodbException))]
        public void TestDeleteUnSavedObject()
        {
            Siaqodb nop = new Siaqodb(objPath);
            nop.DropType<Customer>();

            Customer cu = new Customer();
            cu.ID = 78;
            nop.Delete(cu);
        }
        [TestMethod]
        public void TestXMLExportImport()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<Customer>();
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();
                
                sq.StoreObject(c);
            }
            sq.Flush();
            IObjectList<Customer> cust = sq.LoadAll<Customer>();
            StringBuilder sb=new StringBuilder();
            XmlWriter xmlSer = XmlWriter.Create(sb);
            sq.ExportToXML<Customer>(xmlSer);
            xmlSer.Close();

            XmlReader xmlSerRea = XmlReader.Create(new StringReader(sb.ToString()));
            IObjectList<Customer> l= sq.ImportFromXML<Customer>(xmlSerRea);
            xmlSerRea.Close();
            Assert.AreEqual(10, l.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(cust[i].ID, l[i].ID);
            }
        }
        [TestMethod]
        public void TestXMLExportImportCompleteType()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
        [TestMethod]
        public void TestLoadMetaTypes()
        {
            Siaqodb nop = new Siaqodb(objPath);
            nop.DropType<Customer>();

            Customer cu = new Customer();
            cu.ID = 78;
            nop.StoreObject(cu);
            List<MetaType> list= nop.GetAllTypes();
            Assert.IsTrue(list.Count >= 1);
        }
        [TestMethod]
        public void TestCopyObject()
        {
            Siaqodb nop = new Siaqodb(objPath);
            nop.DropType<Item>();

            for (int i = 0; i < 10; i++)
            {
                Item c = new Item();
                c.ID = i;
                c.MyStr = "ADH" + i.ToString();

                nop.StoreObject(c);
            }
            nop.Flush();

            IObjectList<Item> listC = nop.LoadAll<Item>();
            Item co = new Item();
            CopyObject(co, listC[1]);


        }
        private void CopyObject(SqoDataObject objReciever, SqoDataObject objSender)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;

            System.Reflection.PropertyInfo[] props= objReciever.GetType().GetProperties(flags);
            foreach (System.Reflection.PropertyInfo pi in props)
            {
                if (pi.Name == "OID")
                {
                    continue;
                }
                pi.SetValue(objReciever,pi.GetValue(objSender,null),null);
            }

        }
        [TestMethod]
        [ExpectedException(typeof(UniqueConstraintException))]
        public void TestUniqueExceptionInsert()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
        [TestMethod]
        [ExpectedException(typeof(UniqueConstraintException))]
        public void TestUniqueExceptionInsertTransaction()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
            sq.StoreObject(c1, tr);
            tr.Commit();



        }
        [TestMethod]
        [ExpectedException(typeof(UniqueConstraintException))]
        public void TestUniqueExceptionUpdate()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
        [TestMethod]
        public void TestUpdateObjectBy()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
            it.S = "someNew";
            bool stored = sq.UpdateObjectBy("Age", it);
            Assert.IsTrue(stored);

            IObjectList<ItemUnique> list = sq.LoadAll<ItemUnique>();

            Assert.AreEqual("someNew", list[1].S);


            it = new ItemUnique();
            it.Age = 13;
            it.S = "someNew";
            stored = sq.UpdateObjectBy("Age", it);
            Assert.IsFalse(stored);

        }
        [TestMethod]
        [ExpectedException(typeof(SiaqodbException))]
        public void TestUpdateObjectByDuplicates()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
        [TestMethod]
        [ExpectedException(typeof(SiaqodbException))]
        public void TestUpdateObjectByFieldNotExists()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;

            sq.StoreObject(emp);

            sq.UpdateObjectBy("IDhh", emp);


        }
        [TestMethod]
        public void TestEventsVariable()
        {
            Siaqodb sq = new Siaqodb(objPath);
            //sq.DropType<ClassWithEvents>();

            ClassWithEvents c = new ClassWithEvents();
            c.one = 10;


            sq.StoreObject(c);
            IObjectList<ClassWithEvents> ll = sq.LoadAll<ClassWithEvents>();




        }
        [TestMethod]
        public void TestIndexFirstInsert()
        {
            Siaqodb sq = new Siaqodb(objPath);
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


            q = from ClassIndexes clss in sq
                where clss.two == 10
                select clss;


            Assert.AreEqual(10, q.Count<ClassIndexes>());


        }
        [TestMethod]
        public void TestIndexUpdate()
        {
            Siaqodb sq = new Siaqodb(objPath);
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


            q.ToList<ClassIndexes>()[0].one = 5;

            sq.StoreObject(q.ToList<ClassIndexes>()[0]);

            sq.StoreObject(q.ToList<ClassIndexes>()[1]);//just update nothing change

            q = from ClassIndexes clss in sq
                where clss.one == 9
                select clss;


            Assert.AreEqual(9, q.Count<ClassIndexes>());

            q = from ClassIndexes clss in sq
                where clss.one == 5
                select clss;


            Assert.AreEqual(11, q.Count<ClassIndexes>());
        }
        [TestMethod]
        public void TestIndexSaveAndClose()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                sq.StoreObject(cls);
            }

            sq = new Siaqodb(objPath);
            var q = from ClassIndexes clss in sq
                    where clss.one == 9
                    select clss;


            Assert.AreEqual(10, q.Count<ClassIndexes>());
        }
        [TestMethod]
        public void TestIndexAllOperations()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                sq.StoreObject(cls);
            }

            sq = new Siaqodb(objPath);
            var q = from ClassIndexes clss in sq
                    where clss.one <= 2
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
        [TestMethod]
        public void TestIndexUpdateObjectBy()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
        [TestMethod]
        public void TestIndexDelete()
        {
            Siaqodb sq = new Siaqodb(objPath);
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


            var q = from ClassIndexes clss in sq
                    where clss.two == 7
                    select clss;


            sq.Delete(q.ToList<ClassIndexes>()[0]);

            q = from ClassIndexes clss in sq
                where clss.two == 7
                select clss;

            Assert.AreEqual(9, q.Count<ClassIndexes>());

            sq.DeleteObjectBy("ID", q.ToList<ClassIndexes>()[0]);

            q = from ClassIndexes clss in sq
                where clss.two == 7
                select clss;

            Assert.AreEqual(8, q.Count<ClassIndexes>());


            sq.DeleteObjectBy("ID2", q.ToList<ClassIndexes>()[0]);

            q = from ClassIndexes clss in sq
                where clss.two == 7
                select clss;

            Assert.AreEqual(7, q.Count<ClassIndexes>());
        }
        [TestMethod]
        public void TestIndexAllFieldTypes()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
            sq.Close();
            sq = new Siaqodb(objPath);
            byte byt = 5;
            var q1 = from D40WithIndexes di in sq
                     where di.b == byt
                     select di;

            Assert.AreEqual(1, q1.ToList().Count);

            var q2 = from D40WithIndexes di in sq
                     where di.bo == true
                     select di;

            Assert.AreEqual(10, q2.ToList().Count);

            var q3 = from D40WithIndexes di in sq
                     where di.c == 'c'
                     select di;

            Assert.AreEqual(10, q3.ToList().Count);

            var q4 = from D40WithIndexes di in sq
                     where di.d == 5
                     select di;

            Assert.AreEqual(1, q4.ToList().Count);

            var q5 = from D40WithIndexes di in sq
                     where di.de == 5
                     select di;

            Assert.AreEqual(1, q5.ToList().Count);

            var q6 = from D40WithIndexes di in sq
                     where di.dt == dt
                     select di;

            Assert.AreEqual(10, q6.ToList().Count);

            var q7 = from D40WithIndexes di in sq
                     where di.enn == myEnum.unu
                     select di;

            Assert.AreEqual(10, q7.ToList().Count);

            var q8 = from D40WithIndexes di in sq
                     where di.f == 6
                     select di;

            Assert.AreEqual(1, q8.ToList().Count);

            var q9 = from D40WithIndexes di in sq
                     where di.g == guid
                     select di;

            Assert.AreEqual(10, q9.ToList().Count);

            var q10 = from D40WithIndexes di in sq
                      where di.iu == 10
                      select di;

            Assert.AreEqual(10, q10.ToList().Count);

            var q11 = from D40WithIndexes di in sq
                      where di.l == 7
                      select di;

            Assert.AreEqual(1, q11.ToList().Count);

            var q12 = from D40WithIndexes di in sq
                      where di.s == 1
                      select di;

            Assert.AreEqual(10, q12.ToList().Count);

            var q13 = from D40WithIndexes di in sq
                      where di.sb == 1
                      select di;

            Assert.AreEqual(10, q13.ToList().Count);

            var q14 = from D40WithIndexes di in sq
                      where di.str.StartsWith("Abr")
                      select di;

            Assert.AreEqual(10, q14.ToList().Count);

            var q15 = from D40WithIndexes di in sq
                      where di.ts == tspan
                      select di;

            Assert.AreEqual(10, q15.ToList().Count);

            var q16 = from D40WithIndexes di in sq
                      where di.ul == 10
                      select di;

            Assert.AreEqual(10, q16.ToList().Count);

            var q17 = from D40WithIndexes di in sq
                      where di.us == 1
                      select di;

            Assert.AreEqual(10, q17.ToList().Count);

            var q18 = from ClassIndexes clss in sq
                      where clss.two == 7
                      select clss;

            Assert.AreEqual(10, q18.ToList().Count);
        }
        [TestMethod]
        public void TestAttributesOnProps()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<ClassWithPropertiesAtt>();
            for (int i = 0; i < 10; i++)
            {
                ClassWithPropertiesAtt cls = new ClassWithPropertiesAtt();
                cls.ID = i % 2;
                cls.MyProperty = i + 1;
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
        [TestMethod]
        public void TestPOCO()
        {
            Siaqodb sq = new Siaqodb(objPath);
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

            IObjectList<POCO> pc = sq.LoadAll<POCO>();

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

        [TestMethod]
        public void TestRealPOCO()
        {
            SiaqodbConfigurator.AddIndex("ID", typeof(RealPOCO));
            SiaqodbConfigurator.AddUniqueConstraint("UID", typeof(RealPOCO));

            SiaqodbConfigurator.AddIgnore("ignoredField", typeof(RealPOCO));
            SiaqodbConfigurator.AddIgnore("IgnoredProp", typeof(RealPOCO));

            SiaqodbConfigurator.AddMaxLength("MyStr", 3, typeof(RealPOCO));
            SiaqodbConfigurator.AddMaxLength("mystr", 3, typeof(RealPOCO));

            SiaqodbConfigurator.PropertyUseField("MyStrProp", "mystr", typeof(RealPOCO));

            Siaqodb sq = new Siaqodb(objPath);
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

            sq.Close();

            sq = new Siaqodb(objPath);
            q = from RealPOCO clss in sq
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
            bool excp = false;
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

        }

        [TestMethod]
        public void TestOptimisticConcurency()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<ForConcurencyClass>();
            List<ForConcurencyClass> lis = new List<ForConcurencyClass>();
            for (int i = 0; i < 10; i++)
            {
                ForConcurencyClass c = new ForConcurencyClass();
                c.integ = i + 1;
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


            sq.DeleteObjectBy("integ", de6);


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
        [TestMethod]
        public void TestUseLargeBuffers()
        {
            //by default UseLargeBuffers is true
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<Employee>();
            for (int i = 0; i < 10; i++)
            {
                sq.StoreObject(new Employee());
            }

            sq.Close();
            SiaqodbConfigurator.UseLargeBuffers = false;
            sq = new Siaqodb(objPath);

            var q = from Employee clss in sq
                    select clss;

            Assert.AreEqual(10, q.Count<Employee>());

        }
        [TestMethod]
        public void TestTransactionInsert()
        {
            Siaqodb sq = new Siaqodb(objPath);
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

                list = sq.LoadAll<Customer>();
                Assert.AreEqual(0, list.Count);


                transact.Commit();

            }
            catch (Exception ex)
            {
                transact.Rollback();
            }
            list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);
            sq.Close();
            sq.Open(objPath);
            list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);


            transact = sq.BeginTransaction();
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
            catch (Exception ex)
            {
                transact.Rollback();
            }

            list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);



        }
        [TestMethod]
        public void TestTransactionUpdateInsert()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c);//without transact
            }

            list = sq.LoadAll<Customer>();
            Assert.AreEqual(10, list.Count);

            foreach (Customer c in list)
            {
                c.Name = "updated";
                sq.StoreObject(c, transact);
            }
            list = sq.LoadAll<Customer>();
            foreach (Customer c in list)
            {
                Assert.AreEqual("GTA", c.Name.Substring(0, 3));
            }
            try
            {
                transact.Commit();
            }
            catch (Exception ex)
            {
                transact.Rollback();//problem with OptimistiConcurency
            }
            list = sq.LoadAll<Customer>();

            foreach (Customer c in list)
            {
                Assert.AreEqual("updated", c.Name);
            }

        }
        [TestMethod]
        public void TestTransactionDelete()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c);//without transact
            }
            list = sq.LoadAll<Customer>();
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
        [TestMethod]
        public void TestUpdateObjectByManyFieldsTransaction()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
            Assert.AreEqual(list[0].Name, emp.Name);

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
        [TestMethod]
        public void TestDeleteObjectByTransactions()
        {
            Siaqodb sq = new Siaqodb(objPath);
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

            bool s = sq.DeleteObjectBy(emp, trans, "ID", "CustomerID");
            Assert.IsTrue(s);
            trans.Commit();
            IList<Employee> list = sq.LoadAll<Employee>();
            Assert.AreEqual(list.Count, 0);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            sq.StoreObject(emp);


            trans = sq.BeginTransaction();
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
        [TestMethod]
        public void TestTransactionCrash()
        {
            Siaqodb sq = new Siaqodb(objPath);

            IList<Customer> list = sq.LoadAll<Customer>();
            IList<Employee> list2 = sq.LoadAll<Employee>();

            sq.DropType<Customer>();
            sq.DropType<Employee>();

            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                sq.StoreObject(c, transact);
                Employee e = new Employee();
                e.Name = "EMP" + i.ToString();
                sq.StoreObject(e, transact);
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
                list[i].Name = "updated";
                list2[i].Name = "updatedE";
                sq.StoreObject(list[i], transac2t);
                sq.StoreObject(list2[i], transac2t);
                sq.StoreObject(new Customer(), transac2t);
                sq.StoreObject(new Employee(), transac2t);
            }


            transac2t.Commit();//here do debug and stop after a few commits to be able to simulate crash recovery
        }
        [TestMethod]
        public void TestTransactionManyTypes()
        {
            Siaqodb sq = new Siaqodb(objPath);


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
                d.dt = DateTime.Now;
                sq.StoreObject(d, transact);
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
        [TestMethod]
        public void TestTransactionLists()
        {
            Siaqodb sq = new Siaqodb(objPath);


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
        [TestMethod]
        public void TestTransactionLoop()//bug reported by Riccardo
        {
            Siaqodb _db;
            
            try
            {
                _db = new Sqo.Siaqodb("NHHMTest", Environment.SpecialFolder.MyDocuments);
                for (int i = 0; i < 5000; i++)
                {
                    RealPOCO m = new RealPOCO();
                    _db.StoreObject(m);


                    ITransaction t = _db.BeginTransaction();
                    _db.Delete(m, t);
                    t.Commit();
                }
            }
            catch (Exception ex)
            {

            }
        }

        [TestMethod]
        public void TestOpen2Databases()
        {
            Siaqodb s1 = new Siaqodb(@"s1");
            s1.DropType<POCO>();

            for (int i = 0; i < 10; i++)
            {
                POCO pp = new POCO();
                pp.Uniq = i;
                s1.StoreObject(pp);
            }
            s1.Flush();

            Siaqodb s2 = new Siaqodb(@"s2");

            IList<POCO> poc1 = s1.LoadAll<POCO>();

            Assert.AreEqual(10, poc1.Count);
            IList<POCO> poc2 = s2.LoadAll<POCO>();

            Assert.AreEqual(0, poc2.Count);

        }
        [TestMethod]
        public void TestListsAllTypes()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<D40WithLists>();

            DateTime dt = new DateTime(2010, 1, 1);
            Guid guid = Guid.NewGuid();
            TimeSpan tspan = new TimeSpan();
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
            //sq.Close();
            //sq= new Siaqodb(objPath);
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

                ii++;

            }

            var q21 = (from D40WithLists dll in sq
                       where dll.g.Contains(guid)
                       select dll).ToList();

            Assert.AreEqual(10, q21.Count);
        }
        [TestMethod]
        public void TestNestedSelfObject()
        {
            //SiaqodbConfigurator.SetRaiseLoadEvents(true);
            Siaqodb sq = new Siaqodb(objPath);

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
            int j = 0;
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
        [TestMethod]
        public void TestDateTimeKind()
        {
            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Utc);
            Siaqodb sq = new Siaqodb(objPath);

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
        [TestMethod]
        public void TestShrink()
        {
            Siaqodb sq = new Siaqodb(objPath);
            sq.DropType<D40WithLists>();

            DateTime dt = new DateTime(2010, 1, 1);
            Guid guid = Guid.NewGuid();
            TimeSpan tspan = new TimeSpan();
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
            sq.Close();

            SiaqodbUtil.Shrink(objPath, ShrinkType.Normal);
            SiaqodbUtil.Shrink(objPath, ShrinkType.ForceClaimSpace);

            sq = new Siaqodb(objPath);
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
        [TestMethod]
        public void TestIndexShrink()
        {
            Siaqodb sq = new Siaqodb(objPath);
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
            sq.Close();

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

       

           
        }
    }
    public class RealPOCO
    {
        public int ID { get; set; }
        public int Test;

        public Guid UID { get; set; }

        public int OID { get; set; }

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
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
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

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
    public class ClassWithPropertiesAtt : ISqoDataObject
    {
        [Index]
        public int ID { get; set; }
        [Sqo.Attributes.Ignore]
        public int MyProperty { get; set; }
        [UniqueConstraint]
        public int Uniq { get; set; }
        [MaxLength(3)]
        public string Stringss { get; set; }
        int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
    public class ClassIndexes : ISqoDataObject
    {
        [Index]
        public int one;

        [Index]
        public int two;
        [Index]
        public int ID;

        public int ID2;
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
      
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
    public class ClassWithEvents : ISqoDataObject
    {
        public int one;
        public event EventHandler<EventArgs> MyCustomEvent;
        public MyDelegate myDelegateMember;

        public delegate void MyDelegate();
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }

    }
    public class ItemUnique : ISqoDataObject
    {
        [UniqueConstraint]
        public int Age;

        [UniqueConstraint]
        public string S;
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
	public class Employee : ISqoDataObject
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
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
	}
    public class EmployeeLite : ISqoDataObject
    {
        [MaxLength(20)]
        public string Name;
        public int ID;
        public int CustomerID;
        public TestEnum EmpEnum;

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
	public class Customer 
	{
		[MaxLength(20)]
		public string Name;
		public int ID;
		[UseVariable("ID")]
		public int IDProp { get { return ID; } }
		
		public int IDPropWithoutAtt { get { return ID; } }

		[UseVariable("IDs")]
		public int IDPropWithNonExistingVar { get { return ID; } }
		private int privateInt=100;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
		public void SetValue(System.Reflection.FieldInfo field, object value)
		{
			
				field.SetValue(this, value);
			
		}
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
		public string stringWithoutAtt;
		private string stringWithoutAttPrivate;
        public string SchChmaged = "jujuj";

       

	}
    public class CustomerLite : SqoDataObject
    {
        public string Name { get; set; }
        public int Age { get; set; }
        bool active = true;
        [UseVariable("active")]
        public bool Active { get { return active; } set { active = value; } }
        protected override object GetValue(System.Reflection.FieldInfo field)
        {
            if (field.DeclaringType == typeof(CustomerLite))
                return field.GetValue(this);
            else
                return base.GetValue(field);
        }
        protected override void SetValue(System.Reflection.FieldInfo field, object value)
        {
            if (field.DeclaringType == typeof(CustomerLite))
                field.SetValue(this, value);
            else
                base.SetValue(field, value);
        }
        public TestEnum TEnum { get; set; }
    }
    public enum TestEnum { Unu, Doi, Trei }

	public class CustomerPrivate : ISqoDataObject
	{
		[MaxLength(20)]
		public string Name;
		private int ID;
		[UseVariable("ID")]
		public int IDProp { get { return ID; } set { ID = value; } }

		public int IDPropWithoutAtt { get { return ID; } }

		[UseVariable("IDs")]
		public int IDPropWithNonExistingVar { get { return ID; } }
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
	}
	public class CustomerAnony
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}
	public class Order : SqoDataObject
	{
		[MaxLength(20)]
		public string Name;
		public int ID;
		public int EmployeeID;
		
	}
	public class EmpCust
	{
		public string CName;
		public string EName;
		
	}
	public class EmpCust2
	{
		public string CName;
		public int EName;
		public int OID;
	}
	public class EmpCustObjects
	{
		public Customer CName;
		public Employee EName;

	}
	public class EmpCustAnonym
	{
		public string CName { get; set; }
		public string EName { get; set; }
	}
	public class Something
	{
		public int one;
		public int two;

	}
	public class SomethingAnony
	{
		public int One { get; set; }
		public int Two { get; set; }
	}
	public class EmpCustOID
	{
		public string CName;
		public string EName;
		public int EOID;
	}
	public class D40 : ISqoDataObject
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

		public DateTime dt;
		public Guid g;
		public myEnum enn = myEnum.doi;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }


	}
    public class D40WithIndexes : ISqoDataObject
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

        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;
    }
    public class D40WithLists : ISqoDataObject
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

        public List<myEnum> enn;

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
        private ulong tickCount;
    }
    public class Item : SqoDataObject
    {
        public int ID { get; set; }
        public string MyStr { get; set; }
        protected override object GetValue(System.Reflection.FieldInfo field)
        {
            if (field.DeclaringType == typeof(Item))
                return field.GetValue(this);
            else
                return base.GetValue(field);
        }
        protected override void SetValue(System.Reflection.FieldInfo field, object value)
        {
            if (field.DeclaringType == typeof(Item))
                field.SetValue(this, value);
            else
                base.SetValue(field, value);
        }
    }
	public enum myEnum { unu = 2, doi };

    public class ForConcurencyClass
    {
        public int integ;
        public string test;
        private ulong tickCount;

        public int OID { get; set; }

        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
    public class Person
    {
        public int OID { get; set; }
        public string Name;
        public Person friend;

        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }

    }


}
