using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqo;
using Sqo.Transactions;

namespace TestSiaqodb
{
    /// <summary>
    /// Summary description for JaggedTests
    /// </summary>
    [TestClass]
    public class JaggedTests
    {
        string objPath = @"jaggedTypes";
        public JaggedTests()
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
        public void TestMethod1()
        {
            //
            // TODO: Add test logic here
            //
        }
        [TestMethod]
        public void TestStoreSimpleJagged()
        {
            Siaqodb s_db = new Siaqodb(objPath);

          
            s_db.DropType<JaggedTy>();
            for (int i = 0; i < 10; i++)
            {
                JaggedTy jg = new JaggedTy();
                jg.jaggedByte = new byte[2][];
                jg.jaggedByte[0] = new byte[2];
                jg.jaggedByte[0][0] = (byte)i;
                jg.jaggedByte[0][1] = (byte)(i + 1);
                jg.jaggedByte[1] = new byte[2];
                jg.jaggedByte[1][0] = (byte)(i + 2);
                jg.jaggedByte[1][1] = (byte)(i + 3);

                jg.jaggedInt = new int[2][];
                jg.jaggedInt[0] = new int[2];
                jg.jaggedInt[0][0] = i;
                jg.jaggedInt[0][1] = (i + 1);
                jg.jaggedInt[1] = new int[2];
                jg.jaggedInt[1][0] = (i + 2);
                jg.jaggedInt[1][1] = (i + 3);

                jg.jaggedList = new List<List<int>>();
                List<int> myList = new List<int>();
                myList.Add(i);
                myList.Add(i + 1);
                List<int> myList2 = new List<int>();
                myList2.Add(i * 10);
                myList2.Add(i * 100);
                jg.jaggedList.Add(myList);
                jg.jaggedList.Add(myList2);

                jg.jaggedListStr = new List<List<string>>();
                List<string> myListStr = new List<string>();
                myListStr.Add("ws" + i.ToString());
                myListStr.Add("second" + i.ToString());
                jg.jaggedListStr.Add(myListStr);


                s_db.StoreObject(jg);
            }
           IList<JaggedTy>  all = s_db.LoadAll<JaggedTy>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual((byte)i, all[i].jaggedByte[0][0]);
                Assert.AreEqual((byte)(i + 1), all[i].jaggedByte[0][1]);
                Assert.AreEqual((byte)(i + 2), all[i].jaggedByte[1][0]);
                Assert.AreEqual((byte)(i + 3), all[i].jaggedByte[1][1]);

                Assert.AreEqual(i, all[i].jaggedInt[0][0]);
                Assert.AreEqual((i + 1), all[i].jaggedInt[0][1]);
                Assert.AreEqual((i + 2), all[i].jaggedInt[1][0]);
                Assert.AreEqual((i + 3), all[i].jaggedInt[1][1]);

                Assert.AreEqual(2, all[i].jaggedList[0].Count);
                Assert.AreEqual(2, all[i].jaggedList.Count);
                Assert.AreEqual(i, all[i].jaggedList[0][0]);
                Assert.AreEqual(i + 1, all[i].jaggedList[0][1]);
                Assert.AreEqual(i * 10, all[i].jaggedList[1][0]);
                Assert.AreEqual(i * 100, all[i].jaggedList[1][1]);

                Assert.AreEqual(1, all[i].jaggedListStr.Count);
                Assert.AreEqual(2, all[i].jaggedListStr[0].Count);
                Assert.AreEqual("ws" + i, all[i].jaggedListStr[0][0]);
                Assert.AreEqual("second" + i, all[i].jaggedListStr[0][1]);

            }
            s_db.Close();
            s_db = new Siaqodb(objPath);
            all = s_db.LoadAll<JaggedTy>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual((byte)i, all[i].jaggedByte[0][0]);
                Assert.AreEqual((byte)(i + 1), all[i].jaggedByte[0][1]);
                Assert.AreEqual((byte)(i + 2), all[i].jaggedByte[1][0]);
                Assert.AreEqual((byte)(i + 3), all[i].jaggedByte[1][1]);

                Assert.AreEqual(i, all[i].jaggedInt[0][0]);
                Assert.AreEqual((i + 1), all[i].jaggedInt[0][1]);
                Assert.AreEqual((i + 2), all[i].jaggedInt[1][0]);
                Assert.AreEqual((i + 3), all[i].jaggedInt[1][1]);

                Assert.AreEqual(2, all[i].jaggedList[0].Count);
                Assert.AreEqual(2, all[i].jaggedList.Count);
                Assert.AreEqual(i, all[i].jaggedList[0][0]);
                Assert.AreEqual(i + 1, all[i].jaggedList[0][1]);
                Assert.AreEqual(i * 10, all[i].jaggedList[1][0]);
                Assert.AreEqual(i * 100, all[i].jaggedList[1][1]);

                Assert.AreEqual(1, all[i].jaggedListStr.Count);
                Assert.AreEqual(2, all[i].jaggedListStr[0].Count);
                Assert.AreEqual("ws" + i, all[i].jaggedListStr[0][0]);
                Assert.AreEqual("second" + i, all[i].jaggedListStr[0][1]);

            }
            var q = from JaggedTy jgd in s_db
                    where jgd.jaggedInt.Length == 2
                    select jgd;

            Assert.AreEqual(10, q.ToList().Count);

            List<int> myInList = new List<int>();
            myInList.Add(0);
            myInList.Add(1);

            var q2 = from JaggedTy jgd in s_db
                where jgd.jaggedList.Contains(myInList)
                select jgd;

            Assert.AreEqual(1, q2.ToList().Count);


        }
        [TestMethod]
        public void TestStoreSimpleJaggedAndShrink()
        {
            Siaqodb s_db = new Siaqodb(objPath);


            s_db.DropType<JaggedTy>();
            for (int i = 0; i < 20; i++)
            {
                JaggedTy jg = new JaggedTy();
                jg.jaggedByte = new byte[2][];
                jg.jaggedByte[0] = new byte[2];
                jg.jaggedByte[0][0] = (byte)i;
                jg.jaggedByte[0][1] = (byte)(i + 1);
                jg.jaggedByte[1] = new byte[2];
                jg.jaggedByte[1][0] = (byte)(i + 2);
                jg.jaggedByte[1][1] = (byte)(i + 3);

                jg.jaggedInt = new int[2][];
                jg.jaggedInt[0] = new int[2];
                jg.jaggedInt[0][0] = i;
                jg.jaggedInt[0][1] = (i + 1);
                jg.jaggedInt[1] = new int[2];
                jg.jaggedInt[1][0] = (i + 2);
                jg.jaggedInt[1][1] = (i + 3);

                jg.jaggedList = new List<List<int>>();
                List<int> myList = new List<int>();
                myList.Add(i);
                myList.Add(i + 1);
                List<int> myList2 = new List<int>();
                myList2.Add(i * 10);
                myList2.Add(i * 100);
                jg.jaggedList.Add(myList);
                jg.jaggedList.Add(myList2);

                jg.jaggedListStr = new List<List<string>>();
                List<string> myListStr = new List<string>();
                myListStr.Add("ws" + i.ToString());
                myListStr.Add("second" + i.ToString());
                jg.jaggedListStr.Add(myListStr);

               
                s_db.StoreObject(jg);
            }
            IList<JaggedTy> all = s_db.LoadAll<JaggedTy>();
            for (int i = 0; i < 10; i++)
            {
                s_db.Delete(all[i]);
            }
            s_db.Close();

            SiaqodbUtil.Shrink(objPath, ShrinkType.Normal);
            SiaqodbUtil.Shrink(objPath, ShrinkType.ForceClaimSpace);

            s_db = new Siaqodb(objPath);
            all = s_db.LoadAll<JaggedTy>();

            for (int i = 10; i < 20; i++)
            {
                int j = i - 10;
                Assert.AreEqual((byte)i, all[j].jaggedByte[0][0]);
                Assert.AreEqual((byte)(i + 1), all[j].jaggedByte[0][1]);
                Assert.AreEqual((byte)(i + 2), all[j].jaggedByte[1][0]);
                Assert.AreEqual((byte)(i + 3), all[j].jaggedByte[1][1]);

                Assert.AreEqual(i, all[j].jaggedInt[0][0]);
                Assert.AreEqual((i + 1), all[j].jaggedInt[0][1]);
                Assert.AreEqual((i + 2), all[j].jaggedInt[1][0]);
                Assert.AreEqual((i + 3), all[j].jaggedInt[1][1]);

                Assert.AreEqual(2, all[j].jaggedList[0].Count);
                Assert.AreEqual(2, all[j].jaggedList.Count);
                Assert.AreEqual(i, all[j].jaggedList[0][0]);
                Assert.AreEqual(i + 1, all[j].jaggedList[0][1]);
                Assert.AreEqual(i * 10, all[j].jaggedList[1][0]);
                Assert.AreEqual(i * 100, all[j].jaggedList[1][1]);

                Assert.AreEqual(1, all[j].jaggedListStr.Count);
                Assert.AreEqual(2, all[j].jaggedListStr[0].Count);
                Assert.AreEqual("ws" + i, all[j].jaggedListStr[0][0]);
                Assert.AreEqual("second" + i, all[j].jaggedListStr[0][1]);
               

            }

            var q = from JaggedTy jgd in s_db
                    where jgd.jaggedInt.Length == 2
                    select jgd;

            Assert.AreEqual(10, q.ToList().Count);

            List<int> myInList = new List<int>();
            myInList.Add(10);
            myInList.Add(11);

            var q2 = from JaggedTy jgd in s_db
                     where jgd.jaggedList.Contains(myInList)
                     select jgd;

            Assert.AreEqual(1, q2.ToList().Count);


        }
        [TestMethod]
        public void TestStoreNMatrix()
        {
            Siaqodb s_db = new Siaqodb(objPath);
            s_db.DropType<NMatrixTy>();
            for (int i = 0; i < 10; i++)
            {
                NMatrixTy jg = new NMatrixTy();
                jg.matrix3Int = new int[2][][];
                jg.matrix3Int[0] = new int[2][];
                jg.matrix3Int[0][0] = new int[2];
                jg.matrix3Int[0][0][0] = i;
                jg.matrix3Int[0][0][1] = i+1;
                jg.matrix3Int[0][1] = new int[2];
                jg.matrix3Int[0][1][0] = i+2;
                jg.matrix3Int[0][1][1] = i + 3;

                jg.matrix3Int[1] = new int[2][];
                jg.matrix3Int[1][0] = new int[2];
                jg.matrix3Int[1][0][0] = i;

                jg.matrixStr = new List<List<List<string>>>();
                List<List<string>> jaggedList = new List<List<string>>();
                List<string> myListStr = new List<string>();
                myListStr.Add("ws" + i.ToString());
                myListStr.Add("second" + i.ToString());
                jaggedList.Add(myListStr);
                jaggedList.Add(myListStr);
                jg.matrixStr.Add(jaggedList);
                jg.matrixStr.Add(jaggedList);

                jg.listArrayMatrix = new List<int[][]>();
                int[][] arr = new int[2][];
                arr[0] = new int[2];
                arr[0][0] = i;
                arr[0][1] = i+1;

                jg.listArrayMatrix.Add(arr);
                jg.listArrayMatrix.Add(arr);

                s_db.StoreObject(jg);
            }
            IList<NMatrixTy> all = s_db.LoadAll<NMatrixTy>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(i, all[i].matrix3Int[0][0][0]);
                Assert.AreEqual(i+1, all[i].matrix3Int[0][0][1]);
                Assert.AreEqual(2, all[i].matrix3Int[0].Length);
                Assert.AreEqual(2, all[i].matrix3Int[0][1].Length);

                

                Assert.AreEqual(2, all[i].matrixStr.Count);
                Assert.AreEqual(2, all[i].matrixStr[0].Count);
                Assert.AreEqual(2, all[i].matrixStr[1].Count);
                Assert.AreEqual("ws" + i, all[i].matrixStr[0][0][0]);
                Assert.AreEqual("second" + i, all[i].matrixStr[0][0][1]);
                Assert.AreEqual("ws" + i, all[i].matrixStr[0][1][0]);
                Assert.AreEqual("second" + i, all[i].matrixStr[0][1][1]);
                Assert.AreEqual("ws" + i, all[i].matrixStr[1][1][0]);
                Assert.AreEqual("second" + i, all[i].matrixStr[1][1][1]);

                Assert.AreEqual(i, all[i].listArrayMatrix[0][0][0]);
                Assert.AreEqual(i+1, all[i].listArrayMatrix[1][0][1]);
               


            }
        }
        [TestMethod]
        public void TestStoreDictionaryAndShrink()
        {
            Siaqodb s_db = new Siaqodb(objPath);

            s_db.DropType<WithDict>();
            s_db.DropType<JaggedTy>();
            s_db.DropType<NMatrixTy>();
            for (int i = 0; i < 20; i++)
            {
                WithDict dict = new WithDict();
                dict.DictInt = new Dictionary<int, int>();
                dict.DictStr = new Dictionary<byte, string>();
                dict.DictComplex = new Dictionary<JaggedTy, int>();
                dict.ZuperDict = new Dictionary<uint, NMatrixTy>();

                for (int j = 0; j < 5; j++)
                {
                    dict.DictInt[j] = i + j;
                    dict.ZuperDict[(uint)j] = new NMatrixTy();
                    dict.DictStr[(byte)j] = "sss" + i.ToString();
                    JaggedTy jt = new JaggedTy();
                    dict.DictComplex[jt] = j + i;
                }

                s_db.StoreObject(dict);
            }

            IList<WithDict> all = s_db.LoadAll<WithDict>();
            for (int i = 0; i < 10; i++)
            {
                s_db.Delete(all[i]);
            }
            s_db.Close();

            SiaqodbUtil.Shrink(objPath, ShrinkType.Normal);
            SiaqodbUtil.Shrink(objPath, ShrinkType.ForceClaimSpace);

            s_db = new Siaqodb(objPath);
            all = s_db.LoadAll<WithDict>();

            for (int i = 10; i < 20; i++)
            {
                int j = i - 10;
                Assert.AreEqual(5, all[j].DictInt.Keys.Count);
                Assert.AreEqual(5, all[j].DictStr.Keys.Count);
                Assert.AreEqual(5, all[j].DictComplex.Keys.Count);
                Assert.AreEqual(5, all[j].ZuperDict.Keys.Count);
                for (int k = 0; k < 5; k++)
                {
                    Assert.AreEqual(i + k, all[j].DictInt[k]);
                    Assert.AreEqual("sss" + i, all[j].DictStr[(byte)k]);
                    Assert.IsNotNull(all[j].ZuperDict[(uint)k]);
                }
            }


            var q = from WithDict d in s_db
                    where d.DictInt.ContainsKey(1)
                    select d;
            Assert.AreEqual(10, q.ToList().Count);
            q = from WithDict d in s_db
                where d.DictInt.ContainsValue(11)
                select d;
            Assert.AreEqual(2, q.ToList().Count);

            q = from WithDict d in s_db
                where d.DictInt.ContainsKey(-1)
                select d;
            Assert.AreEqual(0, q.ToList().Count);

        }
        [TestMethod]
        public void TestStoreDictionary()
        {
            Siaqodb s_db = new Siaqodb(objPath);

            s_db.DropType<WithDict>();
            s_db.DropType<JaggedTy>();
            s_db.DropType<NMatrixTy>();
            for (int i = 0; i < 10; i++)
            {
                WithDict dict = new WithDict();
                dict.DictInt = new Dictionary<int, int>();
                dict.DictStr = new Dictionary<byte, string>();
                dict.DictComplex = new Dictionary<JaggedTy, int>();
                dict.ZuperDict = new Dictionary<uint, NMatrixTy>();
                
                for (int j = 0; j < 5; j++)
                {
                    dict.DictInt[j] = i + j;
                    dict.ZuperDict[(uint)j] = new NMatrixTy();
                    dict.DictStr[(byte)j] = "sss" + i.ToString();
                    JaggedTy jt = new JaggedTy();
                    dict.DictComplex[jt] = j + i;
                }

                s_db.StoreObject(dict);
            }
            IList<WithDict> all = s_db.LoadAll<WithDict>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(5, all[i].DictInt.Keys.Count);
                Assert.AreEqual(5, all[i].DictStr.Keys.Count);
                Assert.AreEqual(5, all[i].DictComplex.Keys.Count);
                Assert.AreEqual(5, all[i].ZuperDict.Keys.Count);
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(i+j, all[i].DictInt[j]);
                    Assert.AreEqual("sss"+i, all[i].DictStr[(byte)j]);
                    Assert.IsNotNull( all[i].ZuperDict[(uint)j]);
                }
            }
            s_db.Close();
            s_db = new Siaqodb(objPath);
            all = s_db.LoadAll<WithDict>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(5, all[i].DictInt.Keys.Count);
                Assert.AreEqual(5, all[i].DictStr.Keys.Count);
                Assert.AreEqual(5, all[i].DictComplex.Keys.Count);
                Assert.AreEqual(5, all[i].ZuperDict.Keys.Count);
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(i + j, all[i].DictInt[j]);
                    Assert.AreEqual("sss" + i, all[i].DictStr[(byte)j]);
                    Assert.IsTrue( all[i].ZuperDict[(uint)j].OID>0);
                }
            }
            var q = from WithDict d in s_db
                    where d.DictInt.ContainsKey(1)
                    select d;
            Assert.AreEqual(10, q.ToList().Count);
             q = from WithDict d in s_db
                    where d.DictInt.ContainsValue(1)
                    select d;
            Assert.AreEqual(2, q.ToList().Count);

            q = from WithDict d in s_db
                    where d.DictInt.ContainsKey(-1)
                    select d;
            Assert.AreEqual(0, q.ToList().Count);

        }
        [TestMethod]
        public void TestUpdateDictionary()
        {
            Siaqodb s_db = new Siaqodb(objPath);

            s_db.DropType<WithDict>();
            s_db.DropType<JaggedTy>();
            s_db.DropType<NMatrixTy>();
            for (int i = 0; i < 10; i++)
            {
                WithDict dict = new WithDict();
                dict.DictInt = new Dictionary<int, int>();
                dict.DictStr = new Dictionary<byte, string>();
                dict.DictComplex = new Dictionary<JaggedTy, int>();
                dict.ZuperDict = new Dictionary<uint, NMatrixTy>();

                for (int j = 0; j < 5; j++)
                {
                    dict.DictInt[j] = i + j;
                    dict.ZuperDict[(uint)j] = new NMatrixTy();
                    dict.DictStr[(byte)j] = "sss" + i.ToString();
                    JaggedTy jt = new JaggedTy();
                    dict.DictComplex[jt] = j + i;
                }

                s_db.StoreObject(dict);
            }
            IList<WithDict> all = s_db.LoadAll<WithDict>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    all[i].DictInt[j] = i + j + 10;
                    all[i].DictStr[(byte)j]="updated test";
                    all[i].ZuperDict[(uint)j]=new NMatrixTy();
                }
                s_db.StoreObject(all[i]);
            }
             all = s_db.LoadAll<WithDict>();
            
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(5, all[i].DictInt.Keys.Count);
                Assert.AreEqual(5, all[i].DictStr.Keys.Count);
                Assert.AreEqual(5, all[i].DictComplex.Keys.Count);
                Assert.AreEqual(5, all[i].ZuperDict.Keys.Count);
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(i + j+10, all[i].DictInt[j]);
                    Assert.AreEqual("updated test", all[i].DictStr[(byte)j]);
                    Assert.IsNotNull(all[i].ZuperDict[(uint)j]);
                }
            }
            s_db.Close();
            s_db = new Siaqodb(objPath);
            all = s_db.LoadAll<WithDict>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(5, all[i].DictInt.Keys.Count);
                Assert.AreEqual(5, all[i].DictStr.Keys.Count);
                Assert.AreEqual(5, all[i].DictComplex.Keys.Count);
                Assert.AreEqual(5, all[i].ZuperDict.Keys.Count);
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(i + j + 10, all[i].DictInt[j]);
                    Assert.AreEqual("updated test", all[i].DictStr[(byte)j]);
                    Assert.IsNotNull(all[i].ZuperDict[(uint)j]);
                }
            }
            var q = from WithDict d in s_db
                    where d.DictInt.ContainsKey(1)
                    select d;
            Assert.AreEqual(10, q.ToList().Count);
            q = from WithDict d in s_db
                where d.DictInt.ContainsValue(11)
                select d;
            Assert.AreEqual(2, q.ToList().Count);

            q = from WithDict d in s_db
                where d.DictInt.ContainsKey(-1)
                select d;
            Assert.AreEqual(0, q.ToList().Count);

        }
        [TestMethod]
        public void TestStoreDictionaryTransactional()
        {
            Siaqodb s_db = new Siaqodb(objPath);

            
            s_db.DropType<WithDict>();
            s_db.DropType<JaggedTy>();
            s_db.DropType<NMatrixTy>();
            ITransaction transaction = s_db.BeginTransaction();
            for (int i = 0; i < 10; i++)
            {
                WithDict dict = new WithDict();
                dict.DictInt = new Dictionary<int, int>();
                dict.DictStr = new Dictionary<byte, string>();
                dict.DictComplex = new Dictionary<JaggedTy, int>();
                dict.ZuperDict = new Dictionary<uint, NMatrixTy>();

                for (int j = 0; j < 5; j++)
                {
                    dict.DictInt[j] = i + j;
                    dict.ZuperDict[(uint)j] = new NMatrixTy();
                    dict.DictStr[(byte)j] = "sss" + i.ToString();
                    JaggedTy jt = new JaggedTy();
                    dict.DictComplex[jt] = j + i;
                }

                s_db.StoreObject(dict,transaction);
            }
            transaction.Commit();
            try {

                transaction = s_db.BeginTransaction();
                for (int i = 0; i < 10; i++)
                {
                    WithDict dict = new WithDict();
                    dict.DictInt = new Dictionary<int, int>();
                    dict.DictStr = new Dictionary<byte, string>();
                    dict.DictComplex = new Dictionary<JaggedTy, int>();
                    dict.ZuperDict = new Dictionary<uint, NMatrixTy>();

                    for (int j = 0; j < 5; j++)
                    {
                        dict.DictInt[j] = i + j;
                        dict.ZuperDict[(uint)j] = new NMatrixTy();
                        dict.DictStr[(byte)j] = "sss" + i.ToString();
                        JaggedTy jt = new JaggedTy();
                        dict.DictComplex[jt] = j + i;
                    }

                    s_db.StoreObject(dict, transaction);
                    if (i == 5)
                    {
                        throw new Exception("need some rollback");
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
            }
            IList<WithDict> all = s_db.LoadAll<WithDict>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(5, all[i].DictInt.Keys.Count);
                Assert.AreEqual(5, all[i].DictStr.Keys.Count);
                Assert.AreEqual(5, all[i].DictComplex.Keys.Count);
                Assert.AreEqual(5, all[i].ZuperDict.Keys.Count);
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(i + j, all[i].DictInt[j]);
                    Assert.AreEqual("sss" + i, all[i].DictStr[(byte)j]);
                    Assert.IsNotNull(all[i].ZuperDict[(uint)j]);
                }
            }
            s_db.Close();
            s_db = new Siaqodb(objPath);
            all = s_db.LoadAll<WithDict>();
            Assert.AreEqual(10, all.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(5, all[i].DictInt.Keys.Count);
                Assert.AreEqual(5, all[i].DictStr.Keys.Count);
                Assert.AreEqual(5, all[i].DictComplex.Keys.Count);
                Assert.AreEqual(5, all[i].ZuperDict.Keys.Count);
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(i + j, all[i].DictInt[j]);
                    Assert.AreEqual("sss" + i, all[i].DictStr[(byte)j]);
                    Assert.IsTrue(all[i].ZuperDict[(uint)j].OID > 0);
                }
            }
            var q = from WithDict d in s_db
                    where d.DictInt.ContainsKey(1)
                    select d;
            Assert.AreEqual(10, q.ToList().Count);
            q = from WithDict d in s_db
                where d.DictInt.ContainsValue(1)
                select d;
            Assert.AreEqual(2, q.ToList().Count);

            q = from WithDict d in s_db
                where d.DictInt.ContainsKey(-1)
                select d;
            Assert.AreEqual(0, q.ToList().Count);

        }
    }

    public class JaggedTy
    {
        public int OID { get; set; }
        public int[][] jaggedInt;
        public byte[][] jaggedByte;
        public List<List<int>> jaggedList;
        public List<List<string>> jaggedListStr;
        public int HH;
        public byte[][] jaggedByte1;
        public List<List<int>> jaggedList1;
        public List<List<string>> jaggedListStr1;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
    public class NMatrixTy
    {
        public int OID { get; set; }
        public int[][][] matrix3Int;
        public List<List<List<string>>> matrixStr;
        public List<int[][]> listArrayMatrix;
        public List<List<List<string>>> matrixStr1;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
    }
    public class WithDict
    {
        public int OID { get; set; }
        public Dictionary<int, int> DictInt;
        public Dictionary<JaggedTy, int> DictComplex;
        public Dictionary<byte, string> DictStr;
        public Dictionary<uint, NMatrixTy> ZuperDict;
        int GG;
        public Dictionary<JaggedTy, int> DictComplex22;

        public int[][] jaggedInt;
        public int[][][] jaggedInt3;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
        
    }
}
