using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using SiaqodbSyncMobile;
using Windows.Storage;
using Sqo;
namespace TestAMSSyncWinRT
{
    [TestClass]
    public class UnitTestAMSSync
    {
        StorageFolder dbFolder = ApplicationData.Current.LocalFolder;
        [TestMethod]
        public async Task TestInsert()
        {//Test insert
            StorageFolder db1=await dbFolder.CreateFolderAsync("db1",CreationCollisionOption.OpenIfExists);
            StorageFolder db2 = await dbFolder.CreateFolderAsync("db2", CreationCollisionOption.OpenIfExists);
             Sqo.SiaqodbConfigurator.SetLicense(@"Gq28hR1vXcjfLE0L/2WyWF8+9x9h0f5hA0suJhJ2B79Zh6+jE0+ib30G7C1Wq9mp");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", db1);
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            await mob1.StoreObjectAsync(item);
           await  mob1.FlushAsync();
            await mob1.SyncProvider.Synchronize();

            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", db2);
            mob2.AddSyncType<TodoItem>("TodoItem");


            await mob2.SyncProvider.Synchronize();


            TodoItem todoItem = await (from TodoItem todo in mob2
                                 where todo.Id == item.Id
                                 select todo).FirstOrDefaultAsync();

            Assert.AreEqual(item.Id, todoItem.Id);
            await mob2.CloseAsync();
            await mob1.CloseAsync();

        }
        [TestMethod]
        public async Task TestUpdate()
        {
            StorageFolder db1 = await dbFolder.CreateFolderAsync("db1", CreationCollisionOption.OpenIfExists);
            StorageFolder db2 = await dbFolder.CreateFolderAsync("db2", CreationCollisionOption.OpenIfExists);
             Sqo.SiaqodbConfigurator.SetLicense(@"Gq28hR1vXcjfLE0L/2WyWF8+9x9h0f5hA0suJhJ2B79Zh6+jE0+ib30G7C1Wq9mp");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", db1);
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            await mob1.StoreObjectAsync(item);
           await  mob1.FlushAsync();
            await mob1.SyncProvider.Synchronize();
            IList<TodoItem> all = mob1.LoadAll<TodoItem>();
            all[all.Count - 1].Text = "TestUpdate";

            string idUpdated = all[all.Count - 1].Id;
            await mob1.StoreObjectAsync(all[all.Count - 1]);
           await  mob1.FlushAsync();

            await mob1.SyncProvider.Synchronize();

            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", db2);
            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();

            TodoItem todoItem = await (from TodoItem todo in mob2
                                 where todo.Id == idUpdated
                                 select todo).FirstOrDefaultAsync();

            Assert.AreEqual("TestUpdate", todoItem.Text);

            TodoItem todoItemMob1 = await (from TodoItem todo in mob1
                                     where todo.Id == idUpdated
                                     select todo).FirstOrDefaultAsync();

            Assert.AreEqual("TestUpdate", todoItemMob1.Text);

            await mob1.CloseAsync();
            await mob2.CloseAsync();

        }


        [TestMethod]
        public async Task TestDelete()
        {//Test delete
            StorageFolder db1 = await dbFolder.CreateFolderAsync("db1", CreationCollisionOption.OpenIfExists);
            StorageFolder db2 = await dbFolder.CreateFolderAsync("db2", CreationCollisionOption.OpenIfExists);
             Sqo.SiaqodbConfigurator.SetLicense(@"Gq28hR1vXcjfLE0L/2WyWF8+9x9h0f5hA0suJhJ2B79Zh6+jE0+ib30G7C1Wq9mp");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", db1);

            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            await mob1.StoreObjectAsync(item);

           await  mob1.FlushAsync();
            await mob1.SyncProvider.Synchronize();
            await mob1.DeleteAsync(item);
           await  mob1.FlushAsync();
            await mob1.SyncProvider.Synchronize();
            TodoItem deleted = await (from TodoItem todo in mob1
                                where todo.Id == item.Id
                                select todo).FirstOrDefaultAsync();
            Assert.IsNull(deleted);


            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
           "RLINABsktmvkzJMegbicNASWkzRzEW97", db2);
            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem deleted2 = await (from TodoItem todo in mob2
                                 where todo.Id == item.Id
                                 select todo).FirstOrDefaultAsync();
            Assert.IsNull(deleted2);


        }

        [TestMethod]
        public async Task TestConflictUpdateUpdate()
        {//update on C1 and C2 at same time->should be conflict
            StorageFolder db1 = await dbFolder.CreateFolderAsync("db1", CreationCollisionOption.OpenIfExists);
            StorageFolder db2 = await dbFolder.CreateFolderAsync("db2", CreationCollisionOption.OpenIfExists);
             Sqo.SiaqodbConfigurator.SetLicense(@"Gq28hR1vXcjfLE0L/2WyWF8+9x9h0f5hA0suJhJ2B79Zh6+jE0+ib30G7C1Wq9mp");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", db1);
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            await mob1.StoreObjectAsync(item);
           await  mob1.FlushAsync();

            await mob1.SyncProvider.Synchronize();
            TodoItem itemSynced = await (from TodoItem todo in mob1
                                   where todo.Id == item.Id
                                   select todo).FirstOrDefaultAsync();
            itemSynced.Text = "UpdatedFromC1";
            await mob1.StoreObjectAsync(itemSynced);
           await  mob1.FlushAsync();

            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", db2);
            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem itemSynced2 = await (from TodoItem todo in mob1
                                    where todo.Id == item.Id
                                    select todo).FirstOrDefaultAsync();
            itemSynced.Text = "UpdatedFromC2";
            await mob2.StoreObjectAsync(itemSynced2);
            await mob2.FlushAsync();

            await mob1.SyncProvider.Synchronize();
            await mob2.SyncProvider.Synchronize();

            TodoItem todoItemMob1 = await (from TodoItem todo in mob1
                                     where todo.Id == item.Id
                                     select todo).FirstOrDefaultAsync();

            TodoItem todoItemMob2 = await (from TodoItem todo in mob2
                                     where todo.Id == item.Id
                                     select todo).FirstOrDefaultAsync();


            Assert.AreEqual(todoItemMob1.Text, "UpdatedFromC1");
            Assert.AreEqual(todoItemMob2.Text, "UpdatedFromC1");
            await mob1.CloseAsync();
            await mob2.CloseAsync();

        }

        [TestMethod]
        public async Task TestConflictDeleteUpdate()
        {//delete on C1 and update on C2->conflict
            StorageFolder db1 = await dbFolder.CreateFolderAsync("db1", CreationCollisionOption.OpenIfExists);
            StorageFolder db2 = await dbFolder.CreateFolderAsync("db2", CreationCollisionOption.OpenIfExists);
             Sqo.SiaqodbConfigurator.SetLicense(@"Gq28hR1vXcjfLE0L/2WyWF8+9x9h0f5hA0suJhJ2B79Zh6+jE0+ib30G7C1Wq9mp");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", db1);
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            await mob1.StoreObjectAsync(item);
           await  mob1.FlushAsync();
            await mob1.SyncProvider.Synchronize();
            TodoItem itemSynced = await (from TodoItem todo in mob1
                                   where todo.Id == item.Id
                                   select todo).FirstOrDefaultAsync();

            await mob1.DeleteAsync(itemSynced);
           await  mob1.FlushAsync();


            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
             "RLINABsktmvkzJMegbicNASWkzRzEW97", db2);

            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem itemSyncedUpdate = await (from TodoItem todo in mob2
                                         where todo.Id == item.Id
                                         select todo).FirstOrDefaultAsync();

            itemSyncedUpdate.Text = "updatedFromC2";
            await mob2.StoreObjectAsync(itemSyncedUpdate);

            await mob2.FlushAsync();

            await mob1.SyncProvider.Synchronize();

            TodoItem deleted = await (from TodoItem todo in mob1
                                where todo.Id == item.Id
                                select todo).FirstOrDefaultAsync();
            Assert.IsNull(deleted);

            await mob2.SyncProvider.Synchronize();
            await mob2.FlushAsync();

            TodoItem deleted2 = await (from TodoItem todo in mob2
                                 where todo.Id == item.Id
                                 select todo).FirstOrDefaultAsync();
            Assert.IsNull(deleted2);

            await mob1.CloseAsync();
            await mob2.CloseAsync();

        }

        [TestMethod]
        public async Task TestConflictUpdateDelete()
        {
            //update on C1 and delete on C2->conflict
            StorageFolder db1 = await dbFolder.CreateFolderAsync("db1", CreationCollisionOption.OpenIfExists);
            StorageFolder db2 = await dbFolder.CreateFolderAsync("db2", CreationCollisionOption.OpenIfExists);
             Sqo.SiaqodbConfigurator.SetLicense(@"Gq28hR1vXcjfLE0L/2WyWF8+9x9h0f5hA0suJhJ2B79Zh6+jE0+ib30G7C1Wq9mp");
            SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
            "RLINABsktmvkzJMegbicNASWkzRzEW97", db1);
            mob1.AddSyncType<TodoItem>("TodoItem");

            TodoItem item = new TodoItem();
            item.Text = "Testitem1";
            item.Id = Guid.NewGuid().ToString();
            await mob1.StoreObjectAsync(item);
           await  mob1.FlushAsync();
            await mob1.SyncProvider.Synchronize();
            TodoItem itemSynced = await (from TodoItem todo in mob1
                                   where todo.Id == item.Id
                                   select todo).FirstOrDefaultAsync();

            itemSynced.Text = "updatedFromC1";
            await mob1.StoreObjectAsync(itemSynced);
           await  mob1.FlushAsync();


            SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
             "RLINABsktmvkzJMegbicNASWkzRzEW97", db2);

            mob2.AddSyncType<TodoItem>("TodoItem");
            await mob2.SyncProvider.Synchronize();
            TodoItem itemSyncedDeleted = await (from TodoItem todo in mob2
                                          where todo.Id == item.Id
                                          select todo).FirstOrDefaultAsync();

            await mob2.DeleteAsync(itemSyncedDeleted);
            await mob2.FlushAsync();

            await mob1.SyncProvider.Synchronize();
            await mob2.SyncProvider.Synchronize();

            TodoItem updated = await (from TodoItem todo in mob1
                                where todo.Id == item.Id
                                select todo).FirstOrDefaultAsync();
            Assert.AreEqual("updatedFromC1", updated.Text);

            TodoItem deleted = await (from TodoItem todo in mob2
                                where todo.Id == item.Id
                                select todo).FirstOrDefaultAsync();

            Assert.AreEqual("updatedFromC1", deleted.Text);

            await mob1.CloseAsync();
            await mob2.CloseAsync();
        }


        [TestMethod]
        public async Task TestConflictDeleteDelete()
        {
            try
            {
                //delete on C1 and delete on C2->conflict-> nothing happens on clients
                StorageFolder db1 = await dbFolder.CreateFolderAsync("db1", CreationCollisionOption.OpenIfExists);
                StorageFolder db2 = await dbFolder.CreateFolderAsync("db2", CreationCollisionOption.OpenIfExists);
                 Sqo.SiaqodbConfigurator.SetLicense(@"Gq28hR1vXcjfLE0L/2WyWF8+9x9h0f5hA0suJhJ2B79Zh6+jE0+ib30G7C1Wq9mp");
                SiaqodbMobile mob1 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
                "RLINABsktmvkzJMegbicNASWkzRzEW97", db1);
                mob1.AddSyncType<TodoItem>("TodoItem");

                TodoItem item = new TodoItem();
                item.Text = "Testitem1";
                item.Id = Guid.NewGuid().ToString();
                await mob1.StoreObjectAsync(item);
                await mob1.FlushAsync();
                await mob1.SyncProvider.Synchronize();
                TodoItem itemSynced = await (from TodoItem todo in mob1
                                             where todo.Id == item.Id
                                             select todo).FirstOrDefaultAsync();

                await mob1.DeleteAsync(itemSynced);
                await mob1.FlushAsync();


                SiaqodbMobile mob2 = new SiaqodbMobile(@"https://cristidot.azure-mobile.net/",
                 "RLINABsktmvkzJMegbicNASWkzRzEW97", db2);

                mob2.AddSyncType<TodoItem>("TodoItem");
                await mob2.SyncProvider.Synchronize();
                TodoItem itemSyncedDeleted = await (from TodoItem todo in mob2
                                                    where todo.Id == item.Id
                                                    select todo).FirstOrDefaultAsync();

                await mob2.DeleteAsync(itemSyncedDeleted);
                await mob2.FlushAsync();

                await mob1.SyncProvider.Synchronize();
                await mob2.SyncProvider.Synchronize();
                TodoItem deleted = await (from TodoItem todo in mob1
                                          where todo.Id == item.Id
                                          select todo).FirstOrDefaultAsync();
                Assert.IsNull(deleted);

                TodoItem deleted2 = await (from TodoItem todo in mob2
                                           where todo.Id == item.Id
                                           select todo).FirstOrDefaultAsync();
                Assert.IsNull(deleted2);


                await mob1.CloseAsync();
                await mob2.CloseAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
