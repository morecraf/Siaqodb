using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sqo;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestSiaqodbAsync.Models;

namespace TestSiaqodbAsync
{
    [TestClass]
    public class ChildObjectTests
    {
        string dbFolder = @"D:\morecraf\temp\SqoUnitTests\";
        public ChildObjectTests()
        {
            Sqo.SiaqodbConfigurator.SetLicense(@"VpMKWZsHgtvrUfEPCu0WDGLk5nlVs2+5yN8youWUSixTKvmLnjsVUq9r9kdfFMuCMGtT9uyrBHNQAv+V2KkxOg==");
        }

        [TestMethod]
        public void TestDeleteParentNotChild()
        {
            // setup
            Siaqodb db = new Siaqodb(dbFolder);
            db.DropAllTypes();

            ParentObject p = new ParentObject();
            p.Name = "TestParent";
            p.PropData = new PropertyADO();
            p.PropData.Name = "TestChild";
            db.StoreObject(p);

            Assert.AreEqual(db.Count<ParentObject>(), 1);
            Assert.AreEqual(db.Count<PropertyADO>(), 1);

            db.Delete(p);
            Assert.AreEqual(db.Count<ParentObject>(), 0);
            Assert.AreEqual(db.Count<PropertyADO>(), 1);
            db.Close();
        }

        [TestMethod]
        public void TestDeleteParentAndChild()
        {
            // setup
            Siaqodb db = new Siaqodb(dbFolder);
            db.DropAllTypes();

            ParentObject p = new ParentObject();
            p.Name = "TestParent";
            p.PropData = new PropertyADO();
            p.PropData.Name = "TestChild";
            db.StoreObject(p);

            Assert.AreEqual(db.Count<ParentObject>(), 1);
            Assert.AreEqual(db.Count<PropertyADO>(), 1);

            db.Delete(p, true);
            Assert.AreEqual(db.Count<ParentObject>(), 0);
            Assert.AreEqual(db.Count<PropertyADO>(), 0);
            db.Close();
        }

        [TestMethod]
        public void TestDeleteParentAndChildren()
        {
            // setup
            Siaqodb db = new Siaqodb(dbFolder);
            db.DropAllTypes();

            ParentObject p = new ParentObject();
            p.Name = "TestParent";
            p.PropData = new PropertyADO();
            p.PropData.Name = "TestChild";
            p.PropData.Address = new AddressADO();
            p.PropData.Address.Address1 = "Address1";
            p.PropData.Address.Address2 = "Address2";
            p.PropData.Address.AddressItems = new List<AddressItemADO>();
            p.PropData.Address.AddressItems.Add(new AddressItemADO() { Prop1 = "Test1", Prop2 = "Test1p2" } );
            p.PropData.Address.AddressItems.Add(new AddressItemADO() { Prop1 = "Test2", Prop2 = "Test2p2" } );
            db.StoreObject(p);

            Assert.AreEqual(db.Count<ParentObject>(), 1);
            Assert.AreEqual(db.Count<PropertyADO>(), 1);
            Assert.AreEqual(db.Count<AddressADO>(), 1);
            Assert.AreEqual(db.Count<AddressItemADO>(), 2);

            db.Delete(p, true);
            Assert.AreEqual(db.Count<ParentObject>(), 0);
            Assert.AreEqual(db.Count<PropertyADO>(), 0);
            Assert.AreEqual(db.Count<AddressADO>(), 0);
            Assert.AreEqual(db.Count<AddressItemADO>(), 0);
            db.Close();
        }
    }
}
