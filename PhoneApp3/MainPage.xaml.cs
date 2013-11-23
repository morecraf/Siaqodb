using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneApp3.Resources;
using System.Runtime.Serialization;
using SiaqodbSyncMobile;
using Newtonsoft.Json;

namespace PhoneApp3
{
    public class TodoItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }

        [JsonProperty(PropertyName = "__version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "__updatedAt")]
        public DateTime ServerUpdatedAt { get; set; }

        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
    }
      
    
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int?[] nullableArray = new int?[10];
            nullableArray[0] = null;
            nullableArray[1] = 1;
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"FSCVmMEntdr+nhefoUYc9n8KJFWa1I/GZ2key5jVmDA=");
            SiaqodbMobile mob = new SiaqodbMobile("https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", "mydasbA81");
            mob.AddSyncType<TodoItem>("TodoItem");
            for (int i = 0; i < 2; i++)
            {
                TodoItem item = new TodoItem();
                item.Text = "GSP" + i.ToString();
                item.Id = Guid.NewGuid().ToString();
                //mob.StoreObject(item);
            }
            var items2 = mob.LoadAll<TodoItem>();
            
            int y = 0;
            foreach (var ai in items2)
            {
                if (ai.Id == "23AB5750-A61C-43F7-ACA3-1189E5E7FB1B")
                {
                    ai.Text = "ionescu77";
                    ai.Complete = false;
                    mob.StoreObject(ai);
                }
                y++;
            }
            mob.Flush();
            items2 = mob.LoadAll<TodoItem>();
            
            try
            {
                await mob.SyncProvider.Synchronize();
                var items= mob.LoadAll<TodoItem>();
            }
            catch (Exception ex)
            { 
            
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}