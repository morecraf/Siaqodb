using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo;
using System.Diagnostics;
using Sqo.Attributes;
using System.Collections.Generic;

namespace MigrationTest
{
    [TestClass]
    public class UnitTest1
    {
        string objPath = @"c:\work\temp\unitTests_siaqodb\";

        public UnitTest1()
		{
            SiaqodbConfigurator.EncryptedDatabase = true;
           // SiaqodbConfigurator.VerboseLevel = VerboseLevel.Info;
            SiaqodbConfigurator.LoggingMethod = this.LogWarns;
            Sqo.SiaqodbConfigurator.SetLicense(@"yxnKYjifYu4lxir+r3sAvbdGwzlWvHc+mp1HAwjsdFCQVKbN0DMQOFAxApzCKVL6");
		}
        public void LogWarns(string log, VerboseLevel level)
        {
            Debug.WriteLine(log);
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

		[TestMethod]
		public void TestInsert()
		{
            using (Dotissi.Siaqodb nop = new Dotissi.Siaqodb(objPath))
            {
                nop.DropType<classA>();
                nop.DropType<classB>();

                for (int i = 10; i < 20; i++)
                {
                    classB b = new classB();
                    b.Name = "BDH" + i.ToString();
                    //c.Vasiel = "momo" + i.ToString();
                    nop.StoreObject(b);

                    D d = new D();
                    nop.StoreObject(d);

                    D40WithIndexes d40 = new D40WithIndexes();
                    d40.bo = true;
                    d40.c = (i + "")[0];
                    d40.d = i + 0.4;
                    d40.dt = new DateTime();
                    d40.f = i + 0.3f;
                    d40.g = new Guid();
                    d40.i = i;
                    d40.ID = 10 - i;
                    d40.iu = (uint)i;
                    d40.l = i + 0L;
                    d40.s = (short)i;
                    d40.str = i + "str";
                    d40.Text = i + "text";
                    d40.ul = (ulong)i;
                    d40.us = (ushort)i;

                    nop.StoreObject(d40);
                }

                for (int i = 10; i < 20; i++)
                {
                    classA a = new classA();

                    classB b = new classB();
                    b.Name = "ADH" + i.ToString();
                    a.B = b;

                    D d = new D();
                    d.tap = new TapRecord
                    {
                        TotalScore = i,
                        userName = "user name",
                        A = a
                    };
                    a.D = d;

                    D40WithIndexes d40 = new D40WithIndexes();
                    d40.bo = true;
                    d40.c = (i + "")[0];
                    d40.d = i + 0.4;
                    d40.dt = new DateTime();
                    d40.f = i + 0.3f;
                    d40.g = new Guid();
                    d40.i = i;
                    d40.ID = i;
                    d40.iu = (uint)i;
                    d40.l = i + 0L;
                    d40.s = (short)i;
                    d40.str = i + "str";
                    d40.Text = i + "text";
                    d40.ul = (ulong)i;
                    d40.us = (ushort)i;

                    a.D40 = d40;
                    //c.Vasiel = "momo" + i.ToString();
                    nop.StoreObject(a);
                }
                nop.Flush();
                IObjectList<classA> listC = nop.LoadAll<classA>();
                Assert.AreEqual(listC.Count, 10);
            }

            using (Siaqodb nop = new Siaqodb(objPath))
            {
                SiaqodbUtil.Migrate(nop);
                var all = nop.LoadAll<classA>();
                var allB = nop.LoadAll<classB>();
                var allD = nop.LoadAll<D40WithIndexes>();
            }
		}
    }

    public class classA{
        public classB B {get;set;}

        public D40WithIndexes D40 { get; set; }
        public D D { get; set; }
    }
    public class classB {
        [UniqueConstraint]
        public string Name{get;set;}
    }

    public class TapRecord
    {
        public string userName;

        public classA A { get; set; }

        public int TotalScore;
        public int OID { get; set; }

        public void AddScore(int ballType)
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

    public class D40WithIndexes
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
      

        [Text]
        [Index]
        public string Text = "text longgg";


        private ulong tickCount;
    }
}
