using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Sqo;

using Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo.Exceptions;

namespace TestSiaqodbSilver
{
	/// <summary>
	/// Summary description for Join
	/// </summary>
	[TestClass]
	public class JoinTest
	{
		string objPath = @"nopjoinDB";
		public JoinTest()
		{
			//
			// TODO: Add constructor logic here
			//
            //SiaqodbConfigurator.SetTrialLicense("thoifHnsZCLebOqrgEzO8PitCURHXso6TIK88vqWC94=");
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
		public void SimpleJoin()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			nop.DropType<Employee>();

			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 20; i++)
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
			List<Employee> listInitialEmp = new List<Employee>();
			for (int j = 0; j < 10; j++)
			{
				Employee emp = new Employee();
				emp.CustomerID = j;
				emp.Name = "Employee" + j.ToString();
				emp.ID = j;
				listInitialEmp.Add(emp);
				nop.StoreObject(emp);
			}
			nop.Flush();

			var query = (from Customer c in nop
						 where c.ID < 3
						 join Employee emp in nop
							 on c.ID equals emp.CustomerID

						 select new EmpCustAnonym { CName = c.Name, EName = emp.Name }).ToList();
		
			Assert.AreEqual(3,query.Count );
			Assert.AreEqual(listInitial[0].Name ,query[0].CName);
			Assert.AreEqual(listInitialEmp[0].Name, query[0].EName);

			var query1 = (from Customer c in nop
					 where c.ID < 3
					 join Employee emp in nop
						 on c.ID equals emp.CustomerID

						  select new EmpCustOID { CName = c.Name, EName = emp.Name, EOID = emp.OID}).ToList();

			Assert.AreEqual(3, query1.Count);
			Assert.AreEqual(listInitial[0].Name, query1[0].CName);
			Assert.AreEqual(listInitialEmp[0].Name, query1[0].EName);

		
		}
        [TestMethod]
        public void SimpleJoinWithAutomaticProperties()
        {
            Siaqodb nop = new Siaqodb(objPath);
            nop.DropType<CustomerLite>();
            nop.DropType<Employee>();

            List<CustomerLite> listInitial = new List<CustomerLite>();
            for (int i = 0; i < 20; i++)
            {
                CustomerLite c = new CustomerLite();
                c.Age = i;
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
            List<Employee> listInitialEmp = new List<Employee>();
            for (int j = 0; j < 10; j++)
            {
                Employee emp = new Employee();
                emp.CustomerID = j;
                emp.Name = "Employee" + j.ToString();
                emp.ID = j;
                listInitialEmp.Add(emp);
                nop.StoreObject(emp);
            }
            nop.Flush();

            var query = (from CustomerLite c in nop
                         where c.Age < 3
                         join Employee emp in nop
                             on c.Age equals emp.CustomerID

                         select new EmpCustAnonym { CName = c.Name, EName = emp.Name }).ToList();

            Assert.AreEqual(3, query.Count);
            Assert.AreEqual(listInitial[0].Name, query[0].CName);
            Assert.AreEqual(listInitialEmp[0].Name, query[0].EName);

            var query1 = (from CustomerLite c in nop
                          where c.Age < 3
                          join Employee emp in nop
                              on c.Age equals emp.CustomerID

                          select new EmpCustOID { CName = c.Name, EName = emp.Name, EOID = emp.OID }).ToList();

            Assert.AreEqual(3, query1.Count);
            Assert.AreEqual(listInitial[0].Name, query1[0].CName);
            Assert.AreEqual(listInitialEmp[0].Name, query1[0].EName);


        }
        [TestMethod]
        public void SimpleJoinWithEnum()
        {
            Siaqodb nop = new Siaqodb(objPath);
            nop.DropType<CustomerLite>();
            nop.DropType<EmployeeLite>();
            List<string> s = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                CustomerLite c = new CustomerLite();

                if (i % 2 == 0)
                {
                    c.TEnum = TestEnum.Unu;
                }
                else
                {
                    c.TEnum = TestEnum.Doi;
                }
                ///listInitial.Add(c);
                nop.StoreObject(c);
            }
            List<EmployeeLite> listInitialEmp = new List<EmployeeLite>();
            for (int j = 0; j < 3; j++)
            {
                EmployeeLite emp = new EmployeeLite();
                emp.EmpEnum = TestEnum.Doi;
                emp.Name = "Employee" + j.ToString();
                emp.ID = j;
                listInitialEmp.Add(emp);
                nop.StoreObject(emp);
            }
            nop.Flush();
            var query = (from CustomerLite c in nop
                         join EmployeeLite emp in nop
                             on c.TEnum equals emp.EmpEnum

                         select emp).ToList();

            Assert.AreEqual(15, query.Count);
            Assert.AreEqual(listInitialEmp[1].Name, query[1].Name);


        }
        [TestMethod]
        [ExpectedException(typeof(SiaqodbException))]
        public void SimpleJoinAnonymousType()
        {
            Siaqodb nop = new Siaqodb(objPath);
            nop.DropType<CustomerLite>();
            nop.DropType<EmployeeLite>();
            List<string> s = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                CustomerLite c = new CustomerLite();

                if (i % 2 == 0)
                {
                    c.TEnum = TestEnum.Unu;
                }
                else
                {
                    c.TEnum = TestEnum.Doi;
                }
                ///listInitial.Add(c);
                nop.StoreObject(c);
            }
            List<EmployeeLite> listInitialEmp = new List<EmployeeLite>();
            for (int j = 0; j < 3; j++)
            {
                EmployeeLite emp = new EmployeeLite();
                emp.EmpEnum = TestEnum.Doi;
                emp.Name = "Employee" + j.ToString();
                emp.ID = j;
                listInitialEmp.Add(emp);
                nop.StoreObject(emp);
            }
            nop.Flush();
            var query = (from CustomerLite c in nop
                         join EmployeeLite emp in nop
                             on c.TEnum equals emp.EmpEnum

                         select new { CName = c.Name, EmpName = emp.Name }).ToList();

            Assert.AreEqual(15, query.Count);
            Assert.AreEqual(listInitialEmp[1].Name, query[1].EmpName);


        }
        
		[TestMethod]
		public void SimpleJoinWithWhere()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			nop.DropType<Employee>();

			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 20; i++)
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
			List<Employee> listInitialEmp = new List<Employee>();
			for (int j = 0; j < 10; j++)
			{
				Employee emp = new Employee();
				emp.CustomerID = j;
				emp.Name = "Employee" + j.ToString();
				emp.ID = j;
				listInitialEmp.Add(emp);
				nop.StoreObject(emp);
			}
			nop.Flush();
			//!!!WARNING not yet optimized
			var query = (from Customer c in nop
						 from Employee emp in nop
						 where c.ID==emp.CustomerID && c.ID<3
						 
						 select new EmpCust { CName = c.Name, EName = emp.Name }).ToList();

			Assert.AreEqual(3, query.Count);
			Assert.AreEqual(listInitial[0].Name, query[0].CName);
			Assert.AreEqual(listInitialEmp[0].Name, query[0].EName);
		}
		[TestMethod]
		public void SimpleJoinUseProperty()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			nop.DropType<Employee>();

			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 20; i++)
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
			List<Employee> listInitialEmp = new List<Employee>();
			for (int j = 0; j < 10; j++)
			{
				Employee emp = new Employee();
				emp.CustomerID = j;
				emp.Name = "Employee" + j.ToString();
				emp.ID = j;
				listInitialEmp.Add(emp);
				nop.StoreObject(emp);
			}
			nop.Flush();
			var query = (from Customer c in nop
						 where c.IDProp < 3
						 join Employee emp in nop
							 on c.ID equals emp.CustomerID

						 select new EmpCust { CName = c.Name, EName = emp.Name }).ToList();

			Assert.AreEqual(3, query.Count);
			Assert.AreEqual(listInitial[0].Name, query[0].CName);
			Assert.AreEqual(listInitialEmp[0].Name, query[0].EName);

			try
			{
			query = (from Customer c in nop
					 where c.IDPropWithoutAtt < 3
					 join Employee emp in nop
						 on c.ID equals emp.CustomerID

					 select new EmpCust { CName = c.Name, EName = emp.Name }).ToList();

			
				foreach (var s in query)
				{

				}
				Assert.Fail("Property cannot work without Att");
			}
			catch (Exception ex)
			{
                Assert.IsTrue(ex.Message.Contains("A Property must have UseVariable Attribute"));
			}
			try
			{
			query = (from Customer c in nop
					 where c.IDPropWithNonExistingVar < 3
					 join Employee emp in nop
						 on c.ID equals emp.CustomerID

					 select new EmpCust { CName = c.Name, EName = emp.Name }).ToList();

			
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
		public void SimpleProjectionUsingProperty()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			
			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 20; i++)
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

			var query = (from Customer c in nop

						 select new EmpCust2 { CName = c.Name, EName = c.IDProp,OID=c.OID }).ToList();

			for (int i = 0; i < listInitial.Count; i++)
			{
				Assert.AreEqual(listInitial[i].ID, query[i].EName);
			}
			try
			{
				 query = (from Customer c in nop

						  select new EmpCust2 { CName = c.Name, EName = c.IDPropWithNonExistingVar, OID = c.OID }).ToList();

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
			try
			{
				 query = (from Customer c in nop

						  select new EmpCust2 { CName = c.Name, EName = c.IDPropWithoutAtt, OID = c.OID }).ToList();

				Assert.Fail("Property cannot work without Att");
			}
			catch (Exception ex)
			{
				Assert.AreEqual("A Property must have UseVariable Attribute set", ex.Message);
			}
			
		}
		/*[TestMethod]
		public void MultipleJoins()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			nop.DropType<Employee>();
			nop.DropType<Order>();

			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 20; i++)
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
			List<Employee> listInitialEmp = new List<Employee>();
			for (int j = 0; j < 10; j++)
			{
				Employee emp = new Employee();
				emp.CustomerID = j;
				emp.Name = "Employee" + j.ToString();
				emp.ID = j;
				listInitialEmp.Add(emp);
				nop.StoreObject(emp);
			}
			List<Order> listInitialOrders = new List<Order>();
			for (int j = 0; j < 10; j++)
			{
				Order emp = new Order();
				emp.EmployeeID = j;
				emp.Name = "Order" + j.ToString();
				emp.ID = j;
				listInitialOrders.Add(emp);
				nop.StoreObject(emp);
			}
			nop.Flush();
			var query = (from Customer c in nop
						 where c.ID < 3
						 join Employee emp in nop
							 on c.ID equals emp.CustomerID
							 
						 join Order ord in nop 
							 on emp.ID equals ord.EmployeeID

						 select new EmpCust { CName = c.Name, EName = emp.Name }).ToList();

			Assert.AreEqual(3, query.Count);
			Assert.AreEqual(listInitial[0].Name, query[0].CName);
			Assert.AreEqual(listInitialEmp[0].Name, query[0].EName);
		}*/
		[TestMethod]
		public void SimpleJoin2()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			nop.DropType<Employee>();

			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 20; i++)
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
			List<Employee> listInitialEmp = new List<Employee>();
			for (int j = 0; j < 10; j++)
			{
				Employee emp = new Employee();
				emp.CustomerID = j;
				emp.Name = "Employee" + j.ToString();
				emp.ID = j;
				listInitialEmp.Add(emp);
				nop.StoreObject(emp);
			}
			nop.Flush();
			var query = (from Customer c in nop
						 where c.ID < 3
						 join Employee emp in nop
							 on c.ID equals emp.CustomerID

						 select new EmpCustObjects{ CName = c, EName = emp }).ToList();

			Assert.AreEqual(3, query.Count);
			Assert.AreEqual(listInitial[0].Name, query[0].CName.Name);
			Assert.AreEqual(listInitialEmp[0].Name, query[0].EName.Name);
		}
		[TestMethod]
		public void SimpleJoinOrderBy()
		{
			Siaqodb nop = new Siaqodb(objPath);
			nop.DropType<Customer>();
			nop.DropType<Employee>();

			List<Customer> listInitial = new List<Customer>();
			for (int i = 0; i < 20; i++)
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
			List<Employee> listInitialEmp = new List<Employee>();
			for (int j = 0; j < 10; j++)
			{
				Employee emp = new Employee();
				emp.CustomerID = j;
				emp.Name = "Employee" + j.ToString();
				emp.ID = j;
				listInitialEmp.Add(emp);
				nop.StoreObject(emp);
			}
			nop.Flush();
			var query = from Customer c in nop
						 where c.ID < 3
						 join Employee emp in nop
							 on c.ID equals emp.CustomerID
							//orderby c.Name
						 select new EmpCust{ CName = c.Name, EName = emp.Name };
			int k = 0;
			foreach (var s in query)
			{
				if (k == 1)
					Assert.AreEqual(s.CName, "2TEST");
			}
			var query2 = from Customer c in nop
						where c.ID < 3
						orderby c.Name
						join Employee emp in nop
							on c.ID equals emp.CustomerID
						select new EmpCust{ CName = c.Name, EName = emp.Name };

			foreach (var s in query2)
			{
				if (k == 1)
					Assert.AreEqual(s.CName, "2TEST");
			}
			
		}

	}
}

