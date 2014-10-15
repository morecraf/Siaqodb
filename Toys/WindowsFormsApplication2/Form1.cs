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
using Cryptonor;
using Cryptonor.Queries;
using System.Net;

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
            Siaqodb siaqodb = new Siaqodb(@"c:\work\support\John_Kattenhorn\");
            for (int i = 0; i < 10; i++)
            {
                Node n = new Node() { Name = "sa" + i.ToString() };
                siaqodb.StoreObject(n);
            }
            siaqodb.Flush();
            //siaqodb.GetOID(
            var all = siaqodb.LoadAll<Node>();
            string s = "";
        }
            public class Node
            {
                private int oid;
                public int Id { get { return oid; } set { oid = value; } }
                public string Name { get; set; }
            }

            private async void button2_Click(object sender, EventArgs e)
            {
                ServicePointManager.ServerCertificateValidationCallback +=(sender1, cert, chain, sslPolicyErrors) => true;

                SiaqodbConfigurator.SetDocumentSerializer(new JsonCRSerializer());
                CryptonorConfigurator.SetEncryptor(EncryptionAlgorithm.AES256, "mysuper_secret");
                // CryptonorHttpClient client = new CryptonorHttpClient("http://localhost:53411/", "excelsior","mykey","mypwd");
                //CryptonorClient.CryptonorClient cl = new CryptonorClient.CryptonorClient("http://ipv4.fiddler/CryptonorWebAPI/", "excelsior");
                CryptonorClient.CryptonorClient cl = new CryptonorClient.CryptonorClient("http://portal.cryptonordb.com/api/api/", "9bbaae526db72073e5f23963d1008003", "FRswjDioAT");
                //var cl = new CryptonorClient.CryptonorClient("http://localhost:53411/api/", "9bbaae526db72073e5f23963d1008003", "FRswjDioAT");
                IBucket bucket = cl.GetBucket("iasi");

                //IBucket bucket = cl.GetLocalBucket("crypto_users", @"c:\work\temp\cloudb3");

                DateTime start = DateTime.Now;

                await this.Fill(bucket);

                string elapsed = (DateTime.Now - start).ToString();

                start = DateTime.Now;
                Query que = new Query("key");
                que.Setup(a => a.Start("buni").End("buni" + "999"));
                var filtered = await bucket.GetAsync(que);
                //var values = filtered.GetValues<User>();

                elapsed = (DateTime.Now - start).ToString();

                start = DateTime.Now;
                try
                {
                    //((LocalBucket)bucket).PullCompleted += Form1_PullCompleted;
                    //await ((LocalBucket)bucket).Pull();
                    // var all = await bucket.GetAllAsync();
                    // await bucket.DeleteAsync(all.Objects[0].Key);
                    start = DateTime.Now;

                    for (int i = 0; i < 1000; i++)
                    {
                        var a = await bucket.GetAsync("100" + i);
                    }
                    elapsed = (DateTime.Now - start).ToString();
                }
                catch
                {

                }
                elapsed = (DateTime.Now - start).ToString();
                start = DateTime.Now;
                Query query67 = new Query("mystr");
                query67.Setup(a => a.Value("gk9Zlq321c0"));
                var filtered22 = await bucket.GetAsync(query67);
                elapsed = (DateTime.Now - start).ToString();



                return;

            }
        private async Task Fill(IBucket bucket)
        {
            List<CryptonorObject> list = new List<CryptonorObject>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
            { 
                sb.Append("aaaaaaaaaammmmmmmmmmpopopopopouuuuuuuuuuttttttttttaaaaaaaaaammmmmmmmmmpopopopopouuuuuuuuuutttttttttt");
            }
            string body = sb.ToString();

            for (int i = 1; i < 100; i++)
            {
                CryptonorObject doObj = new CryptonorObject();
                User book = new User();
                book.UserName = textBox1.Text + (i).ToString();
                book.author = "Ursachi Alisia";

                book.body = body;
                book.title = "How tos";
                book.copies_owned = 7;

                doObj.SetValue<User>(book);
                //var aa = doObj.GetValue<User>();
                doObj.Key = book.UserName;
                //doObj.Tags = new Dictionary<string, object>();
                // doObj.Tags["country"] = "RO";
                // doObj.Tags["mydecimal"] = new decimal(20.2);
                doObj.SetTag("birth_year", 1000 + i);
                doObj.SetTag("age", i % 60);
                doObj.SetTag("country", "RO" + i.ToString());
                //doObj.Tags_Guid["myguid3"] = Guid.NewGuid();

                 await bucket.StoreAsync(doObj);
                //list.Add(doObj);
                /*if (i % 10 == 0 && i > 1)
                {
                    await bucket.StoreBatchAsync(list);
                    list = new List<CryptonorObject>();

                }*/

                //await bucket.StoreBatchAsync(list);

            }
        }
        void Form1_PullCompleted(object sender, PullCompletedEventArgs e)
        {
            
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
    public class Person
    {
        public Person()
        {

        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public Person Friend { get; set; }
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
