using Newtonsoft.Json;
using Sqo;
using Sqo.Documents;
using System;
using System.Text;


namespace ConsoleProgram1
{
    internal class MyJsonSerializer : IDocumentSerializer
    {

        public object Deserialize(Type type, byte[] objectBytes)
        {
            string jsonStr = Encoding.UTF8.GetString(objectBytes);
            return JsonConvert.DeserializeObject(jsonStr.TrimEnd('\0'), type);
        }
        public byte[] Serialize(object obj)
        {
            string jsonStr = JsonConvert.SerializeObject(obj, Formatting.Indented);
            return Encoding.UTF8.GetBytes(jsonStr);
        }

    }
    public class Invoice
    {
        public string CustomerName { get; set; }
        public int InvoiceNumber { get; set; }
        public decimal Total { get; set; }
        public DateTime InvoiceDate { get; set; }

    }

    static class Program2
    {
        public static void Main(string[] args)
        {

            SiaqodbConfigurator.SetDocumentSerializer(new MyJsonSerializer());

            using (Siaqodb siaqodb = new Siaqodb(@"c:\work\temp\_yyy\"))
            {
                IBucket bucket = siaqodb.Documents["invoices"];

                Invoice inv = new Invoice { CustomerName = "My Company", InvoiceDate = DateTime.Now, Total = 2390 };

                Document document = new Document();
                document.Key = "Invoice-324r";
                document.SetContent<Invoice>(inv);

                bucket.Store(document);

                Document documentLoaded = bucket.Load("Invoice-324r");
                Invoice invoiceLoaded = documentLoaded.GetContent<Invoice>();
                invoiceLoaded.InvoiceDate = DateTime.Now.AddDays(-1);
                documentLoaded.SetContent<Invoice>(invoiceLoaded);

                bucket.Store(documentLoaded);

                bucket.Delete(documentLoaded);
            }




        }
    }
}
