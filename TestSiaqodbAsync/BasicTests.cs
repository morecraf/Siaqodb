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


namespace SiaqodbUnitTests
{
    
    [TestClass]
    public class BasicTests
    {
        string dbFolder = @"e:\sqoo\temp\testsAsync_db\";
        public BasicTests()
        {
            SiaqodbConfigurator.SetTrialLicense("G5Km9leSRHoYJ784J8ascwPg868xkD5kGQQHDbGcvC0=");
        }
        [TestMethod]
        public async Task TestInsert()
        {
            Siaqodb nop = new Siaqodb();
            try
            {
                await nop.OpenAsync(dbFolder);
                await nop.DropTypeAsync<Customer>();
                for (int i = 10; i < 20; i++)
                {
                    Customer c = new Customer();
                    c.ID = i;
                    c.Name = "ADH" + i.ToString();
                    //c.Vasiel = "momo" + i.ToString();
                    await nop.StoreObjectAsync(c);
                }
                await nop.FlushAsync();
                IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();
                Assert.AreEqual(listC.Count, 10);
                nop.Close();
            }
            catch (Exception ex)
            { 
            
            }

        }
        [TestMethod]
        public async Task TestStringWithoutAttribute()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();

            for (int i = 10; i < 20; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();
                c.stringWithoutAtt = "hjqhdlkqwjhedlqkjwhedlkjqhwelkdjhqlwekhdqlkwjehdlkqwjhedlkjqhweljkdhqwlkejdhlqkwjhedlkqjwhedlkjqhwekldjhqlkwejdhlqkjwehdlkqjwhedlkjhwedkljqhweldkjhqwelkhdqlwkjehdlqkjwhedlkjqwhedlkjhqweljdhqwlekjdhlqkwjehdlkjqwhedlkjwq________________________********************************************************************";
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();

            Assert.AreEqual(100, listC[0].stringWithoutAtt.Length);
            nop.Close();


        }
        [TestMethod]
        public async Task TestInsertAllTypeOfFields()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<D40>();
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
            await nop.StoreObjectAsync(d);

            IObjectList<D40> all1 = await nop.LoadAllAsync<D40>();
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

            nop.Close();
            nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            IObjectList<D40> all = await nop.LoadAllAsync<D40>();
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
        [TestMethod]
        public async Task TestUpdate()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            IObjectList<Customer> listC =await nop.LoadAllAsync<Customer>();
            Assert.AreEqual(listC.Count, 10);
            listC[0].Name = "UPDATEWORK";

            await nop.StoreObjectAsync(listC[0]);
            await nop.FlushAsync();
            nop.Close();
            nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            IObjectList<Customer> listCUpdate = await nop.LoadAllAsync<Customer>();
            Assert.AreEqual("UPDATEWORK", listCUpdate[0].Name);



        }
        [TestMethod]
        public async Task TestUpdateCheckNrRecords()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            
            await nop.DropTypeAsync<Customer>();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();
            Assert.AreEqual(listC.Count, 10);
            listC[0].Name = "UPDATEWORK";

            await nop.StoreObjectAsync(listC[0]);
            await nop.FlushAsync();
            nop.Close();
            nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            
            IObjectList<Customer> listCUpdate = await nop.LoadAllAsync<Customer>();
            Assert.AreEqual("UPDATEWORK", listCUpdate[0].Name);
            Assert.AreEqual(10, listCUpdate.Count);



        }
        [TestMethod]
        public async Task TestInsertAfterDrop()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();
            await nop.DropTypeAsync<Customer>();
            await nop.StoreObjectAsync(listC[0]);
            await nop.FlushAsync();
            nop.Close();
            nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            IObjectList<Customer> listCUpdate = await nop.LoadAllAsync<Customer>();
            Assert.AreEqual(1, listCUpdate.Count);



        }

        [TestMethod]
        public async Task TestSavingEvent()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            nop.SavingObject += new EventHandler<SavingEventsArgs>(nop_SavingObject);
            nop.SavedObject += new EventHandler<SavedEventsArgs>(nop_SavedObject);
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();

            Assert.AreEqual(0, listC.Count);
            Assert.AreEqual(0, nrSaves);


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
        public async Task TestSavedEvent()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            nrSaves = 0;
            nop.SavedObject += new EventHandler<SavedEventsArgs>(nop_SavedObject);
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();

            Assert.AreEqual(10, listC.Count);
            Assert.AreEqual(10, nrSaves);


        }
        [TestMethod]
        public async Task TestDelete()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();

            await nop.DeleteAsync(listC[0]);
            await nop.DeleteAsync(listC[1]);
            await nop.FlushAsync();
            IObjectList<Customer> listDeleted = await nop.LoadAllAsync<Customer>();

            Assert.AreEqual(8, listDeleted.Count);
            Assert.AreEqual(3, listDeleted[0].OID);

        }
        [TestMethod]
        public async Task TestDeleteEvents()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            nop.DeletingObject += new EventHandler<DeletingEventsArgs>(nop_DeletingObject);
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();

            await nop.DeleteAsync(listC[0]);
            await nop.DeleteAsync(listC[1]);
            await nop.FlushAsync();
            IObjectList<Customer> listDeleted = await nop.LoadAllAsync<Customer>();

            Assert.AreEqual(10, listDeleted.Count);
            Assert.AreEqual(1, listDeleted[0].OID);

        }
        void nop_DeletingObject(object sender, DeletingEventsArgs e)
        {
            e.Cancel = true;
        }

        [TestMethod]
        public async Task TestCount()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            for (int i = 0; i < 160; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();
            await nop.DeleteAsync(listC[0]);
            await nop.FlushAsync();
            int count = await nop.CountAsync<Customer>();
            Assert.AreEqual(160, listC.Count);
            Assert.AreEqual(159, count);
        }
        [TestMethod]
        public async Task TestSaveDeletedObject()
        {
            Siaqodb nop = new Siaqodb();
            await nop.OpenAsync(dbFolder);
            await nop.DropTypeAsync<Customer>();
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ADH" + i.ToString();

                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();

            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();
            await nop.DeleteAsync(listC[0]);
            bool exTh = false;
            try
            {
                await nop.StoreObjectAsync(listC[0]);
                await nop.FlushAsync();
            }
            catch (Exception ex)
            {
                exTh = true;    
            }
             nop.Close();
             Assert.IsTrue(exTh);

        }
        [TestMethod]
        public async Task TestDeleteUnSavedObject()
        {
            bool exTh = false;
            try
            {
                Siaqodb nop = new Siaqodb();
                await nop.OpenAsync(dbFolder);
                await nop.DropTypeAsync<Customer>();

                Customer cu = new Customer();
                cu.ID = 78;
                await nop.DeleteAsync(cu);
            }
            catch (SiaqodbException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);
        }
        [TestMethod]
        
        public async Task TestUniqueExceptionInsert()
        {
            bool exTh = false;
            try
            {
                Siaqodb sq = new Siaqodb();
                await sq.OpenAsync(dbFolder);
                await sq.DropTypeAsync<ItemUnique>();

                ItemUnique c = new ItemUnique();
                c.Age = 10;
                c.S = "ceva";

                await sq.StoreObjectAsync(c);
                c.S = "cevaa";
                await sq.StoreObjectAsync(c);
                await sq.FlushAsync();

                ItemUnique c1 = new ItemUnique();
                c1.Age = 11;
                c1.S = "cevaa";

                await sq.StoreObjectAsync(c1);

            }
            catch (UniqueConstraintException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);
        }
        [TestMethod]
        
        public async Task TestUniqueExceptionInsertTransaction()
        {
            bool exTh = false;
            try
            {
                Siaqodb sq = new Siaqodb();
                await sq.OpenAsync(dbFolder);
                await sq.DropTypeAsync<ItemUnique>();

                ItemUnique c = new ItemUnique();
                c.Age = 10;
                c.S = "ceva";

                await sq.StoreObjectAsync(c);
                c.S = "cevaa";
                await sq.StoreObjectAsync(c);
                await sq.FlushAsync();

                ItemUnique c1 = new ItemUnique();
                c1.Age = 11;
                c1.S = "cevaa";

                ITransaction tr = sq.BeginTransaction();
                await sq.StoreObjectAsync(c1, tr);
                await tr.CommitAsync();

            }
            catch (UniqueConstraintException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);
        }
        [TestMethod]
        
        public async Task TestUniqueExceptionUpdate()
        {
            bool exTh = false;
            try
            {
           
            Siaqodb sq = new Siaqodb();
            await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ItemUnique>();

            ItemUnique c = new ItemUnique();
            c.Age = 10;
            c.S = "ceva";

            await sq.StoreObjectAsync(c);
            c.S = "ceva";
            await sq.StoreObjectAsync(c);
            await sq.FlushAsync();

            ItemUnique c1 = new ItemUnique();
            c1.Age = 11;
            c1.S = "ceva1";

            await sq.StoreObjectAsync(c1);

            IObjectList<ItemUnique> list = await sq.LoadAllAsync<ItemUnique>();
            list[1].S = "ceva";
            await sq.StoreObjectAsync(list[1]);//should throw exception

            }
            catch (UniqueConstraintException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);

        }
        [TestMethod]
        public async Task TestUpdateObjectBy()
        {
            Siaqodb sq = new Siaqodb();
            await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ItemUnique>();

            ItemUnique c = new ItemUnique();
            c.Age = 10;
            c.S = "some";
            await sq.StoreObjectAsync(c);

            ItemUnique c1 = new ItemUnique();
            c1.Age = 11;
            c1.S = "some1";

            await sq.StoreObjectAsync(c1);

            ItemUnique it = new ItemUnique();
            it.Age = 11;
            it.S = "someNew";
            bool stored = await sq.UpdateObjectByAsync("Age", it);
            Assert.IsTrue(stored);

            IObjectList<ItemUnique> list = await sq.LoadAllAsync<ItemUnique>();

            Assert.AreEqual("someNew", list[1].S);


            it = new ItemUnique();
            it.Age = 13;
            it.S = "someNew";
            stored = await sq.UpdateObjectByAsync("Age", it);
            Assert.IsFalse(stored);

        }
        [TestMethod]
        
        public async Task TestUpdateObjectByDuplicates()
        {
            bool exTh = false;
            try
            {
                Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
                await sq.DropTypeAsync<Employee>();

                Employee emp = new Employee();
                emp.ID = 100;

                await sq.StoreObjectAsync(emp);

                emp = new Employee();
                emp.ID = 100;
                await sq.StoreObjectAsync(emp);

                emp = new Employee();
                emp.ID = 100;
                await sq.FlushAsync();
                await sq.UpdateObjectByAsync("ID", emp);

            }
            catch (SiaqodbException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);
        }
        [TestMethod]
        
        public async Task TestUpdateObjectByFieldNotExists()
        {
          bool exTh = false;
            try
            {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;

            await sq.StoreObjectAsync(emp);
            await sq.FlushAsync();
            await sq.UpdateObjectByAsync("IDhh", emp);

            }
            catch (SiaqodbException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);
        }
        [TestMethod]
        
        public async Task TestUpdateObjectByManyFieldsDuplicates()
        {
            bool exTh = false;
            try
            {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;

            await sq.StoreObjectAsync(emp);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;

            await sq.StoreObjectAsync(emp);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            await sq.FlushAsync();
            await sq.UpdateObjectByAsync(emp, "ID", "CustomerID");
}
            catch (SiaqodbException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);

        }
        [TestMethod]
        public async Task TestUpdateObjectByManyFields()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            await sq.StoreObjectAsync(emp);

            await sq.FlushAsync();
            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";

            bool s = await sq.UpdateObjectByAsync(emp, "ID", "CustomerID", "Name");

            Assert.IsTrue(s);

        }
        [TestMethod]
        public async Task TestDeleteObjectBy()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            await sq.StoreObjectAsync(emp);


            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";

            bool s = await sq.DeleteObjectByAsync(emp, "ID", "CustomerID", "Name");
            await sq.FlushAsync();
            Assert.IsTrue(s);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            await sq.StoreObjectAsync(emp);
            await sq.FlushAsync();
            emp = new Employee();
            emp.ID = 100;

            s = await sq.DeleteObjectByAsync("ID", emp);

            Assert.IsTrue(s);


        }
        [TestMethod]
        
        public async Task TestUpdateObjectByManyFieldsConstraints()
        {
            bool exTh = false;
            try
            {
                Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
                await sq.DropTypeAsync<ItemUnique>();

                ItemUnique emp = new ItemUnique();
                emp.Age = 100;
                emp.integ = 10;
                emp.S = "g";
                await sq.StoreObjectAsync(emp);

                emp = new ItemUnique();
                emp.Age = 110;
                emp.integ = 10;
                emp.S = "gg";
                await sq.StoreObjectAsync(emp);

                emp = new ItemUnique();
                emp.Age = 100;
                emp.integ = 10;
                emp.S = "gge";


                bool s = await sq.UpdateObjectByAsync(emp, "Age", "integ");
                Assert.IsTrue(s);

                emp = new ItemUnique();
                emp.Age = 100;
                emp.integ = 10;
                emp.S = "gg";

                s = await sq.UpdateObjectByAsync(emp, "Age", "integ");
            }
            catch (UniqueConstraintException ex)
            {
                exTh = true;
            }
            Assert.IsTrue(exTh);
        }

        [TestMethod]
        public async Task TestEventsVariable()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            //await sq.DropTypeAsync<ClassWithEvents>();

            ClassWithEvents c = new ClassWithEvents();
            c.one = 10;


            await sq.StoreObjectAsync(c);
            await sq.FlushAsync();
            IObjectList<ClassWithEvents> ll = await sq.LoadAllAsync<ClassWithEvents>();




        }
        [TestMethod]
        public async Task TestIndexFirstInsert()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            var q = await (from ClassIndexes clss in sq
                    where clss.one == 9
                    select clss).ToListAsync();


            Assert.AreEqual(10, q.Count< ClassIndexes>());
            
            sq.Close();
            sq = new Siaqodb();
            await sq.OpenAsync(dbFolder);
            q = await (from ClassIndexes clss in sq
                where clss.two == 10
                select clss).ToListAsync();


            Assert.AreEqual(10, q.Count<ClassIndexes>());


        }
        [TestMethod]
        public async Task TestIndexUpdate()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            sq.Close();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            var q = await (from ClassIndexes clss in sq
                    where clss.one == 9
                    select clss).ToListAsync();


            q.ToList<ClassIndexes>()[0].one = 5;

            await sq.StoreObjectAsync(q.ToList<ClassIndexes>()[0]);

            await sq.StoreObjectAsync(q.ToList<ClassIndexes>()[1]);//just update nothing change
            await sq.FlushAsync();
            sq.Close();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            q = await (from ClassIndexes clss in sq
                where clss.one == 9
                select clss).ToListAsync();


            Assert.AreEqual(9, q.Count<ClassIndexes>());

            q = await (from ClassIndexes clss in sq
                where clss.one == 5
                select clss).ToListAsync();


            Assert.AreEqual(11, q.Count<ClassIndexes>());
        }
        [TestMethod]
        public async Task TestIndexSaveAndClose()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            sq.Close();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            var q = await (from ClassIndexes clss in sq
                    where clss.one == 9
                    select clss).ToListAsync();


            Assert.AreEqual(10, q.Count<ClassIndexes>());
        }
        [TestMethod]
        public async Task TestIndexAllOperations()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            sq.Close();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            var q = await (from ClassIndexes clss in sq
                    where clss.one <= 2
                    select clss).ToListAsync();


            Assert.AreEqual(30, q.Count<ClassIndexes>());

            q = await (from ClassIndexes clss in sq
                where clss.one < 2
                select clss).ToListAsync();


            Assert.AreEqual(20, q.Count<ClassIndexes>());
            q = await (from ClassIndexes clss in sq
                where clss.one >= 2
                select clss).ToListAsync();


            Assert.AreEqual(80, q.Count<ClassIndexes>());
            q = await (from ClassIndexes clss in sq
                where clss.one > 2
                select clss).ToListAsync();


            Assert.AreEqual(70, q.Count<ClassIndexes>());
        }
        [TestMethod]
        public async Task TestIndexUpdateObjectBy()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                cls.ID = i;
                cls.ID2 = i;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            var q = await (from ClassIndexes clss in sq
                    where clss.two == 4
                    select clss).ToListAsync();

            q.ToList<ClassIndexes>()[0].two = 5;
            await sq.UpdateObjectByAsync("ID", q.ToList<ClassIndexes>()[0]);
            await sq.FlushAsync();
            
            q = await (from ClassIndexes clss in sq
                where clss.two == 4
                select clss).ToListAsync();

            Assert.AreEqual(9, q.Count<ClassIndexes>());

            q = await (from ClassIndexes clss in sq
                where clss.two == 5
                select clss).ToListAsync();
            Assert.AreEqual(11, q.Count<ClassIndexes>());

            q.ToList<ClassIndexes>()[0].two = 6;
            await sq.UpdateObjectByAsync("ID2", q.ToList<ClassIndexes>()[0]);
            await sq.FlushAsync();
            
            q = await (from ClassIndexes clss in sq
                where clss.two == 5
                select clss).ToListAsync();
            Assert.AreEqual(10, q.Count<ClassIndexes>());

            q = await (from ClassIndexes clss in sq
                where clss.two == 6
                select clss).ToListAsync();
            Assert.AreEqual(11, q.Count<ClassIndexes>());


        }
        [TestMethod]
        public async Task TestIndexDelete()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                cls.ID = i;
                cls.ID2 = i;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            sq.Close();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);

            var q =await( from ClassIndexes clss in sq
                    where clss.two == 7
                    select clss).ToListAsync();


            await sq.DeleteAsync(q.ToList<ClassIndexes>()[0]);
            await sq.FlushAsync();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            q = await(from ClassIndexes clss in sq
                where clss.two == 7
                select clss).ToListAsync();

            Assert.AreEqual(9, q.Count<ClassIndexes>());

            await sq.DeleteObjectByAsync("ID", q.ToList<ClassIndexes>()[0]);

            await sq.FlushAsync();
            sq.Close();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            q = await(from ClassIndexes clss in sq
                where clss.two == 7
                select clss).ToListAsync();

            Assert.AreEqual(8, q.Count<ClassIndexes>());


            await sq.DeleteObjectByAsync("ID2", q.ToList<ClassIndexes>()[0]);
            await sq.FlushAsync();
            q = await (from ClassIndexes clss in sq
                where clss.two == 7
                select clss).ToListAsync();

            Assert.AreEqual(7, q.Count<ClassIndexes>());
        }
        [TestMethod]
        public async Task TestIndexAllFieldTypes()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<D40WithIndexes>();

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


                await sq.StoreObjectAsync(d);
            }
            await sq.FlushAsync();
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                cls.ID = i;
                cls.ID2 = i;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            
            sq.Close();
            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            byte byt = 5;
            var q1 = await(from D40WithIndexes di in sq
                     where di.b == byt
                     select di).ToListAsync();

            Assert.AreEqual(1, q1.ToList().Count);

            var q2 = await (from  D40WithIndexes di in sq
                     where di.bo == true
                            select di).ToListAsync();

            Assert.AreEqual(10, q2.ToList().Count);

            var q3 = await (from  D40WithIndexes di in sq
                     where di.c == 'c'
                     select di).ToListAsync();

            Assert.AreEqual(10, q3.ToList().Count);

            var q4 = await (from  D40WithIndexes di in sq
                     where di.d == 5
                     select di).ToListAsync();

            Assert.AreEqual(1, q4.ToList().Count);

            var q5 = await (from  D40WithIndexes di in sq
                     where di.de == 5
                     select di).ToListAsync();

            Assert.AreEqual(1, q5.ToList().Count);

            var q6 = await (from  D40WithIndexes di in sq
                     where di.dt == dt
                     select di).ToListAsync();

            Assert.AreEqual(10, q6.ToList().Count);

            var q7 = await (from  D40WithIndexes di in sq
                     where di.enn == myEnum.unu
                     select di).ToListAsync();

            Assert.AreEqual(10, q7.ToList().Count);

            var q8 = await (from  D40WithIndexes di in sq
                     where di.f == 6
                     select di).ToListAsync();

            Assert.AreEqual(1, q8.ToList().Count);

            var q9 = await (from  D40WithIndexes di in sq
                     where di.g == guid
                     select di).ToListAsync();

            Assert.AreEqual(10, q9.ToList().Count);

            var q10 = await (from  D40WithIndexes di in sq
                      where di.iu == 10
                      select di).ToListAsync();

            Assert.AreEqual(10, q10.ToList().Count);

            var q11 = await (from  D40WithIndexes di in sq
                      where di.l == 7
                      select di).ToListAsync();

            Assert.AreEqual(1, q11.ToList().Count);

            var q12 = await (from  D40WithIndexes di in sq
                      where di.s == 1
                      select di).ToListAsync();

            Assert.AreEqual(10, q12.ToList().Count);

            var q13 = await (from  D40WithIndexes di in sq
                      where di.sb == 1
                      select di).ToListAsync();

            Assert.AreEqual(10, q13.ToList().Count);

            var q14 = await (from  D40WithIndexes di in sq
                      where di.str.StartsWith("Abr")
                      select di).ToListAsync();

            Assert.AreEqual(10, q14.ToList().Count);

            var q15 = await (from  D40WithIndexes di in sq
                      where di.ts == tspan
                      select di).ToListAsync();

            Assert.AreEqual(10, q15.ToList().Count);

            var q16 = await (from  D40WithIndexes di in sq
                      where di.ul == 10
                      select di).ToListAsync();

            Assert.AreEqual(10, q16.ToList().Count);

            var q17 = await (from  D40WithIndexes di in sq
                      where di.us == 1
                      select di).ToListAsync();

            Assert.AreEqual(10, q17.ToList().Count);

            var q18 = await (from  ClassIndexes clss in sq
                      where clss.two == 7
                      select clss).ToListAsync();

            Assert.AreEqual(10, q18.ToList().Count);

            var q19 = await (from  D40WithIndexes di in sq
                      where di.Text == "text longgg"
                      select di).ToListAsync();

            Assert.AreEqual(10, q19.ToList().Count);
        }
        [TestMethod]
        public async Task TestAttributesOnProps()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassWithPropertiesAtt>();
            for (int i = 0; i < 10; i++)
            {
                ClassWithPropertiesAtt cls = new ClassWithPropertiesAtt();
                cls.ID = i % 2;
                cls.MyProperty = i + 1;
                cls.Stringss = "dsdsdsds";
                cls.Uniq = i;
                await sq.StoreObjectAsync(cls);
            }

            await sq.FlushAsync();
            var q = await (from  ClassWithPropertiesAtt clss in sq
                    where clss.ID == 1
                           select clss).ToListAsync();

            Assert.AreEqual(5, q.Count<ClassWithPropertiesAtt>());
            //check ignore work
            Assert.AreEqual(0, q.ToList<ClassWithPropertiesAtt>()[0].MyProperty);

            Assert.AreEqual(3, q.ToList<ClassWithPropertiesAtt>()[0].Stringss.Length);

            q.ToList<ClassWithPropertiesAtt>()[0].Uniq = 0;
            bool except = false;
            try
            {

                await sq.StoreObjectAsync(q.ToList<ClassWithPropertiesAtt>()[0]);
            }
            catch (UniqueConstraintException ex)
            {
                except = true;
            }
            Assert.AreEqual(true, except);

        }
        [TestMethod]
        public async Task TestPOCO()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<POCO>();
            for (int i = 0; i < 10; i++)
            {
                POCO cls = new POCO();
                cls.ID = i % 2;
                cls.MyProperty = i + 1;
                cls.Stringss = "dsdsdsds";
                cls.Uniq = i;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();

            var q = await (from  POCO clss in sq
                    where clss.ID == 1
                           select clss).ToListAsync();

            Assert.AreEqual(5, q.Count<POCO>());
            //check ignore work
            Assert.AreEqual(0, q.ToList<POCO>()[0].MyProperty);

            Assert.AreEqual(3, q.ToList<POCO>()[0].Stringss.Length);

            q.ToList<POCO>()[0].Uniq = 0;
            bool except = false;
            try
            {

                await sq.StoreObjectAsync(q.ToList<POCO>()[0]);
            }
            catch (UniqueConstraintException ex)
            {
                except = true;
            }
            Assert.AreEqual(true, except);

        }
        [TestMethod]
        public async Task TestRealPOCO()
        {
            SiaqodbConfigurator.AddIndex("ID", typeof(RealPOCO));
            SiaqodbConfigurator.AddIndex("ID", typeof(RealPOCO1));

            SiaqodbConfigurator.AddUniqueConstraint("UID", typeof(RealPOCO));
            SiaqodbConfigurator.AddUniqueConstraint("UID", typeof(RealPOCO1));

            SiaqodbConfigurator.AddIgnore("ignoredField", typeof(RealPOCO));
            SiaqodbConfigurator.AddIgnore("ignoredField", typeof(RealPOCO1));

            SiaqodbConfigurator.AddIgnore("IgnoredProp", typeof(RealPOCO));
            SiaqodbConfigurator.AddIgnore("IgnoredProp", typeof(RealPOCO1));

            SiaqodbConfigurator.AddMaxLength("MyStr", 3, typeof(RealPOCO));
            SiaqodbConfigurator.AddMaxLength("MyStr", 3, typeof(RealPOCO1));

            SiaqodbConfigurator.AddMaxLength("mystr", 3, typeof(RealPOCO));
            SiaqodbConfigurator.AddMaxLength("mystr", 3, typeof(RealPOCO1));

            SiaqodbConfigurator.PropertyUseField("MyStrProp", "mystr", typeof(RealPOCO));
            SiaqodbConfigurator.PropertyUseField("MyStrProp", "mystr", typeof(RealPOCO1));

            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<RealPOCO>();
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
                await sq.StoreObjectAsync(cls);
            }

            await sq.FlushAsync();
            var q = await (from  RealPOCO clss in sq
                    where clss.ID == 1
                    select clss).ToListAsync();

            Assert.AreEqual(5, q.Count<RealPOCO>());

            sq.Close();

            sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            q = await (from  RealPOCO clss in sq
                where clss.ID == 1
                select clss).ToListAsync();

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
                await sq.StoreObjectAsync(o2);
            }
            catch (UniqueConstraintException ex)
            {
                excp = true;
            }

            Assert.AreEqual(true, excp);

            //check if mapping works
            q = await (from  RealPOCO clss in sq
                where clss.MyStrProp == "dqw"
                       select clss).ToListAsync();

            Assert.AreEqual(10, q.ToList<RealPOCO>().Count);

        }
        //TODO
        [TestMethod]
        public async Task TestOptimisticConcurency()
        {
           /* Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ForConcurencyClass>();
            List<ForConcurencyClass> lis = new List<ForConcurencyClass>();
            for (int i = 0; i < 10; i++)
            {
                ForConcurencyClass c = new ForConcurencyClass();
                c.integ = i + 1;
                c.test = "test";
                await sq.StoreObjectAsync(c);
                lis.Add(c);
            }
            await sq.FlushAsync();
            var q = await (from  ForConcurencyClass cc in sq
                    select cc).ToListAsync();
            ForConcurencyClass de = q.ToList<ForConcurencyClass>()[0];

            de.test = "d";
            await sq.StoreObjectAsync(de);
            int exCatched = 0;
            try
            {
                await sq.StoreObjectAsync(lis[0]);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            await sq.StoreObjectAsync(de);
            await sq.FlushAsync();
            q = await (from  ForConcurencyClass cc in sq
                select cc).ToListAsync();
            ForConcurencyClass de2 = q.ToList<ForConcurencyClass>()[0];

            await sq.StoreObjectAsync(de2);

            ForConcurencyClass newObj = new ForConcurencyClass();
            newObj.integ = 1;

            await sq.UpdateObjectByAsync("integ", newObj);
            exCatched = 0;
            try
            {

                await sq.StoreObjectAsync(de2);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            await sq.StoreObjectAsync(newObj);

            q = await (from  ForConcurencyClass cc in sq
                select cc).ToListAsync();
            ForConcurencyClass de3 = q.ToList<ForConcurencyClass>()[0];

            await sq.DeleteAsync(newObj);

            exCatched = 0;
            try
            {

                await sq.StoreObjectAsync(de3);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            q = await (from  ForConcurencyClass cc in sq
                select cc).ToListAsync();
            ForConcurencyClass de4 = q.ToList<ForConcurencyClass>()[0];

            ForConcurencyClass de4bis = q.ToList<ForConcurencyClass>()[1];

            var q1 = await (from  ForConcurencyClass cc in sq
                            select cc).ToListAsync();

            ForConcurencyClass de5 = q1.ToList<ForConcurencyClass>()[0];

            await sq.StoreObjectAsync(de4);
            await sq.FlushAsync();
            exCatched = 0;
            try
            {

                await sq.DeleteAsync(de5);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);

            ForConcurencyClass de6 = new ForConcurencyClass();
            de6.integ = 3;


            await sq.DeleteObjectByAsync("integ", de6);


            exCatched = 0;
            try
            {

                await sq.StoreObjectAsync(de4bis);
            }
            catch (OptimisticConcurrencyException e)
            {
                exCatched++;
            }
            Assert.AreEqual(1, exCatched);
            */


        }
        [TestMethod]
        public async Task TestTransactionInsert()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();
            bool needRollback = false;
            try
            {

                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.Name = "GTA" + i.ToString();
                    await sq.StoreObjectAsync(c, transact);
                }

                list = await sq.LoadAllAsync<Customer>();
                Assert.AreEqual(0, list.Count);


                await transact.CommitAsync();

            }
            catch (Exception ex)
            {
                needRollback = true;
            }
            if (needRollback)
            {
                await transact.RollbackAsync();
            }
            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(10, list.Count);
            sq.Close();
            await sq.OpenAsync(dbFolder);
            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(10, list.Count);

            needRollback = false;
            transact = sq.BeginTransaction();
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Customer c = new Customer();
                    c.Name = "GTA" + i.ToString();
                    await sq.StoreObjectAsync(c, transact);
                    if (i == 9)
                    {
                        throw new Exception("fsdfsd");
                    }
                }
                await transact.CommitAsync();
            }
            catch (Exception ex)
            {
                needRollback = true;
            }
            if (needRollback)
            {
                await transact.RollbackAsync();
            }
            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(10, list.Count);



        }
        [TestMethod]
        public async Task TestTransactionUpdateInsert()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                await sq.StoreObjectAsync(c);//without transact
            }
            await sq.FlushAsync();
            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(10, list.Count);

            foreach (Customer c in list)
            {
                c.Name = "updated";
                await sq.StoreObjectAsync(c, transact);
            }
            list = await sq.LoadAllAsync<Customer>();
            foreach (Customer c in list)
            {
                Assert.AreEqual("GTA", c.Name.Substring(0, 3));
            }
            bool needRollback = false;
            try
            {
                await transact.CommitAsync();
            }
            catch (Exception ex)
            {
                needRollback = true;  
            }
            if (needRollback)
            {
                await transact.RollbackAsync();//problem with OptimistiConcurency
            }
            list = await sq.LoadAllAsync<Customer>();

            foreach (Customer c in list)
            {
                Assert.AreEqual("updated", c.Name);
            }

        }
        [TestMethod]
        public async Task TestTransactionDelete()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Customer>();
            IList<Customer> list = null;
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                await sq.StoreObjectAsync(c);//without transact
            }
            list = await sq.LoadAllAsync<Customer>();
            await sq.DeleteAsync(list[0], transact);
            await sq.DeleteAsync(list[1], transact);
            bool rollback = false;
            try
            {
                await transact.CommitAsync();
            }
            catch
            {
                 rollback = true;

            }
            if (rollback)
            {
                await transact.RollbackAsync();
                list = await sq.LoadAllAsync<Customer>();
                Assert.AreEqual(10, list.Count);
               
            }
            else
            {
                list = await sq.LoadAllAsync<Customer>();
                Assert.AreEqual(8, list.Count);

            }

        }
        [TestMethod]
        public async Task TestUpdateObjectByManyFieldsTransaction()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            await sq.StoreObjectAsync(emp);


            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "shuhu";

            bool s = await sq.UpdateObjectByAsync(emp, "ID", "CustomerID");

            Assert.IsTrue(s);
            IList<Employee> list = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(list[0].Name, emp.Name);

            emp.Name = "ANOTHER";
            ITransaction tr = sq.BeginTransaction();
            await sq.UpdateObjectByAsync(emp, tr, "ID", "CustomerID");

            await tr.CommitAsync();
            list = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(list[0].Name, emp.Name);

            tr = sq.BeginTransaction();
            emp.Name = "test";

            await sq.UpdateObjectByAsync(emp, tr, "ID", "CustomerID");

            await tr.RollbackAsync();
            list = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(list[0].Name, "ANOTHER");


        }
        [TestMethod]
        public async Task TestDeleteObjectByTransactions()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<Employee>();

            Employee emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            await sq.StoreObjectAsync(emp);


            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;

            ITransaction trans = sq.BeginTransaction();

            bool s = await sq.DeleteObjectByAsync(emp, trans, "ID", "CustomerID");
            Assert.IsTrue(s);
            await trans.CommitAsync();
            IList<Employee> list = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(list.Count, 0);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            await sq.StoreObjectAsync(emp);


            trans = sq.BeginTransaction();
            s = await sq.DeleteObjectByAsync(emp, trans, "ID", "CustomerID");
            await trans.RollbackAsync();

            list = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(list.Count, 1);

            emp = new Employee();
            emp.ID = 100;
            emp.CustomerID = 30;
            emp.Name = "s";
            await sq.StoreObjectAsync(emp);
            await sq.FlushAsync();
            bool needRolback = false;
            trans = sq.BeginTransaction();
            try
            {
                s = await sq.DeleteObjectByAsync(emp, trans, "ID", "CustomerID");


                await trans.CommitAsync();
            }
            catch
            {
                needRolback = true;
            }
            if(needRolback)
                await trans.RollbackAsync();

            list = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(list.Count, 2);


        }
        [TestMethod]
        public async Task TestTransactionCrash()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);

            IList<Customer> list = await sq.LoadAllAsync<Customer>();
            IList<Employee> list2 = await sq.LoadAllAsync<Employee>();

            await sq.DropTypeAsync<Customer>();
            await sq.DropTypeAsync<Employee>();

            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                await sq.StoreObjectAsync(c, transact);
                Employee e = new Employee();
                e.Name = "EMP" + i.ToString();
                await sq.StoreObjectAsync(e, transact);
            }

            await transact.CommitAsync();


            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(10, list.Count);

            list2 = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(10, list2.Count);

            ITransaction transac2t = sq.BeginTransaction();

            await sq.DeleteAsync(list[5], transac2t);
            await sq.DeleteAsync(list2[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updated";
                list2[i].Name = "updatedE";
                await sq.StoreObjectAsync(list[i], transac2t);
                await sq.StoreObjectAsync(list2[i], transac2t);
                await sq.StoreObjectAsync(new Customer(), transac2t);
                await sq.StoreObjectAsync(new Employee(), transac2t);
            }


            await transac2t.CommitAsync();//here do debug and stop after a few commits to be able to simulate crash recovery
        }
        //TODO
        [TestMethod]
        public async Task TestTransactionManyTypes()
        {
          /*  Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);


            await sq.DropTypeAsync<Customer>();
            await sq.DropTypeAsync<Employee>();
            await sq.DropTypeAsync<D40>();
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                await sq.StoreObjectAsync(c, transact);
                Employee e = new Employee();
                e.Name = "EMP" + i.ToString();
                await sq.StoreObjectAsync(e, transact);

                D40 d = new D40();
                await sq.StoreObjectAsync(d, transact);
            }

            await transact.CommitAsync();


            IList<Customer> list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(10, list.Count);

            IList<Employee> list2 = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(10, list2.Count);

            IList<D40> list3 = await sq.LoadAllAsync<D40>();
            Assert.AreEqual(10, list3.Count);

            ITransaction transac2t = sq.BeginTransaction();

            sq.DeleteAsync(list[5], transac2t);
            sq.DeleteAsync(list2[5], transac2t);
            sq.DeleteAsync(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updated";
                list2[i].Name = "updatedE";
                await sq.StoreObjectAsync(list[i], transac2t);
                await sq.StoreObjectAsync(list2[i], transac2t);
                await sq.StoreObjectAsync(new Customer(), transac2t);
                await sq.StoreObjectAsync(new Employee(), transac2t);
            }


            await transac2t.CommitAsync();

            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(13, list2.Count);

            list3 = await sq.LoadAllAsync<D40>();
            Assert.AreEqual(9, list3.Count);
            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");

            transac2t = sq.BeginTransaction();

            sq.DeleteAsync(list[5], transac2t);
            sq.DeleteAsync(list2[5], transac2t);
            sq.DeleteAsync(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updatedRoll";
                list2[i].Name = "updatedERoll";
                await sq.StoreObjectAsync(list[i], transac2t);
                await sq.StoreObjectAsync(list2[i], transac2t);
                await sq.StoreObjectAsync(new Customer(), transac2t);
                await sq.StoreObjectAsync(new Employee(), transac2t);
            }

            await transac2t.RollbackAsync();

            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(13, list2.Count);

            list3 = await sq.LoadAllAsync<D40>();
            Assert.AreEqual(9, list3.Count);

            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");
        */
        }
        [TestMethod]
        public async Task TestTransactionLists()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);


            await sq.DropTypeAsync<Customer>();
            await sq.DropTypeAsync<Employee>();
            await sq.DropTypeAsync<D40WithLists>();
            ITransaction transact = sq.BeginTransaction();

            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer();
                c.Name = "GTA" + i.ToString();
                await sq.StoreObjectAsync(c, transact);
                Employee e = new Employee();
                e.Name = "EMP" + i.ToString();
                await sq.StoreObjectAsync(e, transact);

                D40WithLists d = new D40WithLists();
                d.dt = new List<DateTime>();
                d.dt.Add(DateTime.Now);
                await sq.StoreObjectAsync(d, transact);
            }

            await transact.CommitAsync();


            IList<Customer> list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(10, list.Count);

            IList<Employee> list2 = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(10, list2.Count);

            IList<D40WithLists> list3 = await sq.LoadAllAsync<D40WithLists>();
            Assert.AreEqual(10, list3.Count);

            ITransaction transac2t = sq.BeginTransaction();

            sq.DeleteAsync(list[5], transac2t);
            sq.DeleteAsync(list2[5], transac2t);
            sq.DeleteAsync(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updated";
                list2[i].Name = "updatedE";
                list3[i].dt[0] = new DateTime(2007, 1, 1);
                await sq.StoreObjectAsync(list[i], transac2t);
                await sq.StoreObjectAsync(list2[i], transac2t);
                await sq.StoreObjectAsync(list3[i], transac2t);
                await sq.StoreObjectAsync(new Customer(), transac2t);
                await sq.StoreObjectAsync(new Employee(), transac2t);
                await sq.StoreObjectAsync(new D40WithLists(), transac2t);
            }


            await transac2t.CommitAsync();

            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(13, list2.Count);

            list3 = await sq.LoadAllAsync<D40WithLists>();
            Assert.AreEqual(13, list3.Count);
            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");
            Assert.AreEqual(list3[0].dt[0], new DateTime(2007, 1, 1));

            transac2t = sq.BeginTransaction();

            await sq.DeleteAsync(list[5], transac2t);
            await sq.DeleteAsync(list2[5], transac2t);
            await sq.DeleteAsync(list3[5], transac2t);

            for (int i = 0; i < 4; i++)
            {
                list[i].Name = "updatedRoll";
                list2[i].Name = "updatedERoll";
                list3[i].dt[0] = new DateTime(2008, 3, 3);
                await sq.StoreObjectAsync(list[i], transac2t);
                await sq.StoreObjectAsync(list2[i], transac2t);
                await sq.StoreObjectAsync(new Customer(), transac2t);
                await sq.StoreObjectAsync(new Employee(), transac2t);

                await sq.StoreObjectAsync(list3[i], transac2t);
            }

            await transac2t.RollbackAsync();

            list = await sq.LoadAllAsync<Customer>();
            Assert.AreEqual(13, list.Count);

            list2 = await sq.LoadAllAsync<Employee>();
            Assert.AreEqual(13, list2.Count);

            int tttt=2;
            list3 = await sq.LoadAllAsync<D40WithLists>();
            Assert.AreEqual(13, list3.Count);

            Assert.AreEqual(list[0].Name, "updated");
            Assert.AreEqual(list2[0].Name, "updatedE");
            Assert.AreEqual(list3[0].dt[0], new DateTime(2007, 1, 1));

        }
        [TestMethod]
        public async Task TestIndexStringStartWith()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<ClassIndexesString>();
            ClassIndexesString cls = new ClassIndexesString() { Name = "aaa" };
            await sq.StoreObjectAsync(cls);

            cls = new ClassIndexesString() { Name = "bbabyb" };
            await sq.StoreObjectAsync(cls);

            cls = new ClassIndexesString() { Name = "bba" };
            await sq.StoreObjectAsync(cls);

            cls = new ClassIndexesString() { Name = "bbazz" };
            await sq.StoreObjectAsync(cls);

            cls = new ClassIndexesString() { Name = "ab" };
            await sq.StoreObjectAsync(cls);

            cls = new ClassIndexesString() { Name = "rere" };
            await sq.StoreObjectAsync(cls);
            cls = new ClassIndexesString() { Name = "abbb" };
            await sq.StoreObjectAsync(cls);

            await sq.FlushAsync();
            //sq.Close();
            //sq = new Siaqodb(objPath);
            var q = await (from  ClassIndexesString clss in sq
                     where clss.Name.StartsWith("bb")
                     select clss).ToListAsync();

            Assert.AreEqual(3, q.Count);
            foreach (var hu in q)
            {
                Assert.IsTrue(hu.Name.StartsWith("bb"));
            }

        }
        [TestMethod]
        public async Task TestListsAllTypes()
        {
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            await sq.DropTypeAsync<D40WithLists>();

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


                await sq.StoreObjectAsync(d);
            }
            await sq.FlushAsync();
            IObjectList<D40WithLists> all1 = await sq.LoadAllAsync<D40WithLists>();
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

            var q21 = await (from  D40WithLists dll in sq
                       where dll.g.Contains(guid)
                       select dll).ToListAsync();

            Assert.AreEqual(10, q21.Count);
        }
        [TestMethod]
        public async Task TestOpen2Databases()
        {
           /* Siaqodb s1 = new Siaqodb(@"G:\demo\s1\");
            s1.DropTypeAsync<POCO>();

            for (int i = 0; i < 10; i++)
            {
                POCO pp = new POCO();
                pp.Uniq = i;
                s1.StoreObjectAsync(pp);
            }
            s1.FlushAsync();

            Siaqodb s2 = new Siaqodb(@"G:\demo\s2\");

            IList<POCO> poc1 = s1.LoadAllAsync<POCO>();

            Assert.AreEqual(10, poc1.Count);
            IList<POCO> poc2 = s2.LoadAllAsync<POCO>();

            Assert.AreEqual(0, poc2.Count);
            */
        }
        [TestMethod]
        public async Task TestLoadingEvents()
        {
            //SiaqodbConfigurator.SetRaiseLoadEvents(true);
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);
            sq.LoadingObject += new EventHandler<LoadingObjectEventArgs>(sq_LoadingObject);
            sq.LoadedObject += new EventHandler<LoadedObjectEventArgs>(sq_LoadedObject);
            await sq.DropTypeAsync<POCO>();
            for (int i = 0; i < 10; i++)
            {
                POCO cls = new POCO();
                cls.ID = i % 2;
                cls.MyProperty = i + 1;
                cls.Stringss = "dsdsdsds";
                cls.Uniq = i;
                await sq.StoreObjectAsync(cls);
            }
            await sq.FlushAsync();
            IList<POCO> all = await sq.LoadAllAsync<POCO>();

        }

        void sq_LoadedObject(object sender, LoadedObjectEventArgs e)
        {

        }

        void sq_LoadingObject(object sender, LoadingObjectEventArgs e)
        {

        }

        [TestMethod]
        public async Task TestNestedSelfObject()
        {
            //SiaqodbConfigurator.SetRaiseLoadEvents(true);
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);

            await sq.DropTypeAsync<Person>();
            for (int i = 0; i < 10; i++)
            {
                Person p = new Person();
                p.Name = i.ToString();
                p.friend = new Person();
                p.friend.Name = (i + 10).ToString();
                await sq.StoreObjectAsync(p);
            }
            await sq.FlushAsync();
            IList<Person> all = await sq.LoadAllAsync<Person>();
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
        public async Task TestDateTimeKind()
        {
            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Utc);
            Siaqodb sq = new Siaqodb(); await sq.OpenAsync(dbFolder);

            await sq.DropTypeAsync<D40>();

            D40 p = new D40();
            p.dt = DateTime.Now;
            await sq.StoreObjectAsync(p);
            await sq.FlushAsync();
            IList<D40> lis = await sq.LoadAllAsync<D40>();
            Assert.AreEqual(DateTimeKind.Utc, lis[0].dt.Kind);

            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Local);
            p = new D40();
            p.dt = DateTime.Now;
            await sq.StoreObjectAsync(p);
            await sq.FlushAsync();

            lis = await sq.LoadAllAsync<D40>();
            Assert.AreEqual(DateTimeKind.Local, lis[0].dt.Kind);
            Assert.AreEqual(DateTimeKind.Local, lis[1].dt.Kind);

            SiaqodbConfigurator.SpecifyStoredDateTimeKind(null);
            p = new D40();
            p.dt = DateTime.Now;
            await sq.StoreObjectAsync(p);
            await sq.FlushAsync();

            lis = await sq.LoadAllAsync<D40>();
            Assert.AreEqual(DateTimeKind.Unspecified, lis[0].dt.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, lis[1].dt.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, lis[2].dt.Kind);

        }
        [TestMethod]
        public async Task TestShrink()
        {
            Siaqodb sq = new Siaqodb(dbFolder);
            await sq.DropTypeAsync<D40WithLists>();

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

                await sq.StoreObjectAsync(d);
            }

            IObjectList<D40WithLists> all = await sq.LoadAllAsync<D40WithLists>();
            for (int i = 5; i < 10; i++)
            {
                await sq.DeleteAsync(all[i]);
            }
            await sq.CloseAsync();

            await SiaqodbUtil.ShrinkAsync(dbFolder, ShrinkType.Normal);
            await SiaqodbUtil.ShrinkAsync(dbFolder, ShrinkType.ForceClaimSpace);

            sq = new Siaqodb(dbFolder);
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

                await sq.StoreObjectAsync(d);
            }
            IObjectList<D40WithLists> all1 = await sq.LoadAllAsync<D40WithLists>();


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

            var q21 = await (from D40WithLists dll in sq
                       where dll.g.Contains(guid)
                       select dll).ToListAsync();

            Assert.AreEqual(15, q21.Count);
        }
        [TestMethod]
        public async Task TestIndexShrink()
        {
            Siaqodb sq = new Siaqodb(dbFolder);
            await sq.DropTypeAsync<D40WithIndexes>();

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


                await sq.StoreObjectAsync(d);
            }
            await sq.DropTypeAsync<ClassIndexes>();
            for (int i = 0; i < 100; i++)
            {
                ClassIndexes cls = new ClassIndexes();
                cls.one = i % 10;
                cls.two = i % 10 + 1;
                cls.ID = i;
                cls.ID2 = i;
                await sq.StoreObjectAsync(cls);
            }
            IList<D40WithIndexes> all30 = await sq.LoadAllAsync<D40WithIndexes>();
            for (int i = 5; i < 10; i++)
            {
                await sq.DeleteAsync(all30[i]);
            }
            await sq.CloseAsync();

            await SiaqodbUtil.ShrinkAsync(dbFolder, ShrinkType.Normal);
            await SiaqodbUtil.ShrinkAsync(dbFolder, ShrinkType.ForceClaimSpace);

            sq = new Siaqodb(dbFolder);
            byte byt = 3;
            var q1 =await  (from D40WithIndexes di in sq
                     where di.b == byt
                     select di).ToListAsync();

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
                if (1 == 2)
                {
                    return null;
                }
                return mystr;
            }
            set
            {
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

        public int OID { get; set; }

        public int IgnoredProp { get; set; }
        public int ignoredField;


        public string MyStr { get; set; }
        public string mystr;

        public string MyStrProp
        {
            get
            {
                if (1 == 2)
                {
                    return null;
                }
                return mystr;
            }
            set
            {
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

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
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
    public class ClassIndexesString : ISqoDataObject
    {
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        [Index]
        public string Name;
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
        private ulong tickCount;
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
        private ulong tickCount;

    }
    public class ItemUnique : ISqoDataObject
    {
        [Sqo.Attributes.UniqueConstraint]
        public int Age;

        [UniqueConstraint]
        public string S;

        public int integ;
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;
    }
    public class Employee : SqoDataObject
    {
        [MaxLength(20)]
        public string Name;
        public int ID;
        public int CustomerID;
        public string ENameProp { get { return Name; } }

        //private int oid;
        //public int OID
        //{
        //    get { return oid; }
        //    set { oid = value; }
        //}
        private ulong tickCount;
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
        private ulong tickCount;
    }
    public class Customer
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
            return s == "ADH3";
        }
        public string stringWithoutAtt;

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;

    }
    public class CustomerLite : SqoDataObject
    {
        public string Name { get; set; }
        public int Age { get; set; }
        bool active = true;
        [UseVariable("active")]
        public bool Active { get { return active; } set { active = value; } }
        public TestEnum TEnum { get; set; }
        private ulong tickCount;

        //private int oid;
        //public int OID
        //{
        //    get { return oid; }
        //    set { oid = value; }
        //}
    }
    public enum TestEnum { Unu, Doi, Trei }
    public class Order : ISqoDataObject
    {
        [MaxLength(20)]
        public string Name;
        public int ID;
        public int EmployeeID;

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
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
    public class Something32 : ISqoDataObject
    {
        public int one;

        public long three;
        public int three1;
        public int three2;

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;


    }
    public class EmpCustOID
    {
        public string CName;
        public string EName;
        public int EOID;
        private ulong tickCount;
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
        public DateTimeOffset dtsofs;
        public DateTime dt;
        public Guid g;
        public myEnum enn = myEnum.doi;
        [Text]
        public string Text;
        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;
    }
    public enum myEnum { unu = 2, doi };

    public class ForConcurencyClass
    {
        public int integ;
        public string test;
        private ulong tickCount;

        public int OID { get; set; }

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

        [Text]
        [Index]
        public string Text = "text longgg";

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
        private ulong tickCount;
    }
    public class Person
    {
        public int OID { get; set; }
        public string Name;
        public Person friend;

    }
}
