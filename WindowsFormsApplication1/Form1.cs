using Sqo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Sqo.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // SiaqodbConfigurator.EncryptedDatabase = true;
            //SiaqodbConfigurator.SetEncryptor(BuildInAlgorithm.AES);
            //SiaqodbConfigurator.SetEncryptionPassword("correct");
            //SiaqodbConfigurator.SetDatabaseFileName<Player>("myplayer");
           // GetMemberBySwipe("");
            SiaqodbConfigurator.SetLicense(@"G5Km9leSRHoYJ784J8ascwPg868xkD5kGQQHDbGcvC0=");
            SiaqodbConfigurator.SetDocumentSerializer(new ProtoBufSerializer());
            Siaqodb sqo = new Siaqodb(@"e:\sqoo\temp\db\");
            sqo.DropType<PlayerHost>();
            //DateTime start = DateTime.Now;
            //for (int i = 0; i < 10; i++)
            //{ 
            //    DataUpdate du=new DataUpdate();
            //    du.MemberId=Guid.NewGuid();
            //    du.ObjectId=Guid.NewGuid();
            //    CachedObject<DataUpdate> cache = new CachedObject<DataUpdate>(new Tuple<string, long>(i.ToString(), i), du);
            //    sqo.StoreObject(cache);
            //}
            //var first=CachedObject<DataUpdate>.LoadAll(sqo);
            DateTime start = DateTime.Now;
            Player LastPlayer = null;
            for (int i = 0; i < 1000; i++)
            {
                Player p = new Player() { Name = "Andor" + i.ToString(), Age = i + 20 };
                p.blob = new byte[100];
                p.dict = new Dictionary<int, int>();
                p.ListName = new List<string>();
                for (int j = 0; j < 100; j++)
                {
                    p.dict.Add(j, j);
                    p.blob[j] = (byte)(j % 100);
                    p.ListName.Add(j.ToString());
                }
                PlayerHost ph = new PlayerHost() { ThePlayer = p, SomeField = i };
                sqo.StoreObject(ph);
                LastPlayer = p;
            }
            string elapsed = (DateTime.Now - start).ToString();
            MessageBox.Show("Inserted:"+elapsed);
            start = DateTime.Now;
           // SiaqodbConfigurator.LoadRelatedObjects<PlayerHost>(false);
            IList<PlayerHost> players = sqo.LoadAll<PlayerHost>();
            
            var q = (from PlayerHost phh in sqo
                    where phh.SomeField==10
                    select phh).FirstOrDefault();
            q.ThePlayer.Age = 900000;
            sqo.StoreObject(q);
            elapsed = (DateTime.Now - start).ToString();
            MessageBox.Show("Read:" + elapsed);
            string d = "";
        //    MemoryStream memStr=new MemoryStream();
          //  ProtoBuf.Serializer.Serialize(memStr, new Player());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            A a = new A(); a.Name = "AAA";
            B b = new B(); b.Name = "BBB"; b.age = 10;
            Z z = new Z();
            z.a = a;
            z.b = b;
            //a.Name.Contains
            z.items.Add(a);
            z.items.Add(b);
           // SiaqodbConfigurator.SetLicense(@"qU3TtvA4T4L30VSlCCGUTXNXoKgzghhG5v8/UHPmMf8=");
           
            Siaqodb sqo = new Siaqodb(@"e:\sqoo\temp\db\");

            sqo.StoreObject(z);
            int count = (from aBase aa in sqo select aa).Count();
            string ass = "s";
        }

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    SiaqodbConfigurator.SetLicense(@"qU3TtvA4T4L30VSlCCGUTXNXoKgzghhG5v8/UHPmMf8=");
        //    SiaqodbUtil.ReIndex(@"C:\Users\cristi\Downloads\External (1)\External");
        //    Siaqodb SIAQODB = new Siaqodb(@"C:\Users\cristi\Downloads\External (1)\External");

        //    int truck = 1;
        //    int upgrade_type = 3;

        //    UpgradeAttached[] attached = (from UpgradeAttached u in SIAQODB
        //                                  where u.truck == truck && u.type == upgrade_type
        //                                  select u).ToArray();

        //    foreach (UpgradeAttached u in attached)
        //    {

        //        //Debug.Log (u.ToString());

        //        SIAQODB.Delete(u);

        //    }

        //    string g = "";
        //}
        public OMember GetMemberBySwipe(string x_swipe)
        {
            SiaqodbConfigurator.SetLicense(@"qU3TtvA4T4L30VSlCCGUTXNXoKgzghhG5v8/UHPmMf8=");
            Siaqodb siaqodb = new Siaqodb(@"e:\sqoo\temp\db\");
            string f_swipe = "test20000";

            //siaqodb.StartBulkInsert(typeof(OMember));
            try
            {
                for (int i = 0; i < 35000; i++)
                {
                    OMember mem = new OMember();
                    mem.mem_fname = "FName" + i.ToString();
                    mem.mem_sname = "SName" + i.ToString();
                    mem.mem_swipe = "test" + i.ToString();
                   // siaqodb.StoreObject(mem);
                }
            }
            finally
            {
               // siaqodb.EndBulkInsert();
            }
            DateTime start = DateTime.Now;
            var data = siaqodb.Cast<OMember>().Where(emp => emp.mem_swipe == f_swipe).FirstOrDefault();
            string elapsed = (DateTime.Now - start).ToString();

            return data;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SiaqodbConfigurator.SetLicense(@"G5Km9leSRHoYJ784J8ascwPg868xkD5kGQQHDbGcvC0=");
            
            Siaqodb sqo = new Siaqodb(@"e:\apps\do\");
          
            DateTime start = DateTime.Now;
           
            for (int i = 0; i < 100000; i++)
            {
                Tick t = new Tick();
                t.mydate = DateTime.Now;
                t.MyInt = i;
                t.mylong = i;
                t.mystring = "asdasd" + i.ToString();
                //t.blob[i % (8 * 1024 + 500)] = (byte)(i % 100);
                sqo.StoreObject(t);
            }
            sqo.Flush();

            string elapsed = (DateTime.Now - start).ToString();
           // MessageBox.Show("Inserted:" + elapsed);
            start = DateTime.Now;
            // SiaqodbConfigurator.LoadRelatedObjects<PlayerHost>(false);
            IList<Tick> players = sqo.LoadAll<Tick>();
            elapsed = (DateTime.Now - start).ToString();
            string g = "";
            
        }
    }
    public class Tick
    {
        public int OID{get;set;}
        public int MyInt;
        public string mystring;
        public DateTime mydate;
        public long mylong;
        //public byte[] blob = new byte[8 * 1024 + 500];

    }
    public class ProtoBufSerializer : IDocumentSerializer
    {

        #region IDocumentSerializer Members

        public object Deserialize(Type type, byte[] objectBytes)
        {
            using (MemoryStream ms = new MemoryStream(objectBytes))
            {
                return  ProtoBuf.Serializer.NonGeneric.Deserialize(type, ms);
            }
        }

        public byte[] Serialize(object obj)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ProtoBuf.Serializer.NonGeneric.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        #endregion
    }
    public class BSONSerializer : IDocumentSerializer
    {
        #region IDocumentSerializer Members
        readonly JsonSerializer serializer = new JsonSerializer();
        public object Deserialize(Type type, byte[] objectBytes)
        {
            using (MemoryStream ms = new MemoryStream(objectBytes))
            {
                var jsonTextReader = new BsonReader(ms);
                return serializer.Deserialize(jsonTextReader,type);
            }
        }

        public byte[] Serialize(object obj)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                BsonWriter writer = new BsonWriter(ms);
                serializer.Serialize(writer, obj);
             
               return ms.ToArray();
            }
        }

        #endregion
    }
    public class MsgPackSerializer : IDocumentSerializer
    {
        #region IDocumentSerializer Members
        
        public object Deserialize(Type type, byte[] objectBytes)
        {
            var serializer = MsgPack.Serialization.MessagePackSerializer.Create(type);
            return serializer.UnpackSingleObject(objectBytes);
        }

        public byte[] Serialize(object obj)
        {
            var serializer = MsgPack.Serialization.MessagePackSerializer.Create(obj.GetType());
            return serializer.PackSingleObject(obj);
        }

        #endregion
    }
    public class OMember : OBase<OMember>
    {
        private bool m_active = true;

       
        public int OID { get; set; }
       
        public int mem_memid { get; set; }
        public bool IsActive { get { return m_active; } set { m_active = value; } }
        public EMemberType MemberType { get; set; }
        public string mem_memcode { get; set; }
        public int mem_lnkid { get; set; }
        public int mem_typid { get; set; }
      
        public int mem_zcbid { get; set; }
     
        public string mem_fname { get; set; }
      
        public string mem_sname { get; set; }
        public string mem_displayname { get; set; }
        public EGender mem_gender { get; set; }
        public DateTime mem_dob { get; set; }
        public DateTime mem_jdate { get; set; }
        public DateTime mem_adate { get; set; }
        public DateTime mem_cdate { get; set; }
        public EMemberMop mem_mop { get; set; }
        public EMemStatus mem_membstat { get; set; }
        public decimal mem_recpbal { get; set; }
        public decimal mem_posbal { get; set; }
        public decimal mem_poslimit { get; set; }
        public string mem_gatemsg { get; set; }
        public string mem_bds { get; set; }
        public string mem_zlkjmail { get; set; }
        public DateTime mem_lastvisit { get; set; }
        public int LimitedVisitcount { get; set; }
        public int mem_cardno { get; set; }
       
        public string mem_swipe { get; set; }
        public int mem_zpiid { get; set; }
        public string mem_post1 { get; set; }
        public string mem_ylkretention { get; set; }
        public int mem_limited_use { get; set; }
        public int mem_corp_proof { get; set; }
        public int mem_student_proof { get; set; }
        public string ClubGroup { get; set; }
        public int mem_pospricelevel { get; set; }
        public DateTime mem_parqexdate { get; set; }
        public string mem_mopfailrsn { get; set; }
        public DateTime mem_lastrenewdate { get; set; }
        public DateTime LimitedEntryStartdate { get; set; }
        public DateTime _lastupgradedate { get; set; }
        public string mem_boltoncodes { get; set; }
        public decimal mem_membershipbalance { get; set; }

        public int mem_authorised_zloid { get; set; }
    }
    public class OBase<T> where T : new()
    {
        public static T Init()
        {
            dynamic holder = new T();
            holder.Base();
            return (T)holder;
        }

        public virtual void Base()
        {

        }
    }

    public enum EMemStatus
    {
        [Description("$$MSTAT_NONE_LBL$$")]
        None = 0,

        [Description("$$MSTAT_REGULAR_LBL$$")]
        Regular = 1,

        [Description("$$MSTAT_ABSENT_LBL$$")]
        Absent = 2,

        [Description("$$MSTAT_LEAVER_LBL$$")]
        Leaver = 3,

        [Description("$$MSTAT_OVERDUE_LBL$$")]
        Overdue = 4,

        [Description("$$MSTAT_BAD_DEBT_LBL$$")]
        BadDebt = 5,

        [Description("$$MSTAT_BAD_DEBT_NO_CHASE_LBL$$")]
        BadDebtNoChase = 6
    }
    public enum EMemberType
    {
        [Description("$$MTYPE_MEMBER_LBL$$")]
        Member = 216,

        [Description("$$MTYPE_PROSPECT_LBL$$")]
        Prospect = 105,

        [Description("$$MTYPE_GUEST_LBL$$")]
        Guest = 635,
    }
    public enum EMemberMop
    {
        DirectDebit,
        CreditCard
    }
    public enum EGender
    {
        NotSet,
        Male,
        Female,
    }
    [ProtoBuf.ProtoContract(ImplicitFields=ProtoBuf.ImplicitFields.AllPublic)]
    public class Player
    {
        public string Name { get; set; }
        public int OID { get; set; }
        public int Age { get; set; }
        public List<string> ListName { get; set; }
        public byte[] blob { get; set; }
        public Dictionary<int,int> dict { get; set; }

    }
    public class PlayerHost
    {
        [Sqo.Attributes.Document]
        public Player ThePlayer { get; set; }
        public int OID { get; set; }
        public int SomeField { get; set; }
    }
    public class aBase
    {
        public int OID{get;set;}
    }
    public class A : aBase
    {
        public string Name;

        public A() { }
    }
    public class B : aBase
    {
        public string Name;
        public int age;

        public B() { }
    }
    public class Z
    {
        public A a;
        public B b;
        public List<aBase> items = new List<aBase>();

        public Z() { }
    }
    /// <summary>
    /// The cache list.
    /// </summary>
    [Serializable]
    public class CachedObject<T> 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedObject{T}"/> class.
        /// </summary>
        /// <param name="cacheKey">
        /// The cache key.
        /// </param>
        /// <param name="cachedObject">
        /// The cached object.
        /// </param>
        /// <param name="entity"></param>
        public CachedObject(Tuple<string, long> cacheKey, T entity) : this()
        {
            this.Object = entity;
            this.CacheKey = cacheKey.Item2;
            this.CacheKeyString = cacheKey.Item1;
            
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="TipsterCachedObject{T}"/> class.
        /// </summary>
        public CachedObject()
        {
            this.CacheCreated = DateTime.UtcNow;
            this.ObjectDependencies = new List<long>();
            this.ListDependencies = new List<long>();
        }
 
        /// <summary>
        /// Gets or sets the cache list id.
        /// </summary>
        public long Id { get; set; }
 
        /// <summary>
        /// Gets or sets the object pk id.
        /// </summary>
        public Guid ObjectPK { get; set; }
 
        /// <summary>
        /// the cacke key, hashed from the cachekeystring
        /// </summary>
        public long CacheKey { get; set; }
 
        /// <summary>
        /// the cache key string
        /// </summary>
        //[Text]
        public string CacheKeyString { get; set; }
 
        public Tuple<string, long> CackeKeyPair { get {return new Tuple<string, long>(this.CacheKeyString, this.CacheKey);} }
 
        /// <summary>
        /// Gets or sets the cache created.
        /// </summary>
        public DateTime CacheCreated { get; set; }
 
        /// <summary>
        /// Gets or sets the object.
        /// </summary>
        public T Object { get; set; }
 
        /// <summary>
        /// Gets or sets the object dependencies.
        /// </summary>
        public List<long> ObjectDependencies { get; set; }
 
        /// <summary>
        /// Gets or sets the list dependencies.
        /// </summary>
        public List<long> ListDependencies { get; set; }
 
        /// <summary>
        /// Gets or sets the cache retrieved.
        /// </summary>
        public DateTime? CacheRetrieved { get; set; }
 
        /// <summary>
        /// Gets or sets the cache updated.
        /// </summary>
        public DateTime? CacheUpdated { get; set; }

        public static CachedObject<T> LoadAll(Siaqodb siaqodb)
        {
            return siaqodb.LoadAll<CachedObject<T>>().FirstOrDefault();
        }
    }
    [Serializable]
    public class DataUpdate : TipsterObject
    {
        #region Constructors and Destructors


        #endregion

        #region Public Properties


        #endregion

        public Guid MemberId { get; set; }
        public ObjectType ObjectType { get; set; }
        public Guid ObjectId { get; set; }
    }
    public enum ObjectType { Type1, Type2 }
    public class TipsterObject
    {
        public int OID { get; set; }
    }

}
