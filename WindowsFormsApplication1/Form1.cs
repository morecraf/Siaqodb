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
            SiaqodbConfigurator.EncryptedDatabase = true;
            SiaqodbConfigurator.SetEncryptor(BuildInAlgorithm.AES);
            SiaqodbConfigurator.SetEncryptionPassword("correct");
            SiaqodbConfigurator.SetDatabaseFileName<Player>("myplayer");

            SiaqodbConfigurator.SetLicense(@"3XnXneBWc/FGTK9mZdpLVR7cUv1fplh11lH4Y60jNlQ=");
            Siaqodb sqo=new Siaqodb(@"c:\apps\OpenSource projects\sqoo\tests\rty22\");
            for (int i = 0; i < 10; i++)
            {
                sqo.StoreObject(new Player() { Name = "Andor" + i.ToString(), Age = i + 20 });
            }
            IList<Player> players = sqo.LoadAll<Player>();
            string d = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            A a = new A(); a.Name = "AAA";
            B b = new B(); b.Name = "BBB"; b.age = 10;
            Z z = new Z();
            z.a = a;
            z.b = b;
            z.items.Add(a);
            z.items.Add(b);
            SiaqodbConfigurator.SetLicense(@"qU3TtvA4T4L30VSlCCGUTXNXoKgzghhG5v8/UHPmMf8=");
           
            Siaqodb sqo = new Siaqodb(@"e:\sqoo\temp\db\");

            sqo.StoreObject(z);
            int count = (from aBase aa in sqo select aa).Count();
            string ass = "s";
        }
    }
    public class Player
    {
        public string Name { get; set; }
        public int OID { get; set; }
        public int Age { get; set; }
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
