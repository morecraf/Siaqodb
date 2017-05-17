using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Android.App;
using Android.Widget;
using Android.OS;

using Sqo;
using SiaqodbSyncProvider;

using DefaultScope;

namespace TestsSiaqodbSync.Xamarin.Android
{
    [Activity(Label = "TestsSiaqodbSync.Xamarin.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        SiaqodbOffline siaqodbOffline;
        private static Random random = new Random();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Sqo.SiaqodbConfigurator.SetLicense(@"QHL0KapKSI4W7dBGrUtLO8lAc0ug0XZNxwFcVVC6KikS2RG+e+qTW0ow+MiuhJRMes+og9xAhYUGO80wI2GbZw==");
            var objPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            siaqodbOffline = new SiaqodbOffline(objPath, new Uri(@"http://10.0.2.2:49325/DefaultScopeSyncService.svc/"));
            siaqodbOffline.SyncProvider.CacheController.ControllerBehavior.SerializationFormat = Microsoft.Synchronization.ClientServices.SerializationFormat.ODataJSON;
            siaqodbOffline.AddTypeForSync<Tag>();
            siaqodbOffline.AddTypeForSync<Priority>();
            siaqodbOffline.AddTypeForSync<Status>();
            siaqodbOffline.AddTypeForSync<User>();
            siaqodbOffline.AddTypeForSync<List>();
            siaqodbOffline.AddTypeForSync<Item>();
            siaqodbOffline.AddTypeForSync<TagItemMapping>();

            siaqodbOffline.SyncProgress += new EventHandler<SyncProgressEventArgs>(siaqodbOffline_SyncProgress);
            siaqodbOffline.SyncCompleted += new EventHandler<SyncCompletedEventArgs>(siaqodbOffline_SyncCompleted);

            Button button = FindViewById<Button>(Resource.Id.button1);
            button.Click += Button_Click;

            var label = FindViewById<TextView>(Resource.Id.textView1);
            label.Text = "Sync...";
            siaqodbOffline.Synchronize();
        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            // add a new object, then store it and syncronize
            Tag obj = new Tag();
            obj.ID = 20;
            obj.Name = RandomString(5);
            siaqodbOffline.StoreObject(obj);

            var label = FindViewById<TextView>(Resource.Id.textView1);
            label.Text = "Sync...";
            siaqodbOffline.Synchronize();
        }

        void siaqodbOffline_SyncCompleted(object sender, SyncCompletedEventArgs e)
        {
            IList<Tag> tags = siaqodbOffline.LoadAll<Tag>();
            IList<Status> status = siaqodbOffline.LoadAll<Status>();
            var label = FindViewById<TextView>(Resource.Id.textView1);
            label.Text = "Finished; Tag count: " + tags.Count + "; status:" + status.Count;
        }

        void siaqodbOffline_SyncProgress(object sender, SyncProgressEventArgs e)
        {
            var label = FindViewById<TextView>(Resource.Id.textView1);
            label.Text = e.Message;
        }

        /// <summary>
        /// Generate a random string
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}

