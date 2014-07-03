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
            //SiaqodbConfigurator.SetDocumentSerializer(new BSONSerializer());
            SiaqodbConfigurator.SetLicense("ZWsshw4VXZbmQa8beHWlOAde6w4EqCYCOqUjgSS0Qi0=");
            Siaqodb db = new Siaqodb(@"e:\temp\_clouddb\");
            for (int i = 0; i < 10; i++)
            {

                //Invoice inv=new Invoice() { InvoiceNumber = i, Customer = "MyCust" + i, Total = i * 10 };
                //db.Store
                //    (
                //    key: i.ToString(), 
                //    obj: inv,
                //    tags: new { Email = "mycust" + i % 2 + "@hope.ro" }
                //    );
                User b = new User() { UserName = "qq" + i.ToString() };
                db.StoreObject(b);
            }
            db.Flush();
            User result = db.Cast<User>().First(user => user.UserName.Contains( "name",StringComparison.OrdinalIgnoreCase)
                &&  user.UserName.StartsWith( "name",StringComparison.OrdinalIgnoreCase)
                && user.UserName.EndsWith("name", StringComparison.OrdinalIgnoreCase));
            string s = "";
           // var q = db.Query<DotissiObject>().Where(a => a.StrTags["Email"] == "mycust0@hope.ro").ToList();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            SiaqodbConfigurator.SetDocumentSerializer(new JsonCRSerializer());
           
            WisentClient.Wisent client = new WisentClient.Wisent();
            DotissiObject doObj = new DotissiObject();
            User book=new User();
            book.UserName="2023";
            book.author="Cristi Ursachi45";
            book.body="An amazing book...";
            book.title="How tos";
            book.copies_owned=7;
            
            doObj.SetValue<User>(book);
            doObj.Key = book.UserName;
            try
            {
                await client.Put("crypto_users", doObj);
            }
            catch(Exception ex)
            {
                
            }
            DotissiObject obj = await client.Get("crypto_users", book.UserName);
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
