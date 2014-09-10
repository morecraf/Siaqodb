using Cryptonor.Queries;
using CryptonorClient;
using Sqo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace App2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
           // QueryOptions qo=new QueryOptions(
            /*Siaqodb siaqodb = new Siaqodb();
            siaqodb.Open(ApplicationData.Current.LocalFolder);
            siaqodb.StoreObject(new A() { Name = "nnn" });
            var q = siaqodb.LoadAll<A>();
            var a = "ass";*/
            var cl = new CryptonorClient.CryptonorClient("http://localhost:53411/", "excelsior", "mykey", "mypwd");
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var subfolder = await folder.CreateFolderAsync("unit_tests", CreationCollisionOption.OpenIfExists);
            IBucket bucket = cl.GetLocalBucket("unit_tests", folder.Path);

            DateTime start = DateTime.Now;

            //await this.Fill();

            string elapsed = (DateTime.Now - start).ToString();

            start = DateTime.Now;
            try
            {
                ((CryptonorLocalBucket)bucket).PullCompleted += Form1_PullCompleted;
                await ((CryptonorLocalBucket)bucket).Pull();
            }
            catch
            {

            }
            elapsed = (DateTime.Now - start).ToString();
            start = DateTime.Now;
            var all =await bucket.GetAll();
            string a = "";
        }
        void Form1_PullCompleted(object sender, PullCompletedEventArgs e)
        {

        }
    }
    public class A
    {
        public string Name;
    }
}
