using Sqo;
using Sqo.Transactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public class Invoice
    {

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public string CustomerName { get; set; }

        public List<InvoiceItem> InvoiceDetails { get; set; }

        public double Discount { get; set; }
    }

    public class InvoiceItem
    {

        public string Description { get; set; }

        public int Quantity { get; set; }

        public double UnitPrice { get; set; }
    }

    static class Program
    {
      

        public static void Main(string[] args)
        {

            // create sample invoice 
            Invoice inv = new Invoice();
            inv.Date = DateTime.Now;
            inv.Description = "Sample Invoice";
            inv.CustomerName = "Customer 001";


            InvoiceItem item1 = new InvoiceItem();
            item1.Description = "Keyboard";
            item1.Quantity = 4;
            item1.UnitPrice = 10;


            InvoiceItem item2 = new InvoiceItem();
            item1.Description = "Speakers";
            item1.Quantity = 3;
            item1.UnitPrice = 60;

            inv.InvoiceDetails = new List<InvoiceItem>();
            inv.InvoiceDetails.AddRange(new InvoiceItem[] { item1, item2 });

            AddInvoice(inv);

        }


        public static void AddInvoice(Invoice inv)
        {

            // data operations
            SiaqodbConfigurator.SetLicense("7Jp04UF3sV6K1/eQd/VSBiGMtfktOp6/nNsxPT66lsCQLSEvH80ndRsxnHrxQAPl");
            Siaqodb db = new Siaqodb(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SiaqoDBSample"));

            ITransaction dbTrans = null;

            try
            {
                // start a transaction
                dbTrans = db.BeginTransaction();


                // get total purchases  (this will raise an exception since there will be two ITransaction objects created on the same Siaqodb instance)
                double totalPurchases = (from Invoice invoice in db
                                         where invoice.CustomerName == inv.CustomerName
                                         select invoice.InvoiceDetails.Sum(item => item.Quantity * item.UnitPrice * (1 - invoice.Discount))).Sum();



                // calculate discount based on purchases volume
                if (totalPurchases >= 0 && totalPurchases <= 999)
                {
                    inv.Discount = 0.05;
                }
                else if (totalPurchases >= 1000 && totalPurchases <= 4999)
                {
                    inv.Discount = 0.1;
                }
                else if (totalPurchases >= 5000 && totalPurchases <= 9999)
                {
                    inv.Discount = 0.15;
                }
                else
                {
                    inv.Discount = 0.2;
                }


                // store invoice items
                foreach (InvoiceItem item in inv.InvoiceDetails)
                {
                    db.StoreObject(item, dbTrans);
                }

                // store the invoice object
                db.StoreObject(inv, dbTrans);

                // commit the transaction
                dbTrans.Commit();

                Console.WriteLine("Invoice added successfully");
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();
                Console.WriteLine(string.Format("Error while adding invoice, {0}", ex.Message));
            }

        }
    }
}
