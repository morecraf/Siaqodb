using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Storage;
using Sqo;
using System.Threading.Tasks;
using System.Collections;

namespace SiaqodbUnitTests
{
    [TestClass]
    public class LINQTests
    {
        StorageFolder dbFolder = ApplicationData.Current.LocalFolder;
        public LINQTests()
        {
              SiaqodbConfigurator.SetLicense("Q3ALvFX78oSAX5bF/uJhboptXN5g2EZLsyiBLHIsWbuIPn+HGtqvTaSZUortZcEV");
          
        }
        [TestMethod]
        public async Task TestBasicQuery()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                         select c).ToListAsync();
            Assert.AreEqual(query.Count, 10);
        }

        [TestMethod]
        public async Task TestBasicWhere()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            List<Customer> listInitial = new List<Customer>();
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();
                listInitial.Add(c);
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.ID < 5
                        select c).ToListAsync();
            Assert.AreEqual(query.Count, 5);
            query = await (from Customer c in nop
                    where c.ID > 5
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 4);

            query = await (from Customer c in nop
                    where c.ID == 5
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 1);

            Assert.AreEqual(listInitial[5].Name, query[0].Name);
            Assert.AreEqual(listInitial[5].ID, query[0].ID);
            Assert.AreEqual(listInitial[5].OID, query[0].OID);
        }
        [TestMethod]
        public async Task TestBasicWhereByOID()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            List<Customer> listInitial = new List<Customer>();
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();
                listInitial.Add(c);
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.OID < 5
                        select c).ToListAsync();
            Assert.AreEqual(query.Count, 4);
            query = await (from Customer c in nop
                    where c.OID > 5
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 5);

            query = await (from Customer c in nop
                    where c.OID > 5 && c.OID < 8
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 2);


            query = await (from Customer c in nop
                    where c.OID == 5
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 1);

            Assert.AreEqual(listInitial[4].Name, query[0].Name);
            Assert.AreEqual(listInitial[4].ID, query[0].ID);
            Assert.AreEqual(listInitial[4].OID, query[0].OID);
        }

        [TestMethod]
        public async Task TestBasicWhereOperators()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            List<Customer> listInitial = new List<Customer>();
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();
                listInitial.Add(c);
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.ID < 5
                        select c).ToListAsync();
            Assert.AreEqual(query.Count, 5);
            query = await (from Customer c in nop
                    where c.ID > 3
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 6);
            query = await (from Customer c in nop
                    where c.ID >= 3
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 7);
            query = await (from Customer c in nop
                    where c.ID <= 3
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 4);

            query = await (from Customer c in nop
                    where c.ID != 3
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 9);


        }
        [TestMethod]
        public async Task TestBasicWhereStringComparison()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.Name.Contains("ADH")
                        select c).ToListAsync();

            Assert.AreEqual(query.Count, 5);
            query = await (from Customer c in nop
                    where c.Name.Contains("2T")
                    select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);

            query = await (from Customer c in nop
                    where c.Name.StartsWith("A")
                    select c).ToListAsync();
            Assert.AreEqual(query.Count, 5);
            query = await (from Customer c in nop
                    where c.Name.StartsWith("ake")
                    select c).ToListAsync();


            Assert.AreEqual(query.Count, 0);
            query = await (from Customer c in nop
                    where c.Name.EndsWith("ADH")
                    select c).ToListAsync();
            Assert.AreEqual(0, query.Count);
            query = await (from Customer c in nop
                    where c.Name.EndsWith("TEST")
                    select c).ToListAsync();
            Assert.AreEqual(5, query.Count);


        }
        int id = 3;
        [TestMethod]
        public async Task WhereLocalVariable()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.ID == this.id
                        select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);
            Assert.AreEqual(3, query[0].ID);
        }
        public int TestMet(int t)
        {
            return t + 1;
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
        public async Task WhereLocalMethod()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.ID == this.TestMet(3)
                        select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);
            Assert.AreEqual(4, query[0].ID);

            query = await (from Customer c in nop
                    where c.OID == this.TestMet(3)
                    select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);
            Assert.AreEqual(4, query[0].OID);
        }
        [TestMethod]
        public async Task WhereLocalMethodOverObject()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            //run unoptimized
            var query = await (from Customer c in nop
                        where this.TestMet2(c.ID) == 3
                        select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);
            Assert.AreEqual(2, query[0].ID);

            query = await (from Customer c in nop
                    where this.TestMet3(c) == 3
                    select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);
            Assert.AreEqual(3, query[0].ID);

        }
        [TestMethod]
        public async Task WhereAnd()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.Name.Contains("A") && c.Name.Contains("3")
                        select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);
            Assert.AreEqual(3, query[0].ID);

            query = await (from Customer c in nop
                    where c.Name.Contains("A") && (c.Name.Contains("3") && c.ID == 3)
                    select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);
            Assert.AreEqual(3, query[0].ID);

        }
        [TestMethod]
        public async Task SimpleSelect()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.Name.Contains("A") && c.Name.Contains("3")
                        select new CustomerAnony { Name = c.Name,ID  = c.ID }).ToListAsync();
            int s = 0;
            foreach (var a in query)
            {
                s++;
            }
            Assert.AreEqual(1, s);



        }
        [TestMethod]
        public async Task WhereOR()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.Name.Contains("A") || c.ID == 2
                        select c).ToListAsync();

            Assert.AreEqual(query.Count, 6);


            query = await (from Customer c in nop
                    where c.Name.Contains("A") || (c.ID == 2 && c.Name.Contains("T")) || c.ID == 4
                    select c).ToListAsync();

            Assert.AreEqual(query.Count, 7);


        }
        [TestMethod]
        public async Task SelectSimple()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        select new {Name= c.Name,ID= c.ID }).ToListAsync();

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
        public async Task SelectSimpleWithDiffType()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        select new CustomerAnony{ Name = c.Name, ID = c.ID }).ToListAsync();

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
        public async Task TestUnoptimizedWhere()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.Name.Length == c.ID
                        select c).ToListAsync();

            int k = 0;
            foreach (var s in query)
            {
                Assert.AreEqual(listInitial[2].Name, s.Name);
                Assert.AreEqual(listInitial[2].ID, s.ID);
            }
            //Assert.AreEqual(k, 1);





        }
        [TestMethod]
        public async Task TestToString()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.ID.ToString() == "1"
                        select c).ToListAsync();


            int k = 0;
            foreach (var s in query)
            {
                Assert.AreEqual(listInitial[1].Name, s.Name);
                Assert.AreEqual(listInitial[1].ID, s.ID);
            }
        }
        [TestMethod]
        public async Task TestSelfMethod()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.IsTrue(c.Name) == true
                        select c).ToListAsync();

            Assert.AreEqual(query.Count, 1);

        }
        [TestMethod]
        public async Task SelectNonExistingType()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Something>();
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Something c in nop
                        select new SomethingAnony {One= c.one,Two= c.two }).ToListAsync();


            Assert.AreEqual(0, query.ToList().Count);





        }
        [TestMethod]
        public async Task SelectWhere()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.ID < 3
                        select new CustomerAnony{Name= c.Name,ID= c.ID }).ToListAsync();

            int k = 0;
            foreach (var s in query)
            {
                Assert.AreEqual(listInitial[k].Name, s.Name);
                Assert.AreEqual(listInitial[k].ID, s.ID);
                k++;
            }
            Assert.AreEqual(3, k);





        }
        [TestMethod]
        public async Task SelectWhereUsingProperty()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from Customer c in nop
                        where c.IDProp < 3
                        select c).ToListAsync();

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
                query = await (from Customer c in nop
                        where c.IDPropWithoutAtt < 3
                        select c).ToListAsync();

                foreach (var s in query)
                {

                }
                //Assert.Fail("Property cannot work without Att");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("A Property must have UseVariable Attribute"));
            }
            try
            {
                query = await (from Customer c in nop
                        where c.IDPropWithNonExistingVar < 3
                        select c).ToListAsync();

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
        public async Task SelectWhereUsingAutomaticProperties()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<CustomerLite>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from CustomerLite c in nop
                        where c.Age < 3
                        select c).ToListAsync();

            int k = 0;
            foreach (var s in query)
            {
                Assert.AreEqual(listInitial[k].Name, s.Name);
                Assert.AreEqual(listInitial[k].Age, s.Age);
                k++;
            }
            Assert.AreEqual(3, k);

            query = await (from CustomerLite c in nop
                    where c.Active == true
                           select c).ToListAsync();
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
        public async Task SelectWhereUnaryOperator()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<CustomerLite>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            //run unoptimized
            var query = await (from CustomerLite c in nop
                         where c.Age > 5 && !c.Active
                         select new { c.Name, c.Age }).ToListAsync();
            int k = 0;

            Assert.AreEqual(4, query.Count);

        }
        [TestMethod]
         public async Task SelectWhereMinus()
        {
            bool exTh = false;
            try
            {
                Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
                await nop.DropTypeAsync<CustomerLite>();
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
                    await nop.StoreObjectAsync(c);
                }
                await nop.FlushAsync();


                var query = await (from CustomerLite c in nop
                                   where c.Age + 2 > 0
                                   select new { c.Name, c.Age }).ToListAsync();
                int k = 0;

                Assert.AreEqual(3, query.Count);
            }
            catch (NotSupportedException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);
        }
        [TestMethod]
        public async Task SelectWhereBooleanAlone()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<CustomerLite>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            //run optimized
            var query = await (from CustomerLite c in nop
                               where c.Active
                               select c).ToListAsync();
            int k = 0;

            Assert.AreEqual(10, query.Count);

            //need some more tests here
            var query1 = await (from CustomerLite c in nop
                          where c.Age > 5 && c.Active
                                select c).ToListAsync();


            Assert.AreEqual(4, query1.Count);

        }
        [TestMethod]
        public async Task OrderByBasic()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            List<Customer> listInitial = new List<Customer>();
            int j=0;
            for (int i = 10; i > 0; i--)
            {
                Customer c = new Customer();
                c.ID = i;
                
                if (i % 2 == 0)
                {
                    c.Name = "2";
                }
                else
                {
                    c.Name = "3";
                }
                listInitial.Add(c);
                await nop.StoreObjectAsync(c);
                j++;
            }
            await nop.FlushAsync();
            var query = await(from Customer c in nop
                               where c.ID > 4
                               orderby  c.ID
                               select c).ToListAsync();

            int k = 0;
            foreach (var s in query)
            {
                if (k == 0)
                {
                    Assert.AreEqual(5, s.ID);
                    
                }
                k++;
            }
            query = await (from Customer c in nop
                                      where c.ID > 4
                                      orderby c.ID descending
                                      select c).ToListAsync();

            k = 0;
            foreach (var s in query)
            {
                if (k == 0)
                {
                    Assert.AreEqual(10, s.ID);

                }
                k++;
            }
            query = await (from Customer c in nop
                                  where c.ID > 4
                                  orderby c.ID,c.Name
                                  select c).ToListAsync();

            k = 0;
            foreach (var s in query)
            {
                if (k == 0)
                {
                    Assert.AreEqual(5, s.ID);

                }
                k++;
            }
            query = await (from Customer c in nop
                           where c.ID > 4
                           orderby c.Name,c.ID
                           select c).ToListAsync();

            k = 0;
            foreach (var s in query)
            {
                if (k == 0)
                {
                    Assert.AreEqual(6, s.ID);

                }
                k++;
            }
            //Assert.AreEqual(3, k);

        }
        [TestMethod]
        public async Task SelectWhereUsingEnum()
        {
            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<CustomerLite>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (from CustomerLite c in nop
                        where c.Age < 3
                        select c).ToListAsync();

            int k = 0;
            foreach (var s in query)
            {
                Assert.AreEqual(listInitial[k].Name, s.Name);
                Assert.AreEqual(listInitial[k].TEnum, s.TEnum);
                k++;
            }
            Assert.AreEqual(3, k);





        }
        [TestMethod]
        public async Task SkipTake()
        {

            Siaqodb nop = new Siaqodb(); await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
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
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            var query = await (await (await (from Customer c in nop
                                             where c.ID >= 5
                                             select c).SkipAsync(2)).TakeAsync(2)).ToListAsync();


            Assert.AreEqual(query.Count, 2);
            Assert.AreEqual(query[0].ID, 7);
            Assert.AreEqual(query[1].ID, 8);






        }

    }
    public class CustomerAnony
    {
        public int ID { get; set; }
        public string Name { get; set; }

       
    }
    public class SomethingAnony
    {
        public int One { get; set; }
        public int Two { get; set; }
    }
}