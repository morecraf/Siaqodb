using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sqo;
using System.Reflection;
using System.IO;

namespace SmartDeviceProject1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SiaqodbConfigurator.SetLicense(@"QfkAx5pzfWWLTzNz4/JEhYTLBAtbTRIMPdmYHuwSSpKxVIjLoRCHccLopehBveZ+");
            string dbPath=Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            Siaqodb instance = new Siaqodb(dbPath);
            A a = new A();
            a.AString = "aaa";
            instance.StoreObject(a);
            IList<A> ssss = instance.LoadAll<A>();
            string s = "";
        }
    }
    public class A
    {
        public int OID { get; set; }
        public String AString { get; set; }
    }
}