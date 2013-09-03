using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sqo;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SiaqodbConfigurator.SetLicense(@"qU3TtvA4T4L30VSlCCGUTXNXoKgzghhG5v8/UHPmMf8=");
            Siaqodb nop = new Siaqodb();

            await nop.OpenAsync(ApplicationData.Current.LocalFolder);
            //await nop.DropTypeAsync<Customer>();
            DateTime start = DateTime.Now;
            for (int i = 1; i < 10000; i++)
            {
                Customer c = new Customer();
                c.ID = i;
                c.Name = "ad" + i.ToString();
                //c.Vasiel = "momo" + i.ToString();
                await nop.StoreObjectAsync(c);
            }
            await nop.FlushAsync();
            string elapsedStore = (DateTime.Now - start).ToString();
            start = DateTime.Now;
            IObjectList<Customer> listC = await nop.LoadAllAsync<Customer>();
            string elapsedRead= (DateTime.Now - start).ToString();
            nop.Close();


        }
    }
    public class Customer
    {
 public int ID;
  
        public string Name;
        

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;

    }
}
