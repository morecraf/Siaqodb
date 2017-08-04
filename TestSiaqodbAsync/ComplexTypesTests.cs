using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Sqo;
using System.Threading.Tasks;
using Sqo.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SiaqodbUnitTests
{
    [TestClass]
    public class ComplexTypesTests
    {
        string dbFolder = @"D:\morecraf\temp\SqoUnitTests\";
        public ComplexTypesTests()
        {
            SiaqodbConfigurator.EncryptedDatabase = false;
            Sqo.SiaqodbConfigurator.SetLicense(@"VpMKWZsHgtvrUfEPCu0WDGLk5nlVs2+5yN8youWUSixTKvmLnjsVUq9r9kdfFMuCMGtT9uyrBHNQAv+V2KkxOg==");
        }
        [TestMethod]
        public async Task TestStore()
        {
            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<A>();
            await s_db.DropTypeAsync<B>();
            await s_db.DropTypeAsync<C>();
            for (int i = 0; i < 10; i++)
            {
                A a = new A();
                a.aId = i;
                a.BVar = new B();
                a.BVar.bId = 11;
                a.BVar.Ci = new C();
                a.BVar.Ci.ACircular = a;
                a.BVar.Ci.cId = i;
                try
                {
                    await s_db.StoreObjectAsync(a);
                }
                catch (Exception ex)
                { 
                
                }
            }
            await s_db.FlushAsync();

            IList<A> allA = await s_db.LoadAllAsync<A>();
            Assert.AreEqual(10, allA.Count);
            Assert.AreEqual(5, allA[5].BVar.Ci.cId);
            Assert.AreEqual(11, allA[2].BVar.bId);
            Assert.AreEqual(5, allA[5].aId);

            IList<B> allB = await s_db.LoadAllAsync<B>();
            Assert.AreEqual(10, allB.Count);
            Assert.AreEqual(5, allB[5].Ci.cId);
            Assert.AreEqual(11, allB[2].bId);

            IList<C> allC = await s_db.LoadAllAsync<C>();
            Assert.AreEqual(10, allC.Count);
            Assert.AreEqual(5, allC[5].cId);
            Assert.AreEqual(11, allC[2].ACircular.BVar.bId);
            Assert.AreEqual(5, allC[5].ACircular.aId);

            allA[0].aId = 100;
            allA[0].BVar.bId = 100;
            allA[0].BVar.Ci.cId = 100;
            await s_db.StoreObjectAsync(allA[0]);
            await s_db.FlushAsync();

            IList<A> allA1 = await s_db.LoadAllAsync<A>();
            Assert.AreEqual(100, allA1[0].aId);
            Assert.AreEqual(100, allA1[0].BVar.bId);
            Assert.AreEqual(100, allA1[0].BVar.Ci.cId);

            allC[1].cId = 200;
            await s_db.StoreObjectAsync(allC[1]);
            await s_db.FlushAsync();
            IList<A> allA2 = await s_db.LoadAllAsync<A>();
            Assert.AreEqual(200, allA2[1].BVar.Ci.cId);

        }
        [TestMethod]
        public async Task TestRead()
        {
            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<A>();
            await s_db.DropTypeAsync<B>();
            await s_db.DropTypeAsync<C>();
            for (int i = 0; i < 10; i++)
            {
                A a = new A();
                a.aId = i;
                a.BVar = new B();
                a.BVar.bId = 11;
                a.BVar.Ci = new C();
                a.BVar.Ci.ACircular = a;
                a.BVar.Ci.cId = i;
                await s_db.StoreObjectAsync(a);
            }
            await s_db.FlushAsync();
            var q = await  (from A a in s_db
                     where a.BVar.bId == 11 && a.BVar.Ci.cId == 5
                     select a).ToListAsync();

            Assert.AreEqual(1, q.Count);
            Assert.AreEqual(5, q[0].BVar.Ci.cId);

            var q1 = await (from A a in s_db
                      where a.aId == 5
                      select a.BVar).ToListAsync();
            Assert.AreEqual(1, q1.Count);
            Assert.AreEqual(5, q1[0].Ci.cId);

            var q2 = await (from A a in s_db
                      where a.aId == 5
                      select new { bVar = a.BVar, cVar = a.BVar.Ci }).ToListAsync();
            Assert.AreEqual(1, q2.Count);
            Assert.AreEqual(5, q2[0].cVar.cId);

            
        }
        [TestMethod]
        public async Task TestTransaction()
        {
            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<A>();
            await s_db.DropTypeAsync<B>();
            await s_db.DropTypeAsync<C>();
            ITransaction transaction = s_db.BeginTransaction();
            for (int i = 0; i < 10; i++)
            {
                A a = new A();
                a.aId = i;
                a.BVar = new B();
                a.BVar.bId = 11;
                a.BVar.Ci = new C();
                a.BVar.Ci.ACircular = a;
                a.BVar.Ci.cId = i;
                await s_db.StoreObjectAsync(a,transaction);
            }
            await transaction.CommitAsync();

            IList<A> allA = await s_db.LoadAllAsync<A>();
            Assert.AreEqual(10, allA.Count);
            Assert.AreEqual(5, allA[5].BVar.Ci.cId);
            Assert.AreEqual(11, allA[2].BVar.bId);
            Assert.AreEqual(5, allA[5].aId);

            IList<B> allB = await s_db.LoadAllAsync<B>();
            Assert.AreEqual(10, allB.Count);
            Assert.AreEqual(5, allB[5].Ci.cId);
            Assert.AreEqual(11, allB[2].bId);

            IList<C> allC = await s_db.LoadAllAsync<C>();
            Assert.AreEqual(10, allC.Count);
            Assert.AreEqual(5, allC[5].cId);
            Assert.AreEqual(11, allC[2].ACircular.BVar.bId);
            Assert.AreEqual(5, allC[5].ACircular.aId);

            allA[0].aId = 100;
            allA[0].BVar.bId = 100;
            allA[0].BVar.Ci.cId = 100;
           
            ITransaction transaction1 = s_db.BeginTransaction();
            await s_db.StoreObjectAsync(allA[0],transaction1);
            await transaction1.CommitAsync();

            IList<A> allA1 = await s_db.LoadAllAsync<A>();
            Assert.AreEqual(100, allA1[0].aId);
            Assert.AreEqual(100, allA1[0].BVar.bId);
            Assert.AreEqual(100, allA1[0].BVar.Ci.cId);

            allC[1].cId = 200;
            ITransaction transaction2 = s_db.BeginTransaction();
            
            await s_db.StoreObjectAsync(allC[1],transaction2);
            s_db.DeleteAsync(allA1[9],transaction2);
            s_db.DeleteAsync(allA1[8].BVar, transaction2);
            await transaction2.CommitAsync();
            IList<A> allA2 = await s_db.LoadAllAsync<A>();
            IList<B> allB2 = await s_db.LoadAllAsync<B>();
            IList<C> allC2 = await s_db.LoadAllAsync<C>();
            
            Assert.AreEqual(200, allA2[1].BVar.Ci.cId);
            Assert.AreEqual(9, allA2.Count);
            Assert.AreEqual(9, allB2.Count);
            Assert.AreEqual(10, allC2.Count);


        }
        [TestMethod]
        public async Task TestInclude()
        {
            SiaqodbConfigurator.LoadRelatedObjects<A>(false);
            SiaqodbConfigurator.LoadRelatedObjects<B>(false);
            try
            {
                Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
                await s_db.DropTypeAsync<A>();
                await s_db.DropTypeAsync<B>();
                await s_db.DropTypeAsync<C>();
                for (int i = 0; i < 10; i++)
                {
                    A a = new A();
                    a.aId = i;
                    a.BVar = new B();
                    a.BVar.bId = 11;
                    a.BVar.Ci = new C();
                    a.BVar.Ci.ACircular = a;
                    a.BVar.Ci.cId = i;
                    await s_db.StoreObjectAsync(a);
                }
                await s_db.FlushAsync();
                IList<A> allA = await s_db.LoadAllAsync<A>();
                IList<B> allB = await s_db.LoadAllAsync<B>();
                for (int i = 0; i < 10; i++)
                {
                    Assert.IsNull(allA[i].BVar);
                    Assert.IsNull(allB[i].Ci);
                }
                var q = await (s_db.Cast<A>().Where(a => a.OID > 5).Include("BVar")).ToListAsync();

                foreach (A a in q)
                {
                    Assert.IsNotNull(a.BVar);
                    Assert.IsNull(a.BVar.Ci);
                }
                var q1 = await (s_db.Cast<A>().Where(a => a.OID > 5).Include("BVar").Include("BVar.Ci")).ToListAsync();

                foreach (A a in q1)
                {
                    Assert.IsNotNull(a.BVar);
                    Assert.IsNotNull(a.BVar.Ci);
                }
                var q2 = await (s_db.Cast<A>().Where(a => a.OID > 5).Include("BVar")).ToListAsync();

                foreach (A a in q2)
                {
                    Assert.IsNotNull(a.BVar);
                    Assert.IsNull(a.BVar.Ci);
                }
            }
            finally
            {
                SiaqodbConfigurator.LoadRelatedObjects<A>(true);
                SiaqodbConfigurator.LoadRelatedObjects<B>(true);
            }
        }
        [TestMethod]
        public async Task TestComplexLists()
        {
            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<TapRecord>();
            await s_db.DropTypeAsync<D>();
          
            for (int i = 0; i < 10; i++)
            {
                D d = new D();
                d.tap = new TapRecord();
                d.tap2 = new TapRecord() { userName = "newelist" };
                d.TapList = new List<TapRecord>();
                d.TapList.Add(d.tap);
                d.TapList.Add(new TapRecord());
                d.TapList2.Add(new TapRecord() { userName = "newelist" });
                await s_db.StoreObjectAsync(d);
            }
            await s_db.FlushAsync();
            IList<D> dlis = await s_db.LoadAllAsync<D>();
            IList<TapRecord> dtap = await s_db.LoadAllAsync<TapRecord>();

            Assert.AreEqual(10, dlis.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(2, dlis[i].TapList.Count);
                Assert.AreEqual(1, dlis[i].TapList2.Count);

                Assert.AreEqual(dlis[i].tap.OID, dlis[i].TapList[0].OID);
                Assert.AreEqual("newelist", dlis[i].TapList2[0].userName);
            }

            var q = await (from D d in s_db
                    where d.TapList2.Contains(new TapRecord() { userName = "newelist" })
                    select d).ToListAsync();
            Assert.AreEqual(10, q.Count);

            var q2 = await (from D d in s_db
                     where d.tap==new TapRecord() && d.tap2==new TapRecord() { userName = "newelist" }
                     select d).ToListAsync();
            Assert.AreEqual(10, q2.Count);
            
        }
        [TestMethod]
        public async Task TestWhereComplexObjectCompare()
        {
            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<TapRecord>();
            await s_db.DropTypeAsync<D>();

            for (int i = 0; i < 10; i++)
            {
                D d = new D();
                d.tap = new TapRecord();
                d.tap2 = new TapRecord() { userName = "newelist" };
                d.TapList = new List<TapRecord>();
                d.TapList.Add(d.tap);
                d.TapList.Add(new TapRecord());
                d.TapList2.Add(new TapRecord() { userName = "newelist" });
                await s_db.StoreObjectAsync(d);
            }
            await s_db.FlushAsync();
            var q = await (from D d in s_db
                     where d.tap2 == new TapRecord { userName = "newelist" }
                     select d).ToListAsync();
            Assert.AreEqual(10, q.Count);

           
        }
        [TestMethod]
        public async Task TestDeleteNestedObject()
        {
            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<A>();
            await s_db.DropTypeAsync<B>();
            await s_db.DropTypeAsync<C>();
            for (int i = 0; i < 10; i++)
            {
                A a = new A();
                a.aId = i;
                a.BVar = new B();
                a.BVar.bId = 11;
                a.BVar.Ci = new C();
                a.BVar.Ci.ACircular = a;
                a.BVar.Ci.cId = i;
                await s_db.StoreObjectAsync(a);
            }
            await s_db.FlushAsync();
            var q = await (from A a in s_db
                     where a.BVar.bId == 11 && a.BVar.Ci.cId == 5
                     select a).ToListAsync();

            Assert.AreEqual(1, q.Count);
           
            await s_db.DeleteAsync(q[0].BVar.Ci);

            await s_db.FlushAsync();
            q = await (from A a in s_db
                 where a.BVar.bId == 11 && a.BVar.Ci.cId == 5
                 select a).ToListAsync();

            Assert.AreEqual(0, q.Count);
            q = await (from A a in s_db
                 where a.BVar.bId == 11 
                 select a).ToListAsync();

            Assert.AreEqual(10, q.Count);
            Assert.IsNull(q[5].BVar.Ci);

            IList<A> lsA = await s_db.LoadAllAsync<A>();
            await s_db.DeleteAsync(lsA[0].BVar);

            await s_db.FlushAsync();
            IList<C> lsC = await s_db.LoadAllAsync<C>();
            IList<B> lsB = await s_db.LoadAllAsync<B>();
            IList<A> lsA1 = await s_db.LoadAllAsync<A>();
            
            Assert.AreEqual(9, lsC.Count);
            Assert.AreEqual(9, lsB.Count);
            Assert.AreEqual(10, lsA1.Count);

        }
        [TestMethod]
        public async Task TestListOfLists()
        {

            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<MyList<int>>();
            for (int i = 0; i < 10; i++)
            {
                MyList<int> myList = new MyList<int>();
                myList.TheList = new List<ListContainer<int>>();
                ListContainer<int> innerList = new ListContainer<int>();
                innerList.List = new List<int>();
                innerList.List.Add(i);
                innerList.List.Add(i+1);
                myList.TheList.Add(innerList);
                await s_db.StoreObjectAsync(myList);
            }
            await s_db.FlushAsync();
            s_db.Close();
            s_db =new Siaqodb(); await s_db.OpenAsync(dbFolder);
            IList<MyList<int>> list=  await s_db.LoadAllAsync<MyList<int>>();
            Assert.AreEqual(10, list.Count);
            Assert.AreEqual(2, list[1].TheList[0].List.Count);
        }
        [TestMethod]
        public async Task TestStorePartialNull()
        {
            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<A>();
            await s_db.DropTypeAsync<B>();
            await s_db.DropTypeAsync<C>();
            for (int i = 0; i < 10; i++)
            {
                A a = new A();
                a.aId = i;
                a.BVar = new B();
                a.BVar.bId = 11;
                a.BVar.Ci = new C();
                a.BVar.Ci.ACircular = a;
                a.BVar.Ci.cId = i;
                await s_db.StoreObjectAsync(a);
            }
            await s_db.FlushAsync();
            IList<A> lsA = await s_db.LoadAllAsync<A>();
            lsA[0].BVar.Ci = null;
            await s_db.StoreObjectPartiallyAsync(lsA[0].BVar, "Ci");
            await s_db.FlushAsync();
            IList<A> lsA1 = await s_db.LoadAllAsync<A>();
            Assert.IsNull(lsA1[0].BVar.Ci);
        }
        [TestMethod]
        public async Task TestStorePartialOnIndexed()
        {
            SiaqodbConfigurator.AddIndex("cId", typeof(C));

            Siaqodb s_db = new Siaqodb(); await s_db.OpenAsync(dbFolder);
            await s_db.DropTypeAsync<A>();
            await s_db.DropTypeAsync<B>();
            await s_db.DropTypeAsync<C>();
            for (int i = 0; i < 10; i++)
            {
                A a = new A();
                a.aId = i;
                a.BVar = new B();
                a.BVar.bId = 11;
                a.BVar.Ci = new C();
                a.BVar.Ci.ACircular = a;
                a.BVar.Ci.cId = i%2;
                await s_db.StoreObjectAsync(a);
            }
            await s_db.FlushAsync();
            IList<A> lsA = await s_db.LoadAllAsync<A>();
            lsA[0].BVar.Ci.cId = 3;
            await s_db.StoreObjectPartiallyAsync(lsA[0].BVar, "Ci");
            var q = await (from C c in s_db
                     where c.cId == 3
                     select c).ToListAsync();
            Assert.AreEqual(1, q.Count);

        }
    }
    public class A
    {
        public int OID { get; set; }
        public B BVar { get; set; }
        public int aId;
    }
    public class C
    {
        public int OID { get; set; }
        public int cId;
        public A ACircular;
    }
    public class BB
    {
        public int OID { get; set; }
        public int bId;
        public C Ci { get; set; }
    }
    public class B:BB
    {
        public int bInt; 
    }
    public class TapRecord
    {
        public string userName;
        public int TotalScore;
        public int OID { get; set; }

        public async Task AddScore(int ballType)
        {
            TotalScore++;
        }

    }
    public class D
    {
        public int OID { get; set; }
        public TapRecord tap;
        public List<TapRecord> TapList;
        public int test = 3;
        public TapRecord tap2 = new TapRecord() { userName = "neww" };
        public List<TapRecord> TapList2 = new List<TapRecord>();
    }
    public class ListContainer<T>
    {
        public int OID { get; set; }
        public List<T> List;
    }
    public class MyList<T>
    {
        public int OID { get; set; }
        public List<ListContainer<T>> TheList;
    }
}