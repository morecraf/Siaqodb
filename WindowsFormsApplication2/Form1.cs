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
using System.Linq.Expressions;

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
            CryptonorLocalBucket db = new CryptonorLocalBucket("crypto_users",@"c:\work\temp\clouddb\","","");
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
           
           // var q = db.Query().Where(a => a.Tags_String["Email"] == "mycust0@hope.ro").ToList();
            
            string s = "";
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            SiaqodbConfigurator.SetDocumentSerializer(new JsonCRSerializer());
            CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.Camellia128,"aaaa");
            CryptonorHttpClient client = new CryptonorHttpClient("http://localhost:53411/", "excelsior");
            CryptonorClient.CryptonorClient cl = new CryptonorClient.CryptonorClient("http://localhost:53411/", "excelsior");
            //IBucket bucket = cl.GetLocalBucket("crypto_users", @"c:\work\temp\cloudb3");
           IBucket bucket = cl.GetBucket("crypto_users");
            DateTime start = DateTime.Now;
          
            List<CryptonorObject> list = new List<CryptonorObject>();
            for (int i = 0; i < 2; i++)
            {
                CryptonorObject doObj = new CryptonorObject();
                User book = new User();
                book.UserName = "3111" + i.ToString();
                book.author = "Ursachi Alisia";
                book.body = "An amazing book...";
                book.title = "How tos";
                book.copies_owned = 7;
                
                doObj.SetValue<User>(book);
                var aa = doObj.GetValue<User>();
                doObj.Key = book.UserName;
                //doObj.Tags = new Dictionary<string, object>();
                // doObj.Tags["country"] = "RO";
                // doObj.Tags["mydecimal"] = new decimal(20.2);
                doObj.SetTag("birth_year", 2008);
                doObj.SetTag("age", 20);
                doObj.SetTag("country", "RO");
                //doObj.Tags_Guid["myguid3"] = Guid.NewGuid();
                
               await bucket.Store(doObj);
               // list.Add(doObj);

            }
            //Expression<Func<CryptonorObject, bool>> expr = a => (a.Tags<int>("birth_year") >= 2007 && a.Tags<int>("birth_year") <=2009);
            //Expression<Func<CryptonorObject, bool>> expr = a => a.Key >= "21111" ;
          
           
            //var qlos = (await bucket.Query().Where(expr).GetResultSetAsync()).GetValues<User>();
          
           // await ((CryptonorLocalBucket)bucket).Push();
            // await ((CryptonorLocalBucket)bucket).Pull(expr,3);
            //var all = await bucket.Get("21110");
           // var qlos = (await bucket.Query().Where(expr).GetResultSetAsync()).GetValues<User>();
            CryptonorQuery query67 = new CryptonorQuery("birth_year");
            decimal d = 23.456M;
            query67.Configure(a => a.In(2008,2009.98));
            var objw = await bucket.Get(query67);
            string elapsed = (DateTime.Now - start).ToString();

            //var asteroid = await client.GetByTag("crypto_users", "country", "RO",10,0);
            //var asteroid2 = await client.GetByTag("crypto_users", "country", "RO",10,asteroid.ContinuationToken);
           
              //var q = await bucket.Query().Where(ar =>ar.Tags<int>("Age")==20).OrderBy(a=>a.Tags<int>("Age")) .GetResultSetAsync();// ar.Tags_String["country"] == "RO" && (ar.Tags_Int["birth_year"] > 1900)).GetResultSetAsync();
              //var objects = q.GetValues<User>();
              //var usernames=objects.Select(a => a.UserName).OrderBy(a => a).ToList(); 
             // var q2 = await bucket.Query(q.ContinuationToken).Where(ar => ar.Tags_String["country"] == "RO" && (ar.Tags_Int["birth_year"] > 1900)).GetResultSetAsync();
              //var objects2 = q2.GetValues<User>();
              //var usernames2 = objects2.Select(a => a.UserName).OrderBy(a => a).ToList(); 
            
            // var all = await bucket.GetAllAsync();
            string aaas = "";
         
           // IEnumerable<CryptonorObject> obj = await client.GetByTag("crypto_users", "myguid3", "eq", new Guid("e8f3b6f8-e034-40d0-92ca-2b5994ce3e60"));
            //var ra = await client.Get("crypto_users");
            string aasw = "";
            List<User> users = new List<User>();
            var filtered = users.Where(b => b.UserName == "myuser");

            var linqSt = from User u in users
                         orderby u.UserName
                         select u;
            A avar = new B();
            avar.MetA();
        }
       
    }
    class A
    {
        public string MetA()
        {
            return "MetA";
        }
    }
    class B:A
    {
        public string MetA()
        {
            return "MetB";
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
