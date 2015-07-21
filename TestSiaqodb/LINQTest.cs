using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Sqo;
#if __MOBILE__
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endif
namespace TestSiaqodb
{
	/// <summary>
	/// Summary description for LINQTest
	/// </summary>
	#if __MOBILE__
	[TestFixture]
	#else
	[TestClass]

	#endif
	public class LINQTest
	{
		string objPath;
		
		public LINQTest()
		{
            SiaqodbConfigurator.EncryptedDatabase = true;
            Sqo.SiaqodbConfigurator.SetLicense(@" OqNhH+uqOErNs375SRgMEXbBB0dyx7R8MAM2M4i+fwWiiS3Qv+QVT8odOEjHSkEX");
			#if __MOBILE__
			objPath=Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			#else
			objPath=@"c:\work\temp\unitTests_siaqodbLMDB\";
			#endif
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

        //TODO: add Enum in where also JOIN etc

		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestBasicQuery()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();

			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				c.Name = "ADH" + i.ToString();

				nop.StoreObject(c);
			}
			nop.Flush();
            var query = from Customer c in nop
                        select c;
			Assert.AreEqual(query.ToList<Customer>().Count,10);
            }
		}
		
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestBasicWhere()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial=new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				c.Name = "ADH" + i.ToString();
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.ID < 5
						select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 5);
			query = from Customer c in nop
						where c.ID > 5
						select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 4);
			
			query = from Customer c in nop
					where c.ID == 5
					select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 1);

			Assert.AreEqual(listInitial[5].Name,query.ToList<Customer>()[0].Name);
			Assert.AreEqual(listInitial[5].ID, query.ToList<Customer>()[0].ID);
			Assert.AreEqual(listInitial[5].OID, query.ToList<Customer>()[0].OID);
            }
		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestBasicWhereByOID()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				c.Name = "ADH" + i.ToString();
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.OID < 5
						select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 4);
			query = from Customer c in nop
					where c.OID > 5
					select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 5);

			query = from Customer c in nop
					where c.OID > 5 && c.OID<8
					select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 2);


			query = from Customer c in nop
					where c.OID == 5
					select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 1);

			Assert.AreEqual(listInitial[4].Name, query.ToList<Customer>()[0].Name);
			Assert.AreEqual(listInitial[4].ID, query.ToList<Customer>()[0].ID);
			Assert.AreEqual(listInitial[4].OID, query.ToList<Customer>()[0].OID);
            }
		}

		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestBasicWhereOperators()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				c.Name = "ADH" + i.ToString();
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.ID < 5
						select c;
			Assert.AreEqual(query.ToList<Customer>().Count, 5);
			 query = from Customer c in nop
						where c.ID >3 
						select c;
			 Assert.AreEqual(query.ToList<Customer>().Count, 6);
			 query = from Customer c in nop
					 where c.ID >= 3
					 select c;
			 Assert.AreEqual(query.ToList<Customer>().Count, 7);
			 query = from Customer c in nop
					 where c.ID <= 3
					 select c;
			 Assert.AreEqual(query.ToList<Customer>().Count, 4);

			 query = from Customer c in nop
					 where c.ID != 3
					 select c;
			 Assert.AreEqual(query.ToList<Customer>().Count, 9);
            }


		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestBasicWhereStringComparison()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.Name.Contains("ADH")
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 5);
			query = from Customer c in nop
						where c.Name.Contains("2T")
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);

			query = from Customer c in nop
						where c.Name.StartsWith("A")
						select c;
			 Assert.AreEqual(query.ToList<Customer>().Count, 5);
			 query = from Customer c in nop
					 where c.Name.StartsWith("ake")
					 select c;
			
			
			Assert.AreEqual(query.ToList<Customer>().Count, 0);
			query = from Customer c in nop
					where c.Name.EndsWith("ADH")
					select c;
			Assert.AreEqual(0, query.ToList<Customer>().Count);
			query = from Customer c in nop
					where c.Name.EndsWith("TEST")
					select c;
			Assert.AreEqual(5, query.ToList<Customer>().Count);
            }


		}
		int id=3;
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void WhereLocalVariable()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.ID == this.id
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
			Assert.AreEqual(3, query.ToList<Customer>()[0].ID);
            }
		}
		public int TestMet(int t)
		{
			return t+1;
		}
        public int TestMet2(int t)
        {
            return t + 1;
        }
        public int TestMet3(Customer t)
        {
            return t.ID;
        }
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void WhereLocalMethod()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.ID == this.TestMet(3)
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
			Assert.AreEqual(4, query.ToList<Customer>()[0].ID);

			 query = from Customer c in nop
						where c.OID == this.TestMet(3)
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
			Assert.AreEqual(4, query.ToList<Customer>()[0].OID);
            }
		}
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void WhereLocalMethodOverObject()
        {
            using(Siaqodb nop = new Siaqodb(objPath)){
            nop.DropType<Customer>();
            List<Customer> listInitial = new List<Customer>();
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                if (i % 2 == 0)
                {
                    c.Name = i.ToString() + "TEST";
                }
                else
                {
                    c.Name = "ADH" + i.ToString();
                }
                listInitial.Add(c);
                nop.StoreObject(c);
            }
            nop.Flush();
            //run unoptimized
            var query = from Customer c in nop
                        where this.TestMet2(c.ID)==3
                        select c;

            Assert.AreEqual(query.ToList<Customer>().Count, 1);
            Assert.AreEqual(2, query.ToList<Customer>()[0].ID);

            query = from Customer c in nop
                        where this.TestMet3(c) == 3
                        select c;

            Assert.AreEqual(query.ToList<Customer>().Count, 1);
            Assert.AreEqual(3, query.ToList<Customer>()[0].ID);
            }
            
        }
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void WhereAnd()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.Name.Contains("A") && c.Name.Contains("3")
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
			Assert.AreEqual(3, query.ToList<Customer>()[0].ID);

			query = from Customer c in nop
					where c.Name.Contains("A") && (c.Name.Contains("3") && c.ID==3)
					select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
			Assert.AreEqual(3, query.ToList<Customer>()[0].ID);
            }

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void SimpleSelect()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.Name.Contains("A") && c.Name.Contains("3")
						select new { Name=c.Name, Som=c.ID};
			int s=0;
			foreach( var a in query)
			{
				s++;
			}
			Assert.AreEqual(1,s);

			}

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void WhereOR()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.Name.Contains("A") || c.ID==2
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 6);
			

			query = from Customer c in nop
					where c.Name.Contains("A") || (c.ID == 2 && c.Name.Contains("T")) || c.ID==4
					select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 7);
            }
			

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void SelectSimple()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						select new { c.Name,c.ID};

			int k = 0;
			foreach (var s in query)
			{
				Assert.AreEqual(listInitial[k].Name, s.Name);
				Assert.AreEqual(listInitial[k].ID, s.ID);
				k++;
			}
			Assert.AreEqual(k, 10);
			


			}

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void SelectSimpleWithDiffType()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						select new { Customerss=c, id=c.ID};

			int k = 0;
			foreach (var s in query)
			{
				Assert.AreEqual(listInitial[k].Name, s.Customerss.Name);
				Assert.AreEqual(listInitial[k].ID, s.id);
				k++;
			}
			Assert.AreEqual(k, 10);
			

            }
			

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestUnoptimizedWhere()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.Name.Length == c.ID
						select c;

			int k = 0;
			foreach (var s in query)
			{
				Assert.AreEqual(listInitial[2].Name, s.Name);
				Assert.AreEqual(listInitial[2].ID, s.ID);
			}	
			//Assert.AreEqual(k, 1);

            }



		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestToString()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.ID.ToString() == "1"
						select c;
			

			int k = 0;
			foreach (var s in query)
			{
				Assert.AreEqual(listInitial[1].Name, s.Name);
				Assert.AreEqual(listInitial[1].ID, s.ID);
			}	
            }
		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void TestSelfMethod()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.IsTrue(c.Name) ==true
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
            }

		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void SelectNonExistingType()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Something>();
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Something c in nop
						select new { c.one, c.two };

			
			Assert.AreEqual(0,query.ToList().Count);

            }



		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void SelectWhere()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop where c.ID<3
						select new { c.Name, c.ID };

			int k = 0;
			foreach (var s in query)
			{
				Assert.AreEqual(listInitial[k].Name, s.Name);
				Assert.AreEqual(listInitial[k].ID, s.ID);
				k++;
			}
			Assert.AreEqual( 3,k);

            }



		}
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void SelectWhereUsingProperty()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 10; i++)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.IDProp < 3
						select new { c.Name, c.ID };

			int k = 0;
			foreach (var s in query)
			{
				Assert.AreEqual(listInitial[k].Name, s.Name);
				Assert.AreEqual(listInitial[k].ID, s.ID);
				k++;
			}
			Assert.AreEqual(3, k);

			try
			{ 
			query = from Customer c in nop
						where c.IDPropWithoutAtt < 3
						select new { c.Name, c.ID };
			 
				 foreach (var s in query)
				 {

				 }
				 //Assert.Fail("Property cannot work without Att");
			 }
			 catch (Exception ex)
			 {
				 Assert.AreEqual("A Property must have UseVariable Attribute set", ex.Message);
			 }
            try
            {
			query = from Customer c in nop
					 where c.IDPropWithNonExistingVar < 3
					 select new { c.Name, c.ID };
			
				foreach (var s in query)
				{

				}
				Assert.Fail("Property cannot work without Att");
			}
			catch (Exception ex)
			{
				if (ex.Message.StartsWith("Field:"))
				{ 
					
				}
				else
					Assert.Fail(ex.Message);
			}
            }

		}
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void SelectWhereUsingAutomaticProperties()
        {
            using(Siaqodb nop = new Siaqodb(objPath)){
            nop.DropType<CustomerLite>();
            List<CustomerLite> listInitial = new List<CustomerLite>();
            for (int i = 0; i < 10; i++)
            {
                CustomerLite c = new CustomerLite();
                c.Age = i;
                if (i % 2 == 0)
                {
                    c.Name = i.ToString() + "TEST";
                }
                else
                {
                    c.Name = "Siaqo" + i.ToString();
                }
                listInitial.Add(c);
                nop.StoreObject(c);
            }
            nop.Flush();
            var query = from CustomerLite c in nop
                        where c.Age < 3
                        select new { c.Name, c.Age };

            int k = 0;
            foreach (var s in query)
            {
                Assert.AreEqual(listInitial[k].Name, s.Name);
                Assert.AreEqual(listInitial[k].Age, s.Age);
                k++;
            }
            Assert.AreEqual(3, k);

            query = from CustomerLite c in nop
                    where c.Active==true
                    select new { c.Name, c.Age };
            k = 0;
            foreach (var s in query)
            {
                Assert.AreEqual(listInitial[k].Name, s.Name);
                Assert.AreEqual(listInitial[k].Age, s.Age);
                k++;
            }
            Assert.AreEqual(10, k);
            }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void SelectWhereUnaryOperator()
        {
            using(Siaqodb nop = new Siaqodb(objPath)){
            nop.DropType<CustomerLite>();
            List<CustomerLite> listInitial = new List<CustomerLite>();
            for (int i = 0; i < 10; i++)
            {
                CustomerLite c = new CustomerLite();
                c.Age = i;
                if (i % 2 == 0)
                {
                    c.Name = i.ToString() + "TEST";
                }
                else
                {
                    c.Name = "Siaqo" + i.ToString();
                }
                c.Active = false;
                listInitial.Add(c);
                nop.StoreObject(c);
            }
            nop.Flush();
           
            //run unoptimized
            var query = (from CustomerLite c in nop
                    where c.Age>5 && !c.Active 
                    select new { c.Name, c.Age }).ToList();
           int k = 0;
            
            Assert.AreEqual(4, query.Count);
            }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        [ExpectedException(typeof(NotSupportedException))]
        public void SelectWhereMinus()
        {
            using(Siaqodb nop = new Siaqodb(objPath)){
            nop.DropType<CustomerLite>();
            List<CustomerLite> listInitial = new List<CustomerLite>();
            for (int i = 0; i < 10; i++)
            {
                CustomerLite c = new CustomerLite();
                c.Age = i;
                if (i % 2 == 0)
                {
                    c.Name = i.ToString() + "TEST";
                }
                else
                {
                    c.Name = "Siaqo" + i.ToString();
                }
                c.Active = false;
                listInitial.Add(c);
                nop.StoreObject(c);
            }
            nop.Flush();

           
            var query = (from CustomerLite c in nop
                         where c.Age+2>0
                         select new { c.Name, c.Age }).ToList();
            int k = 0;

            Assert.AreEqual(3, query.Count);
            }

        }
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void SelectWhereBooleanAlone()
        {
            using(Siaqodb nop = new Siaqodb(objPath)){
            nop.DropType<CustomerLite>();
            List<CustomerLite> listInitial = new List<CustomerLite>();
            for (int i = 0; i < 10; i++)
            {
                CustomerLite c = new CustomerLite();
                c.Age = i;
                if (i % 2 == 0)
                {
                    c.Name = i.ToString() + "TEST";
                }
                else
                {
                    c.Name = "Siaqo" + i.ToString();
                }
                //c.Active = true;
                listInitial.Add(c);
                nop.StoreObject(c);
            }
            nop.Flush();

            //run optimized
            var query = (from CustomerLite c in nop
                         where c.Active
                         select c).ToList();
            int k = 0;

            Assert.AreEqual(10, query.Count);

            //need some more tests here
            var query1 = (from CustomerLite c in nop
                         where c.Age>5 && c.Active
                         select new { c.Name, c.Age }).ToList();
           

            Assert.AreEqual(4, query1.Count);
            }

        }
		#if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
		public void OrderByBasic()
		{
			using(Siaqodb nop = new Siaqodb(objPath)){
			nop.DropType<Customer>();
			List<Customer> listInitial = new List<Customer>();
			for (int i = 10; i > 0; i--)
			{
				Customer c = new Customer();
				c.ID = i;
				if (i % 2 == 0)
				{
					c.Name = i.ToString() + "TEST";
				}
				else
				{
					c.Name = "ADH" + i.ToString();
				}
				listInitial.Add(c);
				nop.StoreObject(c);
			}
			nop.Flush();
			var query = from Customer c in nop
						where c.ID > 4 orderby c.ID
						select new { c.Name, c.ID };

			int k = 0;
			foreach (var s in query)
			{
				if (k == 0)
				{
					Assert.AreEqual(5,s.ID);
					//Assert.AreEqual(listInitial[k].ID, s.ID);
				}
				k++;
			}
			//Assert.AreEqual(3, k);
            }

		}
        #if __MOBILE__
		[Test]
		#else
		[TestMethod]

		#endif
        public void SelectWhereUsingEnum()
        {
            using (Siaqodb nop = new Siaqodb(objPath))
            {
                nop.DropType<CustomerLite>();
                List<CustomerLite> listInitial = new List<CustomerLite>();
                for (int i = 0; i < 10; i++)
                {
                    CustomerLite c = new CustomerLite();
                    c.Name = i.ToString();
                    c.Age = i;
                    if (i % 3 == 0)
                    {
                        c.TEnum = TestEnum.Doi;
                    }
                    else
                    {
                        c.TEnum = TestEnum.Trei;
                    }
                    listInitial.Add(c);
                    nop.StoreObject(c);
                }
                nop.Flush();
                var query = from CustomerLite c in nop
                            where c.Age < 3
                            select new { c.Name, c.TEnum };

                int k = 0;
                foreach (var s in query)
                {
                    Assert.AreEqual(listInitial[k].Name, s.Name);
                    Assert.AreEqual(listInitial[k].TEnum, s.TEnum);
                    k++;
                }
                Assert.AreEqual(3, k);
            }




        }


	}
}
