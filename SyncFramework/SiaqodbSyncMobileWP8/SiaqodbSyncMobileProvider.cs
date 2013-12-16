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
using Sqo;

namespace SiaqodbSyncMobile
{
    public class SiaqodbSyncMobileProvider
    {
        private SiaqodbMobile siaqodbMobile;
        private string applicationUrl;
        private string applicationKey;
        private MobileServiceClient MobileService;
        public event EventHandler<SyncProgressEventArgs> SyncProgress;
        public event EventHandler<SyncCompletedEventArgs> SyncCompleted;
        private SyncStatistics syncStatistics;
        private readonly AsyncLock _locker = new AsyncLock();

        public SiaqodbSyncMobileProvider(SiaqodbMobile siaqodbMobile, string applicationUrl, string applicationKey)
        {
            
            this.siaqodbMobile = siaqodbMobile;
            this.applicationUrl = applicationUrl;
            this.applicationKey = applicationKey;
            MobileService = new MobileServiceClient(applicationUrl,applicationKey,null );
            SyncedTypes = new Dictionary<Type, string>();
        }
#region public API
        public async Task Synchronize()
        {
            await _locker.LockAsync();
            try
            {
                syncStatistics = new SyncStatistics();
                syncStatistics.StartTime = DateTime.Now;


                this.OnSyncProgress(new SyncProgressEventArgs("Synchronization started..."));
                await UploadLocalChanges();
                await DownloadChanges();
                this.OnSyncProgress(new SyncProgressEventArgs("Synchronization finshed!"));
                syncStatistics.EndTime = DateTime.Now;
                this.OnSyncCompleted(new SyncCompletedEventArgs(null, syncStatistics));

            }
            catch (Exception ex)
            {
                syncStatistics.EndTime = DateTime.Now;
                this.OnSyncCompleted(new SyncCompletedEventArgs(ex, syncStatistics));
            }
            finally
            {
                _locker.Release();
            }
            
            

        }
        public async Task Reinitialize()
        {
             await _locker.LockAsync();
             try
             {
                 foreach (Type t in SyncedTypes.Keys)
                 {
                     await siaqodbMobile.DropTypeAsync(t);
                 }
                 await siaqodbMobile.DropTypeAsync<DirtyEntity>();
                 await siaqodbMobile.DropTypeAsync<Anchor>();
             }
             finally
             {
                 _locker.Release();
             }
        }
        internal Dictionary<Type, string> SyncedTypes { get; set; }
        public void AddAsyncType<T>(string azure_table)
        {
            SyncedTypes.Add(typeof(T), azure_table);
        }

#endregion

        #region private area
        private async Task UploadLocalChanges()
        {
            this.OnSyncProgress(new SyncProgressEventArgs("Get local changes..."));
           
            IList<DirtyEntity> allDirtyItems = await siaqodbMobile.LoadAllAsync<DirtyEntity>();
            ILookup<string,DirtyEntity> lookup= allDirtyItems.ToLookup(a => a.EntityType);
            this.OnSyncProgress(new SyncProgressEventArgs("Prepare uploads..."));
           
            foreach (var item in lookup)
            {
                IEnumerable<DirtyEntity> entities = lookup[item.Key];
                Dictionary<int, Tuple<object, DirtyEntity>> inserts = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> updates = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> deletes = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Type type=ReflectionHelper.GetTypeByDiscoveringName(entities.First<DirtyEntity>().EntityType);
                if (!this.SyncedTypes.ContainsKey(type))
                {
                    continue;
                }
                string tableName = this.SyncedTypes[type];
                foreach (DirtyEntity en in entities)
                {
                    
                    if (en.DirtyOp==DirtyOperation.Deleted)
                    {
                        if (inserts.ContainsKey(en.EntityOID))
                        {
                            siaqodbMobile.DeleteBase(inserts[en.EntityOID].Item1);
                            siaqodbMobile.DeleteBase(en);
                            inserts.Remove(en.EntityOID);
                            continue;
                        }
                        else if (updates.ContainsKey(en.EntityOID))
                        {
                            siaqodbMobile.DeleteBase(updates[en.EntityOID].Item1);
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
                    if (en.DirtyOp==DirtyOperation.Inserted)
                    {
                        inserts.Add(en.EntityOID,new Tuple<object,DirtyEntity>( entityFromDB,en));
                    }
                    else if (en.DirtyOp == DirtyOperation.Updated)
                    {
                        updates.Add(en.EntityOID, new Tuple<object, DirtyEntity>(entityFromDB, en));
                    }
                    else if (en.DirtyOp == DirtyOperation.Deleted)
                    {
                        deletes.Add(en.EntityOID, new Tuple<object, DirtyEntity>(entityFromDB, en));
                    }
                    
                }


                this.OnSyncProgress(new SyncProgressEventArgs("Start upload changes..."));
                IMobileServiceTable table = MobileService.GetTable(tableName);
                table.SystemProperties |= MobileServiceSystemProperties.All;

                this.OnSyncProgress(new SyncProgressEventArgs("Start upload inserts..."));
                await UploadInserts(table, inserts);
                this.OnSyncProgress(new SyncProgressEventArgs("Inserts uploads finished..."));
               
                this.OnSyncProgress(new SyncProgressEventArgs("Start upload updates..."));
                await UploadUpdates(table, updates);
                this.OnSyncProgress(new SyncProgressEventArgs("Updates uploads finished..."));

                this.OnSyncProgress(new SyncProgressEventArgs("Start upload deletes..."));
                await UploadDeletes(table, deletes);
                this.OnSyncProgress(new SyncProgressEventArgs("Deletes uploads finished..."));


                siaqodbMobile.Flush();
            }
            siaqodbMobile.DropType<DirtyEntity>();
            this.OnSyncProgress(new SyncProgressEventArgs("Uploads finsihed..."));

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
                    string timeStampValue = serializedObj.Property("__version").Value.ToString();
                    
                    
                    Dictionary<string,string> paramsAMS=GetParamsAMS();
                    paramsAMS.Add("ENTimeStamp", timeStampValue);
                    await table.DeleteAsync(serializedObj,paramsAMS);
                    siaqodbMobile.DeleteBase(tuple.Item2);
                }
                syncStatistics.TotalDeletedUploads = (uint)deletes.Count;
               
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

                var body = new JObject() { { "id", Guid.NewGuid().ToString() }, { table.TableName, arr } };
                //var serializedArr = JObject.Parse(body.ToString());
                var upd=await table.UpdateAsync(body,GetParamsAMS());
                foreach (var a in tobeDeleted)
                {
                    siaqodbMobile.DeleteBase(a);
                }
                

                syncStatistics.TotalUpdatedUploads = (uint)updates.Count;
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
                    serializedObj.Remove("__version");
                    tobeDeleted.Add(tuple.Item2);
                    arr.Add(serializedObj);
                }

                var body = new JObject() { { table.TableName, arr } };
               
                var r=await table.InsertAsync(body,GetParamsAMS());
                foreach (var a in tobeDeleted)
                {
                    siaqodbMobile.DeleteBase(a);
                }
                syncStatistics.TotalInsertedUploads = (uint)inserts.Count;
            }
        }
        private async Task DownloadChanges()
        {
            this.OnSyncProgress(new SyncProgressEventArgs("Downloading changes from server..."));
            foreach (Type t in SyncedTypes.Keys)
            {
                IMobileServiceTable table = MobileService.GetTable(SyncedTypes[t]);
                table.SystemProperties |= MobileServiceSystemProperties.All;

                Anchor anchor = siaqodbMobile.Query<Anchor>().Where(anc => anc.EntityType == ReflectionHelper.GetDiscoveringTypeName(t)).FirstOrDefault();
                string filter = "";
                string anchorJSON = "";
                if (anchor != null)
                {
                    string dateTimeString = new DateTime( anchor.TimeStamp.Ticks,DateTimeKind.Utc).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",CultureInfo.InvariantCulture);

                    filter="$filter=(__updatedAt gt "+ string.Format(CultureInfo.InvariantCulture,"datetime'{0}'",dateTimeString)+")";
                    anchorJSON = dateTimeString;//JsonConvert.SerializeObject(anchor.TimeStamp);
                }
                Dictionary<string,string> paramAMS=GetParamsAMS();
                paramAMS.Add("Anchor", anchorJSON);
                var token = await table.ReadAsync(filter, paramAMS);
                //Type typeIList = typeof(List<>).MakeGenericType(t);
                //ConstructorInfo ctor = typeIList.GetConstructor(new Type[] { });
                //IList list = (IList)ctor.Invoke(new object[]{});
                this.OnSyncProgress(new SyncProgressEventArgs("Items downloaded,start store locally..."));
           
                DownloadedBatch serverEntities = JsonConvert.DeserializeObject(token.ToString(), typeof(DownloadedBatch)) as DownloadedBatch;
                if (serverEntities != null)
                {
                    siaqodbMobile.StartBulkInsert(t);
                    try
                    {
                        if (serverEntities.ItemsList != null)
                        {
                            syncStatistics.TotalDownloads = (uint)serverEntities.ItemsList.Count;
                            foreach (var entity in serverEntities.ItemsList)
                            {
                                object objEn = JsonConvert.DeserializeObject(((JObject)entity).ToString(), t);
                                siaqodbMobile.StoreDownloadedEntity(objEn);
                            }
                        }
                        if (serverEntities.TombstoneList != null)
                        {
                            syncStatistics.TotalDownloads += (uint)serverEntities.TombstoneList.Count;
                           
                            foreach (var delEntity in serverEntities.TombstoneList)
                            {
                                var delEnJ = (JObject)delEntity;
                                JToken ENId = delEnJ.GetValue("enid");

                                Dictionary<string, object> criteria = new Dictionary<string, object>();
                                criteria.Add(ExternalMetaHelper.GetBackingField(ReflectionHelper.GetIdProperty(t)), ENId.ToString());
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
                        anchor = new Anchor() { EntityType = ReflectionHelper.GetDiscoveringTypeName(t), TimeStamp = serverEntities.TimeStamp };
                    }
                    siaqodbMobile.StoreObjectBase(anchor);

                }
            }
            siaqodbMobile.Flush();
            this.OnSyncProgress(new SyncProgressEventArgs("Download and store locally finished..."));
           
        }

        
        private Dictionary<string, string> GetParamsAMS()
        {
            Dictionary<string, string> params_toAMS = new Dictionary<string, string>();
            params_toAMS.Add("IsSiaqodbSync", "true");
            return params_toAMS;
        }
        #endregion

        
        internal void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress != null)
            {
                this.SyncProgress(this, args);
            }
        }
        internal void OnSyncCompleted(SyncCompletedEventArgs args)
        {
            if (this.SyncCompleted != null)
            {
                this.SyncCompleted(this, args);
            }
        }
    }
}
