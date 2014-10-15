using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace TestSiaqodb
{
    /// <summary>
    /// Summary description for ObjectTableTest
    /// </summary>
    [TestClass]
    public class ObjectTableTest
    {
        public ObjectTableTest()
        {
            //
            // TODO: Add constructor logic here
            //
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
        public void Test1()
        {
            //ObjectTable tb = new ObjectTable();
            //tb.Columns.Add("unu");
            //tb.Columns.Add("doi");
            //for (int i = 0; i < 1000; i++)
            //{
            //    ObjectRow row = tb.NewRow();
            //    row["unu"] = i;
            //    row[1] = 1 + i;
            //    tb.Rows.Add(row);
            //}

        }
    }
}
