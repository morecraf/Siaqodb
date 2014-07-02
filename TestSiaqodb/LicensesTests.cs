using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo;
using TestSiaqodb.M.S;

namespace TestSiaqodb
{
    /// <summary>
    /// Summary description for LicensesTests
    /// </summary>
    [TestClass]
    public class LicensesTests
    {
        string objPath = @"e:\sqoo\temp\tests_db\";
        public LicensesTests()
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

         [TestMethod]
        public void TestLicenseStarterEdition()
        {
            Siaqodb s_db = new Siaqodb(objPath);
            s_db.DropType<A>();
            s_db.DropType<B>();
            s_db.DropType<C>();
            for (int i = 0; i < 102; i++)
            {
                A a = new A();
                a.aId = i;
                a.BVar = new B();
                a.BVar.bId = 11;
                a.BVar.Ci = new C();
                a.BVar.Ci.ACircular = a;
                a.BVar.Ci.cId = i % 2;
                s_db.StoreObject(a);
            }
            IList<A> lsA = s_db.LoadAll<A>();

            Assert.AreEqual(102, lsA.Count);
            s_db.Close();
            bool ok = false;
            try
            {


                s_db.Open(objPath);
            }
            catch (Sqo.Exceptions.InvalidLicenseException e)
            {
                ok = true;
            }
            Assert.IsTrue(ok);
           
           
        }
    }
}
