using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sqo.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbSyncMobile
{
    public class SiaqodbSyncMobileProvider
    {
        private SiaqodbMobile siaqodbMobile;
        private string applicationUrl;
        private string applicationKey;
        private MobileServiceClient MobileService;
        public SiaqodbSyncMobileProvider(SiaqodbMobile siaqodbMobile, string applicationUrl, string applicationKey)
        {
            
            this.siaqodbMobile = siaqodbMobile;
            this.applicationUrl = applicationUrl;
            this.applicationKey = applicationKey;
            MobileService = new MobileServiceClient(applicationUrl,applicationKey,null );
            SyncedTypes = new Dictionary<Type, string>();
        }
        public async Task Synchronize()
        {
            await UploadLocalChanges();
            await DownloadChanges();

        }
        public async Task Reinitialize()
        {
            foreach (Type t in SyncedTypes.Keys)
            {
                siaqodbMobile.DropType(t);
            }
            siaqodbMobile.DropType<DirtyEntity>();
            siaqodbMobile.DropType<Anchor>();
        }
        private async Task UploadLocalChanges()
        {
            IList<DirtyEntity> allDirtyItems = siaqodbMobile.LoadAll<DirtyEntity>();
            ILookup<string,DirtyEntity> lookup= allDirtyItems.ToLookup(a => a.EntityType);
            
            foreach (var item in lookup)
            {
                IEnumerable<DirtyEntity> entities = lookup[item.Key];
                Dictionary<int, Tuple<object, DirtyEntity>> inserts = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> updates = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> deletes = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Type type=Type.GetType(entities.First<DirtyEntity>().EntityType);
                if (!this.SyncedTypes.ContainsKey(type))
                {
                    continue;
                }
                string tableName = this.SyncedTypes[type];
                foreach (DirtyEntity en in entities)
                {
                    
                    if (en.IsTombstone)
                    {
                        if (inserts.ContainsKey(en.EntityOID))
                        {
                            siaqodbMobile.DeleteBase(inserts[en.EntityOID]);
                            siaqodbMobile.DeleteBase(en);
                            inserts.Remove(en.EntityOID);
                            continue;
                        }
                        else if (updates.ContainsKey(en.EntityOID))
                        {
                            siaqodbMobile.DeleteBase(updates[en.EntityOID]);
                            updates.Remove(en.EntityOID);
                        }
                    }
                    else
                    {
                        if (deletes.ContainsKey(en.EntityOID) || inserts.ContainsKey(en.EntityOID) || updates.ContainsKey(en.EntityOID))
                        {
                            siaqodbMobile.DeleteBase(en);
                            continue;
                        }
                    }


                    object entityFromDB = _bs._lobjby(siaqodbMobile, type, en.EntityOID);
                    int idValue = ReflectionHelper.GetIdValue(entityFromDB);
                    if (idValue == 0)//insert
                    {
                        inserts.Add(en.EntityOID,new Tuple<object,DirtyEntity>( entityFromDB,en));
                    }
                    else if (!en.IsTombstone)
                    {
                        updates.Add(en.EntityOID, new Tuple<object, DirtyEntity>(entityFromDB, en));
                    }
                    else
                    {
                        deletes.Add(en.EntityOID, new Tuple<object, DirtyEntity>(entityFromDB, en));
                    }
                    
                }
             


                IMobileServiceTable table = MobileService.GetTable(tableName);
                await UploadInserts(table, inserts);
                await UploadUpdates(table, updates);
                await UploadDeletes(table, deletes);

            }
            siaqodbMobile.DropType<DirtyEntity>();
           
        }

        private async Task UploadDeletes(IMobileServiceTable table, Dictionary<int, Tuple<object, DirtyEntity>> deletes)
        {
            if (deletes.Count > 0)
            {
               
                foreach (int entityOID in deletes.Keys)
                {
                    Tuple<object, DirtyEntity> tuple = deletes[entityOID];
                    object entityFromDB = tuple.Item1;
                    var serObj = MobileServiceTableSerializer.Serialize(entityFromDB);
                    JObject serializedObj = JObject.Parse(serObj.ToString());
                    await table.DeleteAsync(serializedObj);
                    siaqodbMobile.DeleteBase(tuple.Item2);
                }
               
            }
        }

        private async Task UploadUpdates(IMobileServiceTable table, Dictionary<int, Tuple<object, DirtyEntity>> updates)
        {
            if (updates.Count > 0)
            {
                var arr = new JArray();
                List<DirtyEntity> tobeDeleted = new List<DirtyEntity>();
                foreach (int entityOID in updates.Keys)
                {
                    Tuple<object, DirtyEntity> tuple = updates[entityOID];
                    object entityFromDB = tuple.Item1;
                    var serObj = MobileServiceTableSerializer.Serialize(entityFromDB);
                    //JObject serializedObj = JObject.Parse(serObj.ToString());
                    //await table.UpdateAsync(serializedObj);
                    arr.Add(serObj);
                    tobeDeleted.Add(tuple.Item2);
                }

                var body = new JObject() { { "id", 1 }, { table.TableName, arr } };
                var serializedArr = JObject.Parse(body.ToString());
                await table.UpdateAsync(serializedArr);
                tobeDeleted.ForEach(a => siaqodbMobile.DeleteBase(a));
            }
        }

        private async Task UploadInserts(IMobileServiceTable table, Dictionary<int, Tuple<object, DirtyEntity>> inserts)
        {
            if (inserts.Count > 0)
            {
                var arr = new JArray();
                List<DirtyEntity> tobeDeleted = new List<DirtyEntity>();
                foreach (int entityOID in inserts.Keys)
                {
                    Tuple<object, DirtyEntity> tuple = inserts[entityOID];
                    object entityFromDB = tuple.Item1;
                    var serObj = MobileServiceTableSerializer.Serialize(entityFromDB);
                    //JObject serializedObj = JObject.Parse(serObj.ToString());
                    //await table.InsertAsync(serializedObj);
                    tobeDeleted.Add(tuple.Item2);
                    arr.Add(serObj);
                }

                var body = new JObject() { { table.TableName, arr } };
                JObject serializedArr = JObject.Parse(body.ToString());
                await table.InsertAsync(serializedArr);
                tobeDeleted.ForEach(a => siaqodbMobile.DeleteBase(a));
            }
        }
        private async Task DownloadChanges()
        {
            foreach (Type t in SyncedTypes.Keys)
            {
                IMobileServiceTable table = MobileService.GetTable(SyncedTypes[t]);
                Anchor anchor = siaqodbMobile.Query<Anchor>().Where(anc => anc.EntityType == t.AssemblyQualifiedName).FirstOrDefault();//TODO
                string filter = "";
                if (anchor != null)
                {
                    string dateTimeString = new DateTime( anchor.TimeStamp.Ticks,DateTimeKind.Utc).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",CultureInfo.InvariantCulture);

                    filter="$filter=(TimeStamp gt "+ string.Format(CultureInfo.InvariantCulture,"datetime'{0}'",dateTimeString)+")";
                }
                var token = await table.ReadAsync(filter);
                //Type typeIList = typeof(List<>).MakeGenericType(t);
                //ConstructorInfo ctor = typeIList.GetConstructor(new Type[] { });
                //IList list = (IList)ctor.Invoke(new object[]{});
                DownloadedBatch serverEntities = JsonConvert.DeserializeObject(token.ToString(), typeof(DownloadedBatch)) as DownloadedBatch;
                if (serverEntities != null)
                {
                    siaqodbMobile.StartBulkInsert(t);
                    try
                    {
                        foreach (var entity in serverEntities.ItemsList)
                        {
                            object objEn= JsonConvert.DeserializeObject(((JObject)entity).ToString(), t);
                            siaqodbMobile.StoreDownloadedEntity(objEn);
                        }
                    }
                    finally
                    {
                        siaqodbMobile.EndBulkInsert(t);
                    }
                    if (anchor != null)
                    {
                        anchor.TimeStamp = serverEntities.TimeStamp;
                    }
                    else
                    {
                        anchor = new Anchor() { EntityType = t.AssemblyQualifiedName, TimeStamp = serverEntities.TimeStamp };
                    }
                    siaqodbMobile.StoreObjectBase(anchor);

                }
            }
            siaqodbMobile.Flush();
        }

        internal Dictionary<Type,string> SyncedTypes { get; set; }
        public void AddAsyncType<T>(string azure_table)
        {
            SyncedTypes.Add(typeof(T),azure_table);
        }

    }
}
