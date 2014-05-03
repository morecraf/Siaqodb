using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Sqo;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SiaqodbConfigurator.SetDocumentSerializer(new BSONSerializer());
            DotissiDB db = new DotissiDB(@"c:\Users\Xofin\Documents\GitHub\Siaqodb\db_temp\");
            for (int i = 0; i < 10; i++)
            {

                Invoice inv=new Invoice() { InvoiceNumber = i, Customer = "MyCust" + i, Total = i * 10 };
                db.Store
                    (
                    key: i.ToString(), 
                    obj: inv,
                    tags: new { Email = "mycust" + i % 2 + "@hope.ro" }
                    );
                
            }
            var q = db.Query<DotissiObject>().Where(a => a.StrTags["Email"] == "mycust0@hope.ro").ToList();
        }
    }
    public class Invoice
    {
        public int InvoiceNumber { get; set; }
        public string Customer { get; set; }
        public decimal Total { get; set; }
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
                return serializer.Deserialize(jsonTextReader, type);
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

}
