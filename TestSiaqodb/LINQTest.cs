using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo;

namespace TestSiaqodb
{
	/// <summary>
	/// Summary description for LINQTest
	/// </summary>
	[TestClass]
	public class LINQTest
	{
        string objPath = @"c:\apps\OpenSource projects\sqoo\tests\unitests\";
		
		public LINQTest()
		{
            SiaqodbConfigurator.EncryptedDatabase = true;
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

		[TestMethod]
		public void TestBasicQuery()
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
            var query = from Customer c in nop
                        select c;
			Assert.AreEqual(query.ToList<Customer>().Count,10);
		}
		
		[TestMethod]
		public void TestBasicWhere()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestBasicWhereByOID()
		{
			Siaqodb nop = new Siaqodb(objPath);
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

		[TestMethod]
		public void TestBasicWhereOperators()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestBasicWhereStringComparison()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
						where c.Name.StartsWith("a")
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
					where c.Name.EndsWith("TESt")
					select c;
			Assert.AreEqual(5, query.ToList<Customer>().Count);


		}
		int id=3;
		[TestMethod]
		public void WhereLocalVariable()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void WhereLocalMethod()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
        [TestMethod]
        public void WhereLocalMethodOverObject()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void WhereAnd()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
						where c.Name.Contains("a") && c.Name.Contains("3")
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
			Assert.AreEqual(3, query.ToList<Customer>()[0].ID);

			query = from Customer c in nop
					where c.Name.Contains("a") && (c.Name.Contains("3") && c.ID==3)
					select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 1);
			Assert.AreEqual(3, query.ToList<Customer>()[0].ID);

		}
		[TestMethod]
		public void SimpleSelect()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
						where c.Name.Contains("a") && c.Name.Contains("3")
						select new { Name=c.Name, Som=c.ID};
			int s=0;
			foreach( var a in query)
			{
				s++;
			}
			Assert.AreEqual(1,s);

			

		}
		[TestMethod]
		public void WhereOR()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
						where c.Name.Contains("a") || c.ID==2
						select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 6);
			

			query = from Customer c in nop
					where c.Name.Contains("a") || (c.ID == 2 && c.Name.Contains("T")) || c.ID==4
					select c;

			Assert.AreEqual(query.ToList<Customer>().Count, 7);
			

		}
		[TestMethod]
		public void SelectSimple()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void SelectSimpleWithDiffType()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestUnoptimizedWhere()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestToString()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void TestSelfMethod()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void SelectNonExistingType()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void SelectWhere()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void SelectWhereUsingProperty()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
        [TestMethod]
        public void SelectWhereUsingAutomaticProperties()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
        [TestMethod]
        public void SelectWhereUnaryOperator()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void SelectWhereMinus()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
        [TestMethod]
        public void SelectWhereBooleanAlone()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
		[TestMethod]
		public void OrderByBasic()
		{
			Siaqodb nop = new Siaqodb(objPath);
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
        [TestMethod]
        public void SelectWhereUsingEnum()
        {
            Siaqodb nop = new Siaqodb(objPath);
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
