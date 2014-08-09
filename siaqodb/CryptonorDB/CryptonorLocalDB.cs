using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.Indexes;
using Sqo.Transactions;
using System.Collections;
using System.Linq.Expressions;
using System.IO;
using Sqo;
using Cryptonor.Indexes;

namespace Cryptonor
{
    public class CryptonorLocalDB 
    {
        private Siaqodb siaqodb;
        TagsIndexManager indexManager;
        private readonly object _locker = new object();
        private readonly AsyncLock _lockerAsync = new AsyncLock();
#if !WinRT
        public CryptonorLocalDB(string bucketPath)
        {
            //SiaqodbConfigurator.EncryptedDatabase = true;
            this.siaqodb = new Siaqodb(bucketPath);
            indexManager = new TagsIndexManager(this.siaqodb);
           
           
        }

       
#endif
       
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop)
        {
            await this.CreateDirtyEntityAsync(obj, dop, null);
        }
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop, ITransaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = this.siaqodb.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            dirtyEntity.OperationTime = DateTime.Now;
            if (transaction != null)
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity, transaction);
            }
            else
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity);
            }
        }
       
        private async Task CreateTombstoneDirtyEntityAsync( int oid)
        {
            await this.CreateTombstoneDirtyEntityAsync( oid, null);
        }
        private async Task CreateTombstoneDirtyEntityAsync(int oid, Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
            dirtyEntity.OperationTime = DateTime.Now;
            if (transaction != null)
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity, transaction);
            }
            else
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity);
            }
        }

        public async Task Store(CryptonorObject obj)
        {
            int oID = this.siaqodb.GetOID(obj);
            if (oID == 0)
            {
                this.siaqodb.GetOIDForAMSByField(obj, "key");
            }
            oID = this.siaqodb.GetOID(obj);
            DirtyOperation dop = (oID == 0) ? DirtyOperation.Inserted : DirtyOperation.Updated;
            Dictionary<string, object> oldTags = null;
            if (dop == DirtyOperation.Updated)
            {
                oldTags = indexManager.PrepareUpdateIndexes(oID);
            }
            await this.siaqodb.StoreObjectAsync(obj);
            indexManager.UpdateIndexes(obj.OID, oldTags, obj.GetAllTags());
            if (obj.IsDirty)
            {
                await this.CreateDirtyEntityAsync(obj, dop);
            }

            this.siaqodb.Flush();

        }
      
        public async Task<IList<CryptonorObject>> LoadAll()
        {
            return await this.siaqodb.LoadAllAsync<CryptonorObject>();
        }
        public async Task<IList<CryptonorObject>> LoadAll(int skip, int limit)
        {
            return await this.siaqodb.Query<CryptonorObject>().Skip(skip).Take(limit).ToListAsync();
        }
       
        public async Task<CryptonorObject> Load(string key)
        {
            throw new NotImplementedException();
        }
        public async Task<IList<CryptonorObject>> Load(Cryptonor.Queries.CryptonorQuery query)
        {
            List<int> oids = new List<int>();
            if (string.Compare(query.TagName, "key", true) == 0)
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
                oidsLoaded=oidsLoaded.Take(query.Limit.Value);
            foreach (int oid in oidsLoaded)
            {
                var obj = await this.siaqodb.LoadObjectByOIDAsync<CryptonorObject>(oid);
                allFiltered.Add(obj);
            }
            return allFiltered;
           
        }

        private void LoadByKey(Queries.CryptonorQuery query, List<int> oids)
        {
            IBTree index = siaqodb.GetIndex("key", typeof(CryptonorObject));
            IndexQueryFinder.FindOids(index, query, oids);
        }
       
       
        private DotissiConfigurator configurator=new DotissiConfigurator();
        public DotissiConfigurator Configurator { get { return configurator; } }
        public async Task Delete(CryptonorObject cobj)
        {
            if (cobj.OID == 0)
            {
                throw new Exception("Object not exists in local database");
            }
            var oldTags = indexManager.PrepareUpdateIndexes(cobj.OID);
            await this.siaqodb.DeleteAsync(cobj);
            await this.CreateTombstoneDirtyEntityAsync(cobj.OID);
            indexManager.UpdateIndexesAfterDelete(cobj.OID, oldTags);
        }
        public async Task<CryptonorChangeSet> GetChangeSet()
        {
            IList<DirtyEntity> all=await this.siaqodb.LoadAllAsync<DirtyEntity>();
           
            Dictionary<int, Tuple<CryptonorObject, DirtyEntity>> inserts = new Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>();
            Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>  updates= new Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>();
            Dictionary<int, Tuple<CryptonorObject, DirtyEntity>> deletes = new Dictionary<int, Tuple<CryptonorObject, DirtyEntity>>();
           
            foreach (DirtyEntity en in all)
            {

                if (en.DirtyOp == DirtyOperation.Deleted)
                {
                    if (inserts.ContainsKey(en.EntityOID))
                    {
                        await siaqodb.DeleteAsync(inserts[en.EntityOID].Item1);
                        await siaqodb.DeleteAsync(en);
                        inserts.Remove(en.EntityOID);
                        continue;
                    }
                    else if (updates.ContainsKey(en.EntityOID))
                    {
                        await siaqodb.DeleteAsync(updates[en.EntityOID].Item1);
                        updates.Remove(en.EntityOID);
                    }
                }
                else
                {
                    if (deletes.ContainsKey(en.EntityOID) || inserts.ContainsKey(en.EntityOID) || updates.ContainsKey(en.EntityOID))
                    {
                        await siaqodb.DeleteAsync(en);
                        continue;
                    }
                }


                CryptonorObject entityFromDB = (CryptonorObject)await siaqodb.LoadObjectByOIDAsync(typeof(CryptonorObject), en.EntityOID);
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
            IList<CryptonorObject> changed = new List<CryptonorObject>();
            IList<DeletedObject> deleted = new List<DeletedObject>();
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
                deleted.Add(new DeletedObject { DeletedTime = val.Item2.OperationTime, Key = val.Item1.Key });
            }
            return new CryptonorChangeSet { ChangedObjects = changed,DeletedObjects=deleted };
        }

        public async Task ClearSyncMetadata()
        {
            await siaqodb.DropTypeAsync<DirtyEntity>();
        }
       
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
        private void DeleteFileByExt(string extension)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(siaqodb.GetDBPath());
            FileInfo[] fi = di.GetFiles("*" + extension);

            foreach (FileInfo f in fi)
            {
                File.Delete(f.FullName);
            }
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
        public object GetValue(FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
    }
    public class CryptonorChangeSet
    {
        public IList<CryptonorObject> ChangedObjects { get; internal set; }
        public IList<DeletedObject> DeletedObjects { get; internal set; }
       
    }
    public class DeletedObject
    {
        public string Key { get; set; }
        public DateTime DeletedTime { get; set; }
    }
    public class DotissiConfigurator
    {
        internal Dictionary<Type, string> buckets = new Dictionary<Type, string>();
        public void RegisterBucket(Type type, string bucketName)
        {
            buckets[type] = bucketName;
        }
    }
    
}
