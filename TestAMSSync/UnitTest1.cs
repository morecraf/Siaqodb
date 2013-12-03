using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SiaqodbSyncMobile;
using System.Linq;
using System.Collections.Generic;
namespace TestAMSSync
{
    [TestClass]
    public class UnitTestAMSSync
    {
        [TestMethod]
        public async Task TestInsert()
        {//Test insert
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", "db1");
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            mob1.StoreObject(item);
            mob1.Flush();
            await mob1.SyncProvider.Synchronize();

            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", "db2");
            mob2.AddSyncType<TodoItem>("TodoItem");


            await mob2.SyncProvider.Synchronize();


            TodoItem todoItem = (from TodoItem todo in mob2
                      where todo.Id == item.Id
                      select todo).FirstOrDefault();

            Assert.AreEqual(item.Id, todoItem.Id);
            mob2.Close();
            mob1.Close();

        }
        [TestMethod]
        public async Task TestUpdate()
        {
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", "db1");
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            mob1.StoreObject(item);
            mob1.Flush();
            await mob1.SyncProvider.Synchronize();
            IList<TodoItem> all = mob1.LoadAll<TodoItem>();
            all[all.Count - 1].Text = "TestUpdate";
          
            string idUpdated = all[all.Count - 1].Id;
            mob1.StoreObject(all[all.Count - 1]);
            mob1.Flush();
            
            await mob1.SyncProvider.Synchronize();

            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", "db2");
            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();

            TodoItem todoItem = (from TodoItem todo in mob2
                      where todo.Id == idUpdated
                      select todo).FirstOrDefault();

            Assert.AreEqual("TestUpdate", todoItem.Text);

            TodoItem todoItemMob1 = (from TodoItem todo in mob1
                                    where todo.Id == idUpdated
                     select todo).FirstOrDefault();

            Assert.AreEqual("TestUpdate", todoItemMob1.Text);

            mob1.Close();
            mob2.Close();

        }


        [TestMethod]
        public async Task TestDelete()
        {//Test delete
           
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", "db1");
          
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            mob1.StoreObject(item);
           
            mob1.Flush();
            await mob1.SyncProvider.Synchronize();
            mob1.Delete(item);
            mob1.Flush();
            await mob1.SyncProvider.Synchronize();
            TodoItem deleted = (from TodoItem todo in mob1
                        where todo.Id == item.Id
                        select todo).FirstOrDefault();
            Assert.IsNull(deleted);

            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", "db2");
            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem deleted2 = (from TodoItem todo in mob2
                                where todo.Id == item.Id
                                select todo).FirstOrDefault();
            Assert.IsNull(deleted2);


        }

        [TestMethod]
        public async Task TestConflictUpdateUpdate()
        {//update on C1 and C2 at same time->should be conflict
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", "db1");
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            mob1.StoreObject(item);
            mob1.Flush();

            await mob1.SyncProvider.Synchronize();
            TodoItem itemSynced = (from TodoItem todo in mob1
                                where todo.Id == item.Id
                                select todo).FirstOrDefault();
            itemSynced.Text = "UpdatedFromC1";
            mob1.StoreObject(itemSynced);
            mob1.Flush();

            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", "db2");
            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem itemSynced2 = (from TodoItem todo in mob1
                                   where todo.Id == item.Id
                                   select todo).FirstOrDefault();
            itemSynced.Text = "UpdatedFromC2";
            mob2.StoreObject(itemSynced2);
            mob2.Flush();

            await mob1.SyncProvider.Synchronize();
            await mob2.SyncProvider.Synchronize();
            
            TodoItem todoItemMob1 = (from TodoItem todo in mob1
                        where todo.Id == item.Id 
                        select todo).FirstOrDefault();

            TodoItem todoItemMob2 = (from TodoItem todo in mob2
                                     where todo.Id == item.Id
                                     select todo).FirstOrDefault();


            Assert.AreEqual(todoItemMob1.Text, "UpdatedfromC1");
            Assert.AreEqual(todoItemMob2.Text, "UpdatedfromC1");
            mob1.Close();
            mob2.Close();

        }

        [TestMethod]
        public async Task TestConflictDeleteUpdate()
        {//delete on C1 and update on C2->conflict
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", "db1");
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            mob1.StoreObject(item);
            mob1.Flush();
            await mob1.SyncProvider.Synchronize();
            TodoItem itemSynced = (from TodoItem todo in mob1
                                    where todo.Id == item.Id
                                    select todo).FirstOrDefault();
          
            mob1.Delete(itemSynced);
            mob1.Flush();


            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
             "RLINABsktmvkzJMegbicNASWkzRzEW97", "db2");
           
            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem itemSyncedUpdate = (from TodoItem todo in mob2
                                    where todo.Id == item.Id
                                    select todo).FirstOrDefault();

            itemSyncedUpdate.Text = "updatedFromC2";
            mob2.StoreObject(itemSyncedUpdate);
          
            mob2.Flush();

            await mob1.SyncProvider.Synchronize();
            await mob2.SyncProvider.Synchronize();

            TodoItem deleted = (from TodoItem todo in mob1
                                         where todo.Id == item.Id
                                         select todo).FirstOrDefault();
            Assert.IsNull(deleted);

            TodoItem deleted2 = (from TodoItem todo in mob2
                                where todo.Id == item.Id
                                select todo).FirstOrDefault();
            Assert.IsNull(deleted2);

            mob1.Close();
            mob2.Close();

        }

        [TestMethod]
        public async Task TestConflictUpdateDelete()
        {
            //update on C1 and delete on C2->conflict
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", "db1");
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            mob1.StoreObject(item);
            mob1.Flush();
            await mob1.SyncProvider.Synchronize();
            TodoItem itemSynced = (from TodoItem todo in mob1
                                   where todo.Id == item.Id
                                   select todo).FirstOrDefault();

            itemSynced.Text = "updatedFromC1";
            mob1.StoreObject(itemSynced);
            mob1.Flush();


            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
             "RLINABsktmvkzJMegbicNASWkzRzEW97", "db2");

            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem itemSyncedDeleted = (from TodoItem todo in mob2
                                         where todo.Id == item.Id
                                         select todo).FirstOrDefault();

            mob2.Delete(itemSyncedDeleted);
            mob2.Flush();

            await mob1.SyncProvider.Synchronize();
            await mob2.SyncProvider.Synchronize();

            TodoItem updated = (from TodoItem todo in mob1
                                where todo.Id == item.Id
                                select todo).FirstOrDefault();
            Assert.AreEqual("updatedFromC1",updated.Text);

            TodoItem deleted = (from TodoItem todo in mob2
                                 where todo.Id == item.Id
                                 select todo).FirstOrDefault();
            
            Assert.AreEqual("updatedFromC1", deleted.Text);

            mob1.Close();
            mob2.Close();
        }

       
       [TestMethod]
        public async Task TestConflictDeleteDelete()
        {
            //delete on C1 and delete on C2->conflict-> nothing happens on clients
            Sqo.SiaqodbConfigurator.SetTrialLicense(@"SaZkK/2R2nqAjYg3udWHenOtS3b128RnUILgiTxuYRk=");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", "db1");
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            mob1.StoreObject(item);
            mob1.Flush();
            await mob1.SyncProvider.Synchronize();
            TodoItem itemSynced = (from TodoItem todo in mob1
                                   where todo.Id == item.Id
                                   select todo).FirstOrDefault();

            mob1.Delete(itemSynced);
            mob1.Flush();


            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
             "RLINABsktmvkzJMegbicNASWkzRzEW97", "db2");

            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem itemSyncedDeleted = (from TodoItem todo in mob2
                                         where todo.Id == item.Id
                                         select todo).FirstOrDefault();

            mob2.Delete(itemSyncedDeleted);
            mob2.Flush();

            await mob1.SyncProvider.Synchronize();
            await mob2.SyncProvider.Synchronize();
            TodoItem deleted = (from TodoItem todo in mob1
                                where todo.Id == item.Id
                                select todo).FirstOrDefault();
            Assert.IsNull(deleted);

            TodoItem deleted2 = (from TodoItem todo in mob2
                                 where todo.Id == item.Id
                                 select todo).FirstOrDefault();
            Assert.IsNull(deleted2);


            mob1.Close();
            mob2.Close();
        }
    }
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
}
