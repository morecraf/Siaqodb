using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.Indexes;
using Sqo.Transactions;
using System.Collections;
using System.Linq.Expressions;
using System.IO;
using Sqo;
using Cryptonor.Indexes;
using Sqo.Utilities;
#if ASYNC
using System.Threading.Tasks;
#endif

#if WinRT

using Windows.Storage;
#endif
namespace Cryptonor
{
    public class CryptonorLocalDB 
    {
        private Siaqodb siaqodb;
        TagsIndexManager indexManager;
        private readonly object _locker = new object();
#if ASYNC
        private readonly AsyncLock _lockerAsync = new AsyncLock();
#endif
#if !WinRT
        public CryptonorLocalDB(string bucketPath)
        {
            SiaqodbConfigurator.SetLicense(@"8+9xiEEUL3aP1P5F1pkpCZQq2RoRgE5czRvwXgQIHBE=");
            this.siaqodb = new Siaqodb(bucketPath);
            indexManager = new TagsIndexManager(this.siaqodb);
           
           
        }
#else
        public CryptonorLocalDB(StorageFolder bucketPath)
        {
            SiaqodbConfigurator.SetLicense(@"6mQill2r3RSwk/Nl9ZOBOoHDBrsSTUN8ZYGqWmmbxwuOHcOPjeEzLiacv5kTxM3Z");
            this.siaqodb = new Siaqodb();
            this.siaqodb.Open(bucketPath);
            indexManager = new TagsIndexManager(this.siaqodb);
           
           
        }
       
#endif
        private void CreateDirtyEntity(object obj, DirtyOperation dop)
        {
            this.CreateDirtyEntity(obj, dop, null);
        }
#if ASYNC
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop)
        {
            await this.CreateDirtyEntityAsync(obj, dop, null).LibAwait();
        }
#endif
        private void CreateDirtyEntity(object obj, DirtyOperation dop, ITransaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = this.siaqodb.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            dirtyEntity.OperationTime = DateTime.Now;
            if (transaction != null)
            {
                this.siaqodb.StoreObject(dirtyEntity, transaction);
            }
            else
            {
                this.siaqodb.StoreObject(dirtyEntity);
            }
        }
#if ASYNC
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop, ITransaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = this.siaqodb.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            dirtyEntity.OperationTime = DateTime.Now;
            if (transaction != null)
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity, transaction).LibAwait();
            }
            else
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity).LibAwait();
            }
        }
#endif
        private void CreateTombstoneDirtyEntity(int oid)
        {
            this.CreateTombstoneDirtyEntity(oid, null);
        }
        private void CreateTombstoneDirtyEntity(int oid, Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
            dirtyEntity.OperationTime = DateTime.Now;
            if (transaction != null)
            {
                this.siaqodb.StoreObject(dirtyEntity, transaction);
            }
            else
            {
                this.siaqodb.StoreObject(dirtyEntity);
            }
        }
#if ASYNC
        private async Task CreateTombstoneDirtyEntityAsync( int oid)
        {
            await this.CreateTombstoneDirtyEntityAsync(oid, null).LibAwait();
        }
        private async Task CreateTombstoneDirtyEntityAsync(int oid, Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
            dirtyEntity.OperationTime = DateTime.Now;
            if (transaction != null)
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity, transaction).LibAwait();
            }
            else
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity).LibAwait();
            }
        }
#endif
        public void Store(CryptonorObject obj)
        {
            try
            {
                int oID = this.siaqodb.GetOID(obj);
                if (oID == 0)
                {
                    this.siaqodb.GetOIDForAMSByField(obj, "key");
                }
                oID = this.siaqodb.GetOID(obj);
                DirtyOperation dop = (oID == 0) ? DirtyOperation.Inserted : DirtyOperation.Updated;
                obj.SerializeTags();
                Dictionary<string, object> oldTags = null;
                if (dop == DirtyOperation.Updated)
                {
                    oldTags = indexManager.PrepareUpdateIndexes(oID);
                }
                this.siaqodb.StoreObject(obj);
                indexManager.UpdateIndexes(obj.OID, oldTags, obj.Tags);
                if (obj.IsDirty)
                {
                    this.CreateDirtyEntity(obj, dop);
                }

            }
            finally
            {

            }


        }
#if ASYNC
        public async Task StoreAsync(CryptonorObject obj)
        {
            await _lockerAsync.LockAsync();
            try
            {
                int oID = this.siaqodb.GetOID(obj);
                if (oID == 0)
                {
                    this.siaqodb.GetOIDForAMSByField(obj, "key");
                }
                oID = this.siaqodb.GetOID(obj);
                DirtyOperation dop = (oID == 0) ? DirtyOperation.Inserted : DirtyOperation.Updated;
                obj.SerializeTags();
                Dictionary<string, object> oldTags = null;
                if (dop == DirtyOperation.Updated)
                {
                    oldTags =await indexManager.PrepareUpdateIndexesAsync(oID);
                }
                await this.siaqodb.StoreObjectAsync(obj).LibAwait();
                await indexManager.UpdateIndexesAsync(obj.OID, oldTags, obj.Tags);
                if (obj.IsDirty)
                {
                    await this.CreateDirtyEntityAsync(obj, dop).LibAwait();
                }

            }
            finally
            { 
                _lockerAsync.Release();
            }
          

        }
#endif
        public void StoreBatch(IList<CryptonorObject> objects)
        {
            siaqodb.StartBulkInsert(typeof(CryptonorObject));
            indexManager.AllowPersistence(false);
            DateTime start = DateTime.Now;
            try
            {
                foreach (CryptonorObject obj in objects)
                    this.Store(obj);
            }
            finally
            {
                siaqodb.EndBulkInsert(typeof(CryptonorObject));
            }
            string elaps = (DateTime.Now - start).ToString();

            indexManager.AllowPersistence(true);
            indexManager.Persist();
            this.siaqodb.Flush();

        }
#if ASYNC
       
        public async Task StoreBatchAsync(IList<CryptonorObject> objects)
        {
            await siaqodb.StartBulkInsertAsync(typeof(CryptonorObject)).LibAwait();
            indexManager.AllowPersistence(false);
            DateTime start = DateTime.Now;
            try {
                foreach (CryptonorObject obj in objects)
                    await this.StoreAsync(obj).LibAwait();
            }
            finally
            {
              //TODO
            }
            string elaps = (DateTime.Now - start).ToString();
            await siaqodb.EndBulkInsertAsync(typeof(CryptonorObject)).LibAwait();
            indexManager.AllowPersistence(true);
            await indexManager.PersistAsync();
            await this.siaqodb.FlushAsync().LibAwait();
            
        }
#endif
      
        public IList<CryptonorObject> LoadAll()
        {
            return this.siaqodb.LoadAll<CryptonorObject>();
        }
#if ASYNC
        public async Task<IList<CryptonorObject>> LoadAllAsync()
        {
            return await this.siaqodb.LoadAllAsync<CryptonorObject>().LibAwait();
        }
#endif
        public IList<CryptonorObject> LoadAll(int skip, int limit)
        {
            return this.siaqodb.Query<CryptonorObject>().Skip(skip).Take(limit).ToList();
        }
#if ASYNC
        public async Task<IList<CryptonorObject>> LoadAllAsync(int skip, int limit)
        {
            return await this.siaqodb.Query<CryptonorObject>().Skip(skip).Take(limit).ToListAsync().LibAwait();
        }
#endif 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CryptonorObject Load(string key)
        {
            return this.siaqodb.Query<CryptonorObject>().FirstOrDefault(a => a.Key == key);
        }
#if ASYNC
        public async Task<CryptonorObject> LoadAsync(string key)
        {
            return await this.siaqodb.Query<CryptonorObject>().Where(a => a.Key == key).FirstOrDefaultAsync().LibAwait();
        }
#endif
        public IList<CryptonorObject> Load(Cryptonor.Queries.Query query)
        {
            List<int> oids = new List<int>();
            if (string.Compare(query.TagName, "key", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.LoadByKey(query, oids);
            }
            else //by tags
            {
                this.indexManager.LoadOidsByIndex(query, oids);
            }
            List<CryptonorObject> allFiltered = new List<CryptonorObject>();
            IEnumerable<int> oidsLoaded = oids;
            if (query.Skip != null)
                oidsLoaded = oidsLoaded.Skip(query.Skip.Value);
            if (query.Limit != null)
                oidsLoaded = oidsLoaded.Take(query.Limit.Value);
            foreach (int oid in oidsLoaded)
            {
                var obj = this.siaqodb.LoadObjectByOID<CryptonorObject>(oid);
                allFiltered.Add(obj);
            }
            return allFiltered;

        }
#if ASYNC
        public async Task<IList<CryptonorObject>> LoadAsync(Cryptonor.Queries.Query query)
        {
            List<int> oids = new List<int>();
            if (string.Compare(query.TagName, "key", StringComparison.OrdinalIgnoreCase) == 0)
            {
                await this.LoadByKeyAsync(query, oids);
            }
            else //by tags
            {   
                await this.indexManager.LoadOidsByIndexAsync(query, oids);
            }
            List<CryptonorObject> allFiltered = new List<CryptonorObject>();
            IEnumerable<int> oidsLoaded = oids;
            if (query.Skip != null)
                oidsLoaded = oidsLoaded.Skip(query.Skip.Value);
            if (query.Limit != null)
                oidsLoaded=oidsLoaded.Take(query.Limit.Value);
            foreach (int oid in oidsLoaded)
            {
                var obj = await this.siaqodb.LoadObjectByOIDAsync<CryptonorObject>(oid).LibAwait();
                allFiltered.Add(obj);
            }
            return allFiltered;
           
        }
#endif
        private void LoadByKey(Queries.Query query, List<int> oids)
        {
            IBTree index = siaqodb.GetIndex("key", typeof(CryptonorObject));
            IndexQueryFinder.FindOids(index, query, oids);
        }
#if ASYNC
        private async Task LoadByKeyAsync(Queries.Query query, List<int> oids)
        {
            IBTree index = siaqodb.GetIndex("key", typeof(CryptonorObject));
            await IndexQueryFinder.FindOidsAsync(index, query, oids);
        }
#endif

        public void Delete(CryptonorObject cobj)
        {
            if (cobj.OID == 0)
            {
                throw new Exception("Object not exists in local database");
            }
            var oldTags = indexManager.PrepareUpdateIndexes(cobj.OID);
            int oid = cobj.OID;
            this.siaqodb.Delete(cobj);
            this.CreateTombstoneDirtyEntity(oid);
            indexManager.UpdateIndexesAfterDelete(oid, oldTags);
        }
#if ASYNC
        public async Task DeleteAsync(CryptonorObject cobj)
        {
            if (cobj.OID == 0)
            {
                throw new Exception("Object not exists in local database");
            }
            var oldTags = await indexManager.PrepareUpdateIndexesAsync(cobj.OID);
            int oid = cobj.OID;
            await this.siaqodb.DeleteAsync(cobj).LibAwait();
            await this.CreateTombstoneDirtyEntityAsync(oid).LibAwait();
            await indexManager.UpdateIndexesAfterDeleteAsync(oid, oldTags);
        }
#endif
        public ChangeSet GetChangeSet()
        {
            IList<DirtyEntity> all = this.siaqodb.LoadAll<DirtyEntity>();

            Dictionary<int, ATuple<CryptonorObject, DirtyEntity>> inserts = new Dictionary<int, ATuple<CryptonorObject, DirtyEntity>>();
            Dictionary<int, ATuple<CryptonorObject, DirtyEntity>> updates = new Dictionary<int, ATuple<CryptonorObject, DirtyEntity>>();
            Dictionary<int, ATuple<CryptonorObject, DirtyEntity>> deletes = new Dictionary<int, ATuple<CryptonorObject, DirtyEntity>>();

            foreach (DirtyEntity en in all)
            {

                if (en.DirtyOp == DirtyOperation.Deleted)
                {
                    if (inserts.ContainsKey(en.EntityOID))
                    {
                        siaqodb.Delete(inserts[en.EntityOID].Name);
                        siaqodb.Delete(en);
                        inserts.Remove(en.EntityOID);
                        continue;
                    }
                    else if (updates.ContainsKey(en.EntityOID))
                    {
                        siaqodb.Delete(updates[en.EntityOID].Name);
                        updates.Remove(en.EntityOID);
                    }
                }
                else
                {
                    if (deletes.ContainsKey(en.EntityOID) || inserts.ContainsKey(en.EntityOID) || updates.ContainsKey(en.EntityOID))
                    {
                        siaqodb.Delete(en);
                        continue;
                    }
                }


                CryptonorObject entityFromDB = (CryptonorObject)siaqodb.LoadObjectByOID(typeof(CryptonorObject), en.EntityOID);
                if (en.DirtyOp == DirtyOperation.Inserted)
                {
                    inserts.Add(en.EntityOID, new ATuple<CryptonorObject, DirtyEntity>(entityFromDB, en));
                }
                else if (en.DirtyOp == DirtyOperation.Updated)
                {
                    updates.Add(en.EntityOID, new ATuple<CryptonorObject, DirtyEntity>(entityFromDB, en));
                }
                else if (en.DirtyOp == DirtyOperation.Deleted)
                {
                    deletes.Add(en.EntityOID, new ATuple<CryptonorObject, DirtyEntity>(entityFromDB, en));
                }

            }
            List<CryptonorObject> changed = new List<CryptonorObject>();
            List<DeletedObject> deleted = new List<DeletedObject>();
            foreach (ATuple<CryptonorObject, DirtyEntity> val in inserts.Values)
            {
                changed.Add(val.Name);
            }
            foreach (ATuple<CryptonorObject, DirtyEntity> val in updates.Values)
            {
                changed.Add(val.Name);
            }
            foreach (ATuple<CryptonorObject, DirtyEntity> val in deletes.Values)
            {
                deleted.Add(new DeletedObject { Version = val.Name.Version, Key = val.Name.Key });
            }
            return new ChangeSet { ChangedObjects = changed, DeletedObjects = deleted };
        }
#if ASYNC
        public async Task<ChangeSet> GetChangeSetAsync()
        {
            IList<DirtyEntity> all = await this.siaqodb.LoadAllAsync<DirtyEntity>().LibAwait();
           
            Dictionary<int, Tuple<CryptonorObject, DirtyEntity>> inserts = new Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>();
            Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>  updates= new Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>();
            Dictionary<int, Tuple<CryptonorObject, DirtyEntity>> deletes = new Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>();
           
            foreach (DirtyEntity en in all)
            {

                if (en.DirtyOp == DirtyOperation.Deleted)
                {
                    if (inserts.ContainsKey(en.EntityOID))
                    {
                        await siaqodb.DeleteAsync(inserts[en.EntityOID].Item1).LibAwait();
                        await siaqodb.DeleteAsync(en).LibAwait();
                        inserts.Remove(en.EntityOID);
                        continue;
                    }
                    else if (updates.ContainsKey(en.EntityOID))
                    {
                        await siaqodb.DeleteAsync(updates[en.EntityOID].Item1).LibAwait();
                        updates.Remove(en.EntityOID);
                    }
                }
                else
                {
                    if (deletes.ContainsKey(en.EntityOID) || inserts.ContainsKey(en.EntityOID) || updates.ContainsKey(en.EntityOID))
                    {
                        await siaqodb.DeleteAsync(en).LibAwait();
                        continue;
                    }
                }


                CryptonorObject entityFromDB = (CryptonorObject)await siaqodb.LoadObjectByOIDAsync(typeof(CryptonorObject), en.EntityOID).LibAwait();
                if (en.DirtyOp == DirtyOperation.Inserted)
                {
                    inserts.Add(en.EntityOID, new Tuple<CryptonorObject, DirtyEntity>(entityFromDB, en));
                }
                else if (en.DirtyOp == DirtyOperation.Updated)
                {
                    updates.Add(en.EntityOID, new Tuple<CryptonorObject, DirtyEntity>(entityFromDB, en));
                }
                else if (en.DirtyOp == DirtyOperation.Deleted)
                {
                    deletes.Add(en.EntityOID, new Tuple<CryptonorObject, DirtyEntity>(entityFromDB, en));
                }

            }
            List<CryptonorObject> changed = new List<CryptonorObject>();
            List<DeletedObject> deleted = new List<DeletedObject>();
            foreach( Tuple<CryptonorObject,DirtyEntity> val in inserts.Values)
            {
                changed.Add(val.Item1);
            }
            foreach (Tuple<CryptonorObject, DirtyEntity> val in updates.Values)
            {
                changed.Add(val.Item1);
            }
            foreach (Tuple<CryptonorObject, DirtyEntity> val in deletes.Values)
            {
                deleted.Add(new DeletedObject { Version = val.Item1.Version, Key = val.Item1.Key });
            }
            return new ChangeSet { ChangedObjects = changed,DeletedObjects=deleted };
        }
#endif
        public void ClearSyncMetadata()
        {
            siaqodb.DropType<DirtyEntity>();
        }
#if ASYNC
        public async Task ClearSyncMetadataAsync()
        {
            await siaqodb.DropTypeAsync<DirtyEntity>();
        }
#endif
        public void Purge()
        { 
            string extension = ".sqo";
            string extension2 = ".sqr";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
                extension2 = ".esqr";
            }
            DeleteFileByExt(extension);
            DeleteFileByExt(extension2);
        }
        public string GetAnchor()
        {

            var all = this.siaqodb.LoadAll<Anchor>();
            Anchor anc = all.FirstOrDefault();
            if (anc != null)
                return anc.AnchorValue;
            return null;
        }
#if ASYNC
        public async Task<string> GetAnchorAsync()
        {

            var all = await this.siaqodb.LoadAllAsync<Anchor>().LibAwait();
            Anchor anc = all.FirstOrDefault();
            if (anc != null)
                return anc.AnchorValue;
            return null;
        }
#endif
        public  void StoreAnchor(string anchor)
        {
            Anchor anc = new Anchor() { OID = 1,AnchorValue=anchor };
            siaqodb.StoreObject(anc);
        }
#if ASYNC
        public async Task StoreAnchorAsync(string anchor)
        {
            Anchor anc = new Anchor() { OID = 1, AnchorValue = anchor };
            await siaqodb.StoreObjectAsync(anc).LibAwait();
        }
#endif
        private void DeleteFileByExt(string extension)
        {
#if WinRT
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(this.siaqodb.GetDBPath()).AsTask().Result;
            try
            {
                var files = storageFolder.GetFilesAsync().AsTask().Result;
                foreach (var file in files)
                {
                    file.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask().Wait();
                }
            }
            catch (FileNotFoundException ex)
            {
               //TODO handle it
            }
           
#else
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(siaqodb.GetDBPath());
            FileInfo[] fi = di.GetFiles("*" + extension);

            foreach (FileInfo f in fi)
            {
                File.Delete(f.FullName);
            }
#endif
        }
       
    }
  
     enum DirtyOperation
    {
        Inserted = 1,
        Updated,
        Deleted
    }
     class DirtyEntity
    {
        public int EntityOID;
        public DirtyOperation DirtyOp;
        public DateTime OperationTime;
        public int OID
        {
            get;
            set;
        }
         #if SILVERLIGHT
        public object GetValue(FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
#endif
    }
    public class ChangeSet
    {
        public List<CryptonorObject> ChangedObjects { get; set; }
        public List<DeletedObject> DeletedObjects { get; set; }
        public string Anchor { get; set; }
    }
    public class DeletedObject
    {
        public string Key { get; set; }
        public string Version { get; set; }
    }
    internal class Anchor
    {
        public int OID { get; set; }
        public string AnchorValue { get; set; }
#if  SILVERLIGHT
        public object GetValue(FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
#endif
    }
    
}
