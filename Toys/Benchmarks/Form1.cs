using Sqo;
using Sqo.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Benchmarks
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<KeyDir> list = new List<KeyDir>();
        private async void button1_Click(object sender, EventArgs e)
        {
            SiaqodbConfigurator.SetLicense(@"vV2aL+lsO2Vr+Xz98USEWR2EC75iY6l8XxmKFXZ/f3c=");
          
            Siaqodb siaqodb = new Siaqodb(@"c:\work\temp\paginatedTests\");
             DateTime start = DateTime.Now;
             siaqodb.StartBulkInsert(typeof(CryptonorObject));
            for (int i = 0; i < 100000; i++)
            {
                CryptonorObject doObj = new CryptonorObject();
                doObj.Document = new byte[128];
                doObj.Tags = new byte[30];
                doObj.Key = i.ToString();
                doObj.year = i;
                doObj.country = i.ToString();
                doObj.age = i;

                //list.Add(new KeyDir());

                //int oID = siaqodb.GetOID(doObj);
                //if (oID == 0)
                //{
                //    Sqo.Internal._bs._loidby(siaqodb,"key", doObj );
                //}
                //oID = siaqodb.GetOID(doObj);
              // siaqodb.StoreObject(doObj);
            }
           // siaqodb.EndBulkInsert(typeof(CryptonorObject));
           // siaqodb.Flush();
            //var all = siaqodb.LoadAll<KeyDir>();

            string elapsed = (DateTime.Now - start).ToString();
            List<MetaType> li = siaqodb.GetAllTypes();
            start = DateTime.Now;

            var alloids =siaqodb.LoadAllOIDs(li[0]);
            List<string> keys = new List<string>();
            foreach (int oid in alloids)
            { 
              keys.Add((string)siaqodb.LoadValue(oid,"key" ,li[0])); 
            }
            elapsed = (DateTime.Now - start).ToString();

            string a = "";
        }
    }
    public class CryptonorObject
    {
        
        private string key;
       
        public int age;
       
        public string country;
       
        public int year;
        public int OID { get; set; }

        public bool ShouldSerializeOID()
        {
            return false;
        }
        public bool ShouldSerializeIsDirty()
        {
            return false;
        }
        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }
      
        private byte[] document;
        public byte[] Document
        {
            get { return document; }
            set { document = value; }
        }
        
        private byte[] tags;
        public byte[] Tags
        {
            get { return tags; }
            set { tags = value; }
        }
        public string Version { get; set; }

        public bool IsDirty { get; set; }

        internal CryptonorObject(string key, byte[] document)
        {
            this.Key = key;
            this.Document = document;
        }
        public CryptonorObject()
        {
        }


       

      

    }
    public class KeyDir
    {
        public int Key { get; set; }
        public int ValuePos { get; set; }
        public int ValueSize { get; set; }
    }
}
