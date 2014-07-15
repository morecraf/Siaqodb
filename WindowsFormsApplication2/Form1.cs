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
using CryptonorClient;
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
            SiaqodbConfigurator.SetDocumentSerializer(new JsonCRSerializer());
            SiaqodbConfigurator.SetLicense("anqHBdiAJzXSpNdJRy+BkMMNlL1+jZBe4wyzvnZpba8=");
            CryptonorLocalBucket db = new CryptonorLocalBucket(@"c:\work\temp\clouddb\");
            for (int i = 0; i < 10; i++)
            {

                Invoice inv=new Invoice() { InvoiceNumber = i, Customer = "MyCust" + i, Total = i * 10 };
                db.Store
                    (
                    key: i.ToString(), 
                    obj: inv,
                    tags: new { Email = "mycust" + i % 2 + "@hope.ro" }
                    );
                //User b = new User() { UserName = "qq" + i.ToString() };
                //db.StoreObject(b);
            }
            //db.Flush();
            //User result = db.Cast<User>().First(user => user.UserName.Contains( "name",StringComparison.OrdinalIgnoreCase)
             //   &&  user.UserName.StartsWith( "name",StringComparison.OrdinalIgnoreCase)
              //  && user.UserName.EndsWith("name", StringComparison.OrdinalIgnoreCase));
           
            var q = db.Query().Where(a => a.Tags_String["Email"] == "mycust0@hope.ro").ToList();
            
            string s = "";
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            SiaqodbConfigurator.SetDocumentSerializer(new JsonCRSerializer());
            CryptonorConfigurator.SetEncryptionKey("alfabetaalfabeta");
            Cryptonor.CryptonorHttpClient client = new Cryptonor.CryptonorHttpClient();
            CryptonorObject doObj = new CryptonorObject();
            User book=new User();
            book.UserName="2030";
            book.author="Cristi Ursachi45";
            book.body="An amazing book...";
            book.title="How tos";
            book.copies_owned=7;
            
            doObj.SetValue<User>(book);
            var aa = doObj.GetValue<User>();
            doObj.Key = book.UserName;
           // doObj.Tags = new Dictionary<string, object>();
            //doObj.Tags["country"] = "RO";
           // doObj.Tags["mydecimal"] = new decimal(20.2);
            doObj.SetTag("myguid3", new Guid("e8f3b6f8-e034-40d0-92ca-2b5994ce3e60"));
            //doObj.Tags_Guid["myguid3"] = Guid.NewGuid();

            try
            {
               await client.Put("crypto_users", doObj);
            }
            catch(Exception ex)
            {
                
            }
            IEnumerable<CryptonorObject> obj = await client.GetByTag("crypto_users", "myguid3", "eq", new Guid("e8f3b6f8-e034-40d0-92ca-2b5994ce3e60"));
            //var ra = await client.Get("crypto_users");
            string a = "";

        }
    }
    public class User
    {
        public string UserName;
        public string title;
        public string author;
        public string body;
        public int copies_owned;

    }
    public class Invoice
    {
        public int InvoiceNumber { get; set; }
        public string Customer { get; set; }
        public decimal Total { get; set; }
    }
    public class JsonCRSerializer : IDocumentSerializer
    {
        #region IDocumentSerializer Members
        readonly JsonSerializer serializer = new JsonSerializer();
        public object Deserialize(Type type, byte[] objectBytes)
        {
            string jsonStr = Encoding.UTF8.GetString(objectBytes);
            return JsonConvert.DeserializeObject(jsonStr,type);
        }

        public byte[] Serialize(object obj)
        {
            JsonSerializerSettings sett=new JsonSerializerSettings();
            
            string jsonStr =JsonConvert.SerializeObject(obj);
            return  Encoding.UTF8.GetBytes(jsonStr);
        }

        #endregion
    }

}
