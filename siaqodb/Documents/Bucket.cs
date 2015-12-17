using LightningDB;
using Sqo.Documents.Indexes;
using Sqo.Documents.Queries;
using Sqo.Documents.Utils;
using Sqo.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sqo.Documents.Sync;
namespace Sqo.Documents
{
    public class Bucket : IBucket
    {
        Siaqodb siaqodb;
        TagsIndexManager indexManag;
        string dirtyEntitiesDB;
        string anchorDB;
        string indexInfoDB;
        private readonly object _locker = new object();
        public Bucket(string bucketName, Siaqodb siaqodb)
        {
            this.BucketName = "buk_" + bucketName;
            dirtyEntitiesDB = this.BucketName + "_sys_dirtydb";
            this.anchorDB = this.BucketName + "_sys_anchordb";
            this.siaqodb = siaqodb;
            indexInfoDB = this.BucketName + "_sys_indexinfo";
            indexManag = new TagsIndexManager(indexInfoDB, this.siaqodb);
        }
        public string BucketName
        {
            get;
            internal set;
        }
        internal void Delete(string key, bool isDirty)
        {
            Document doc = this.Load(key);
            if (doc != null)
            {
                Delete(doc, isDirty);
            }
        }
        public void Delete(string key)
        {
            this.Delete(key, true);
        }
        public void Delete(Document doc)
        {
            this.Delete(doc, true);
        }
        public void Delete(Document doc, ITransaction transaction)
        {
            lock (_locker)
            {
                this.Delete(doc, transaction, true);
            }
        }
        internal void Delete(Document doc, ITransaction transaction, bool isDirty)
        {
            var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
            var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create);

            byte[] keyBytes = ByteConverter.StringToByteArray(doc.Key);
            var oldTags = indexManag.PrepareUpdateIndexes(keyBytes, lmdbTransaction, db);

            lmdbTransaction.Delete(db, keyBytes);
            if (SiaqodbConfigurator.IsBucketSyncable(this.BucketName) && isDirty)
            {
                CreateDirtyEntity(DirtyOperation.Deleted, lmdbTransaction, doc.Key, doc.Version);
            }
            indexManag.UpdateIndexesAfterDelete(doc.Key, oldTags, lmdbTransaction, BucketName);



        }
        internal void Delete(Document doc, bool isDirty)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    try
                    {
                        Delete(doc, transaction, isDirty);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LightningException lEx = ex as LightningException;
                        if (lEx != null && lEx.Message.StartsWith("MDB_NOTFOUND"))
                        {
                            return;
                        }
                        throw ex;
                    }
                }
            }
        }



        public Document FindFirst(Query query)
        {
            lock (_locker)
            {
                bool started;
                var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);
                try
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    IEnumerable<string> keysLoaded = this.GetKeys(lmdbTransaction, query);
                    string key = keysLoaded.FirstOrDefault();
                    if (key != null)
                        return Get(key, lmdbTransaction);

                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
                return null;
            }
        }
        public IList<Document> Find(Query query)
        {
            lock (_locker)
            {
                bool started;
                var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);
                try
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    List<Document> allFiltered = new List<Document>();
                    IEnumerable<string> keysLoaded = this.GetKeys(lmdbTransaction, query);

                    foreach (string key in keysLoaded)
                    {
                        var obj = Get(key, lmdbTransaction);
                        allFiltered.Add(obj);
                    }
                    return allFiltered;
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }

        }
        public int Count(Query query)
        {
            lock (_locker)
            {
                bool started;
                var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);
                try
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    IEnumerable<string> keysLoaded = this.GetKeys(lmdbTransaction, query);
                    return keysLoaded.Count();

                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }

            }
        }
        public int Count()
        {
            lock (_locker)
            {
                bool started;
                var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);
                try
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    using (var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create))
                    {
                        var c = lmdbTransaction.CreateCursor(db);
                        var i = 0;
                        var current = c.MoveToFirst();
                        while (current.HasValue)
                        {
                            i++;
                            current = c.MoveNext();
                        }
                        return i;
                    }
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }

        }

        private IEnumerable<string> GetKeys(LightningTransaction lmdbTransaction, Query query)
        {
            var qr = new QueryRunner(indexManag, lmdbTransaction, this.BucketName);
            var keys = qr.Run(query);
            IEnumerable<string> keysLoaded = keys;
            if (query.skip != null)
                keysLoaded = keysLoaded.Skip(query.skip.Value);
            if (query.limit != null)
                keysLoaded = keysLoaded.Take(query.limit.Value);
            return keysLoaded;
        }

        public T Load<T>(string key)
        {
            lock (_locker)
            {
                Document obj = this.Load(key);
                return obj.GetContent<T>();
            }
        }
        public Document Load(string key)
        {

            lock (_locker)
            {
                bool started;
                var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);
                try
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    return Get(key, lmdbTransaction);
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }

        }
        private Document Get(string key, LightningTransaction transaction)
        {

            var db = transaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create);

            byte[] keyBytes = ByteConverter.StringToByteArray(key);
            byte[] crObjBytes = transaction.Get(db, keyBytes);
            if (crObjBytes != null)
            {
                IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
                return serializer.Deserialize(typeof(Document), crObjBytes) as Document;

            }
            return null;

        }



        public IList<Document> LoadAll()
        {
            lock (_locker)
            {
                bool started;
                var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);
                try
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    List<Document> list = new List<Document>();

                    var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create);

                    IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;

                    using (var cursor = lmdbTransaction.CreateCursor(db))
                    {
                        var current = cursor.MoveNext();

                        while (current.HasValue)
                        {
                            byte[] crObjBytes = current.Value.Value;
                            if (crObjBytes != null)
                            {
                                var obj = serializer.Deserialize(typeof(Document), crObjBytes) as Document;
                                list.Add(obj);
                            }
                            current = cursor.MoveNext();
                        }
                    }


                    return list;

                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }


            }

        }

        public IList<Document> LoadAll(int skip, int limit)
        {
            lock (_locker)
            {
                bool started;
                var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);
                try
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    List<Document> list = new List<Document>();
                    using (var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create))
                    {
                        IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;

                        using (var cursor = lmdbTransaction.CreateCursor(db))
                        {
                            var current = cursor.MoveNext();
                            int i = 0;
                            while (current.HasValue)
                            {
                                if (i < skip)
                                {
                                    current = cursor.MoveNext();
                                    i++;
                                    continue;
                                }
                                byte[] crObjBytes = current.Value.Value;
                                if (crObjBytes != null)
                                {
                                    var obj = serializer.Deserialize(typeof(Document), crObjBytes) as Document;
                                    list.Add(obj);
                                }
                                if (list.Count == limit)
                                    break;
                                current = cursor.MoveNext();
                                i++;
                            }
                        }

                    }
                    return list;
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }

            }
        }

        public void Store(Document doc)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {

                    this.Store(doc, transaction);
                    transaction.Commit();

                }
            }

        }
        internal void Store(Document doc,bool isDirty)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {

                    this.Store(doc, transaction,isDirty);
                    transaction.Commit();

                }
            }

        }
        internal void Store(Document doc, ITransaction transaction, bool isDirty)
        {
            var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();


            DirtyOperation dop;
            var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create);

            byte[] keyBytes = ByteConverter.StringToByteArray(doc.Key);
            var oldTags = indexManag.PrepareUpdateIndexes(keyBytes, lmdbTransaction, db);
            dop = oldTags != null ? DirtyOperation.Updated : DirtyOperation.Inserted;

            IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
            byte[] crObjBytes = serializer.Serialize(doc);
            lmdbTransaction.Put(db, keyBytes, crObjBytes);
            indexManag.UpdateIndexes(doc.Key, oldTags, doc.Tags, lmdbTransaction, BucketName);
            if (SiaqodbConfigurator.IsBucketSyncable(this.BucketName) && isDirty)
            {
                CreateDirtyEntity(dop, lmdbTransaction, doc);
            }
        }
        public void Store(Document doc, ITransaction transaction)
        {
            lock (_locker)
            {
                this.Store(doc, transaction, true);
            }

        }
        public void Store(object obj, object tags = null)
        {
            this.Store(null, obj, tags);
        }

        public void Store(string key, object obj)
        {
            this.Store(key, obj, null);
        }

        public void Store(string key, object obj, object tags = null)
        {
            Dictionary<string, object> tags_Dict = null;
            if (tags != null)
            {
                tags_Dict = new Dictionary<string, object>();
                object o = tags;
                Type tagsType = o.GetType();

                PropertyInfo[] pi = tagsType.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    tags_Dict.Add(p.Name, p.GetValue(o, null));
                }
            }

            Store(key, obj, tags_Dict);
        }

        public void Store(string key, object obj, Dictionary<string, object> tags)
        {
            Document doc = new Document();
            doc.Key = key;
            doc.SetContent(obj);

            if (tags != null)
            {
                foreach (string tagName in tags.Keys)
                {
                    doc.SetTag(tagName, tags[tagName]);
                }
            }

            Store(doc);
        }
        public void StoreBatch(IList<Document> docs)
        {
            this.StoreBatch(docs, true);
        }
        internal void StoreBatch(IList<Document> docs, bool isDirty)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();


                    DirtyOperation dop;
                    using (var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create))
                    {
                        foreach (Document doc in docs)
                        {
                            byte[] keyBytes = ByteConverter.StringToByteArray(doc.Key);
                            var oldTags = indexManag.PrepareUpdateIndexes(keyBytes, lmdbTransaction, db);
                            dop = oldTags != null ? DirtyOperation.Updated : DirtyOperation.Inserted;

                            IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
                            byte[] crObjBytes = serializer.Serialize(doc);
                            lmdbTransaction.Put(db, keyBytes, crObjBytes);
                            indexManag.UpdateIndexes(doc.Key, oldTags, doc.Tags, lmdbTransaction, BucketName);
                            if (SiaqodbConfigurator.IsBucketSyncable(this.BucketName) && isDirty)
                            {
                                CreateDirtyEntity(dop, lmdbTransaction, doc);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }

        }


        private void CreateDirtyEntity(DirtyOperation dop, LightningTransaction transaction, Document obj)
        {
            CreateDirtyEntity(dop, transaction, obj.Key, obj.Version);
        }
        private void CreateDirtyEntity(DirtyOperation dirtyOperation, LightningTransaction transaction, string key, string version)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.DirtyOp = dirtyOperation;
            dirtyEntity.Key = key;
            dirtyEntity.OperationTime = DateTime.Now;
            dirtyEntity.Version = version;

            var db = transaction.OpenDatabase(dirtyEntitiesDB, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);

            IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
            byte[] dirtyEntityBytes = serializer.Serialize(dirtyEntity);
            byte[] keyBytes = ByteConverter.StringToByteArray(key);
            transaction.Put(db, keyBytes, dirtyEntityBytes);


        }
        public override int GetHashCode()
        {
            return this.BucketName.GetHashCode();
        }
        /// <summary>
        /// Cast method to be used in LINQ queries
        /// </summary>
        /// <typeparam name="T">Type over which LINQ will take action</typeparam>
        /// <returns></returns>
        public IDocQuery<T> Cast<T>() where T : Document
        {
            return new DocQuery<T>(this, new Query());
        }
        /// <summary>
        /// Query method to be used in LINQ queries
        /// </summary>
        /// <typeparam name="T">Type over which LINQ will take action</typeparam>
        /// <returns></returns>
        public IDocQuery<T> Query<T>() where T : Document
        {
            return this.Cast<T>();
        }
        internal ChangeSet GetChangeSet()
        {
            lock (_locker)
            {
                IList<DirtyEntity> all = this.GetAllDirtyEntities(dirtyEntitiesDB).OrderBy(a => a.OperationTime).ToList();

                Dictionary<string, ATuple<Document, DirtyEntity>> inserts = new Dictionary<string, ATuple<Document, DirtyEntity>>();
                Dictionary<string, ATuple<Document, DirtyEntity>> updates = new Dictionary<string, ATuple<Document, DirtyEntity>>();
                Dictionary<string, ATuple<DeletedDocument, DirtyEntity>> deletes = new Dictionary<string, ATuple<DeletedDocument, DirtyEntity>>();

                foreach (DirtyEntity en in all)
                {
                    if (en.Key == null)
                        continue;
                    if (en.DirtyOp == DirtyOperation.Deleted)
                    {
                        if (inserts.ContainsKey(en.Key))
                        {
                            DeleteTombstoneEntity(en);
                            inserts.Remove(en.Key);
                            continue;
                        }
                        else if (updates.ContainsKey(en.Key))
                        {
                            updates.Remove(en.Key);
                        }
                    }
                    else
                    {
                        if (deletes.ContainsKey(en.Key) || inserts.ContainsKey(en.Key) || updates.ContainsKey(en.Key))
                        {
                            DeleteTombstoneEntity(en);
                            continue;
                        }
                    }


                    Document entityFromDB = Load(en.Key);
                    if (en.DirtyOp == DirtyOperation.Inserted)
                    {
                        inserts.Add(en.Key, new ATuple<Document, DirtyEntity>(entityFromDB, en));
                    }
                    else if (en.DirtyOp == DirtyOperation.Updated)
                    {
                        updates.Add(en.Key, new ATuple<Document, DirtyEntity>(entityFromDB, en));
                    }
                    else if (en.DirtyOp == DirtyOperation.Deleted)
                    {
                        var deletedFromDb = new DeletedDocument { Version = en.Version, Key = en.Key };
                        deletes.Add(en.Key, new ATuple<DeletedDocument, DirtyEntity>(deletedFromDb, en));
                    }

                }
                List<Document> changed = new List<Document>();
                List<DeletedDocument> deleted = new List<DeletedDocument>();
                foreach (ATuple<Document, DirtyEntity> val in inserts.Values)
                {
                    changed.Add(val.Name);
                }
                foreach (ATuple<Document, DirtyEntity> val in updates.Values)
                {
                    changed.Add(val.Name);
                }
                foreach (ATuple<DeletedDocument, DirtyEntity> val in deletes.Values)
                {
                    deleted.Add(new DeletedDocument { Version = val.Name.Version, Key = val.Name.Key });
                }
                return new ChangeSet { ChangedDocuments = changed, DeletedDocuments = deleted };
            }
        }
        private IList<DirtyEntity> GetAllDirtyEntities(string dbName)
        {

            using (var transaction = siaqodb.BeginTransaction())
            {
                var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();

                var db = lmdbTransaction.OpenDatabase(dbName, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);

                IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
                List<DirtyEntity> list = new List<DirtyEntity>();
                using (var cursor = lmdbTransaction.CreateCursor(db))
                {
                    var current = cursor.MoveNext();
                    while (current.HasValue)
                    {
                        byte[] crObjBytes = current.Value.Value;
                        if (crObjBytes != null)
                        {
                            var obj = (DirtyEntity)serializer.Deserialize(typeof(DirtyEntity), crObjBytes);
                            list.Add(obj);
                        }
                        GetDuplicates(cursor, list, serializer);
                        current = cursor.MoveNext();
                    }
                }
                transaction.Commit();
                return list;

            }

        }
        private void DeleteTombstoneEntity(DirtyEntity entity)
        {
            using (var transaction = siaqodb.BeginTransaction())
            {
                var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                try
                {
                    using (var db = lmdbTransaction.OpenDatabase(dirtyEntitiesDB, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort))
                    {
                        byte[] keyBytes = ByteConverter.StringToByteArray(entity.Key);
                        IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
                        byte[] dirtyEntityBytes = serializer.Serialize(entity);

                        lmdbTransaction.Delete(db, keyBytes, dirtyEntityBytes);
                        transaction.Commit();

                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
        private static void GetDuplicates(LightningCursor cursor, List<DirtyEntity> list, IDocumentSerializer serializer)
        {
            var currentDuplicate = cursor.MoveNextDuplicate();
            while (currentDuplicate.HasValue)
            {
                var obj = (DirtyEntity)serializer.Deserialize(typeof(DirtyEntity), currentDuplicate.Value.Value);
                list.Add(obj);
                currentDuplicate = cursor.MoveNextDuplicate();
            }
        }
        internal void ClearSyncMetadata()
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    try
                    {
                        using (var db = lmdbTransaction.OpenDatabase(dirtyEntitiesDB, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort))
                        {
                            lmdbTransaction.DropDatabase(db, true);
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
        internal string GetAnchor()
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    try
                    {
                        var db = lmdbTransaction.OpenDatabase(anchorDB, DatabaseOpenFlags.Create);

                        byte[] keyBytes = ByteConverter.StringToByteArray("anchor");
                        byte[] ancBytes = lmdbTransaction.Get(db, keyBytes);
                        if (ancBytes != null)
                            return ByteConverter.ByteArrayToString(ancBytes);
                        return null;


                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        transaction.Commit();
                    }
                }
            }
        }
        internal void StoreAnchor(string anchor)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    try
                    {
                        using (var db = lmdbTransaction.OpenDatabase(anchorDB, DatabaseOpenFlags.Create))
                        {
                            byte[] keyBytes = ByteConverter.StringToByteArray("anchor");
                            var anchorBytes = ByteConverter.StringToByteArray(anchor);
                            lmdbTransaction.Put(db, keyBytes, anchorBytes);
                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
        internal void UpdateVersions(IEnumerable<KeyValuePair<string, string>> successfullUpdates)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();

                    using (var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create))
                    {
                        foreach (var keyVers in successfullUpdates)
                        {
                            byte[] keyBytes = ByteConverter.StringToByteArray(keyVers.Key);
                            byte[] crObjBytes = lmdbTransaction.Get(db, keyBytes);
                            if (crObjBytes != null)
                            {
                                IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
                                var doc = serializer.Deserialize(typeof(Document), crObjBytes) as Document;
                                doc.Version = keyVers.Value;
                                var crObjBytesNew = serializer.Serialize(doc);
                                lmdbTransaction.Put(db, keyBytes, crObjBytesNew);

                            }

                        }
                        transaction.Commit();
                    }
                }
            }

        }
        private void DropIndexes(LightningTransaction transaction)
        {
            var indexes = indexManag.GetIndexes();
            foreach (string index in indexes)
            {
                var db = transaction.OpenDatabase(index, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);
                transaction.DropDatabase(db, true);
            }
        }
        internal void Drop()
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    try
                    {
                        var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();

                        DropIndexes(lmdbTransaction);

                        var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create);
                        lmdbTransaction.DropDatabase(db, true);

                        var dbAnc = lmdbTransaction.OpenDatabase(this.anchorDB, DatabaseOpenFlags.Create);
                        lmdbTransaction.DropDatabase(dbAnc, true);

                        var dbDE = lmdbTransaction.OpenDatabase(this.dirtyEntitiesDB, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);
                        lmdbTransaction.DropDatabase(dbDE, true);

                        var dbIndexInfo = lmdbTransaction.OpenDatabase(this.indexInfoDB, DatabaseOpenFlags.Create);
                        lmdbTransaction.DropDatabase(dbIndexInfo, true);

                        lmdbTransaction.Commit();


                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
    }

    [System.Reflection.Obfuscation(Exclude = true)]
    enum DirtyOperation
    {
        Inserted = 1,
        Updated,
        Deleted
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    class DirtyEntity
    {
        public DirtyOperation DirtyOp;
        public DateTime OperationTime;
        public string Key{ get; set; }
        public string Version { get; set; }

    }
    

  
    class ATuple<T, V>
    {
        public T Name { get; set; }
        public V Value { get; set; }
        public ATuple(T name, V value)
        {
            Name = name;
            Value = value;
        }

    }

}
