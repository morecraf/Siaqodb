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

            SiaqodbConfigurator.SetTrialLicense(@"G5Km9leSRHoYJ784J8ascwPg868xkD5kGQQHDbGcvC0=");
            Siaqodb sqo = new Siaqodb(@"e:\sqoo\temp\db\");
            DateTime start = DateTime.Now;
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
            }
            string elapsed = (DateTime.Now - start).ToString();
            MessageBox.Show("Inserted:"+elapsed);
            start = DateTime.Now;
            IList<PlayerHost> players = sqo.LoadAll<PlayerHost>();
            var q = (from PlayerHost phh in sqo
                    where phh.SomeField == 10
                    select phh).ToList();
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
}
