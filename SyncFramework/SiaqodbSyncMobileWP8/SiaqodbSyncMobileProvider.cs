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
using Sqo.Utilities;

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

                siaqodbMobile.Flush();
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
                    var serObj = Newtonsoft.Json.JsonConvert.SerializeObject(entityFromDB);
                    JObject serializedObj = JObject.Parse(serObj.ToString());
                    string timeStampValue = "";
                    foreach (var props in serializedObj.Properties())
                    {
                        if (string.Compare(props.Name, "TimeStamp", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            timeStampValue = props.Value.ToString();
                           
                        }
                    }
                    
                    Dictionary<string,string> paramsAMS=GetParamsAMS();
                    paramsAMS.Add("ENTimeStamp", timeStampValue);
                    await table.DeleteAsync(serializedObj,paramsAMS);
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
                    var serObj = Newtonsoft.Json.JsonConvert.SerializeObject(entityFromDB);
                    JObject serializedObj = JObject.Parse(serObj.ToString());
                   
                    
                    //var updated=await table.UpdateAsync(serializedObj);
                    arr.Add(serializedObj);
                    tobeDeleted.Add(tuple.Item2);
                }

                var body = new JObject() { { "id", 1 }, { table.TableName, arr } };
                //var serializedArr = JObject.Parse(body.ToString());
                var upd=await table.UpdateAsync(body,GetParamsAMS());
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
                    var serObj = Newtonsoft.Json.JsonConvert.SerializeObject(entityFromDB);
                    JObject serializedObj = JObject.Parse(serObj.ToString());
                    string IdPropName = "Id";
                    foreach (var props in serializedObj.Properties())
                    {
                        if (string.Compare(props.Name, "id", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            IdPropName = props.Name;
                        }
                    }
                    serializedObj.Remove(IdPropName);
                    //await table.InsertAsync(serializedObj);
                    tobeDeleted.Add(tuple.Item2);
                    arr.Add(serializedObj);
                }

                var body = new JObject() { { table.TableName, arr } };
               
                var r=await table.InsertAsync(body,GetParamsAMS());
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
                string anchorJSON = "";
                if (anchor != null)
                {
                    string dateTimeString = new DateTime( anchor.TimeStamp.Ticks,DateTimeKind.Utc).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",CultureInfo.InvariantCulture);

                    filter="$filter=(TimeStamp gt "+ string.Format(CultureInfo.InvariantCulture,"datetime'{0}'",dateTimeString)+")";
                    anchorJSON = dateTimeString;//JsonConvert.SerializeObject(anchor.TimeStamp);
                }
                Dictionary<string,string> paramAMS=GetParamsAMS();
                paramAMS.Add("Anchor", anchorJSON);
                var token = await table.ReadAsync(filter, paramAMS);
                //Type typeIList = typeof(List<>).MakeGenericType(t);
                //ConstructorInfo ctor = typeIList.GetConstructor(new Type[] { });
                //IList list = (IList)ctor.Invoke(new object[]{});
                DownloadedBatch serverEntities = JsonConvert.DeserializeObject(token.ToString(), typeof(DownloadedBatch)) as DownloadedBatch;
                if (serverEntities != null)
                {
                    siaqodbMobile.StartBulkInsert(t);
                    try
                    {
                        if (serverEntities.ItemsList != null)
                        {
                            foreach (var entity in serverEntities.ItemsList)
                            {
                                object objEn = JsonConvert.DeserializeObject(((JObject)entity).ToString(), t);
                                siaqodbMobile.StoreDownloadedEntity(objEn);
                            }
                        }
                        if (serverEntities.TombstoneList != null)
                        {
                            foreach (var delEntity in serverEntities.TombstoneList)
                            {
                                var delEnJ = (JObject)delEntity;
                                JToken ENId = delEnJ.GetValue("ENId");

                                Dictionary<string, object> criteria = new Dictionary<string, object>();
                                criteria.Add(ExternalMetaHelper.GetBackingField(ReflectionHelper.GetIdProperty(t)), Convert.ToInt32(ENId.ToString()));
                                int nrDeleted = siaqodbMobile.DeleteObjectByBase(t, criteria);
                            }
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
        private Dictionary<string, string> GetParamsAMS()
        {
            Dictionary<string, string> params_toAMS = new Dictionary<string, string>();
            params_toAMS.Add("IsSiaqodbSync", "true");
            return params_toAMS;
        }

    }
}
