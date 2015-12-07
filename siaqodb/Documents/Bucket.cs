using LightningDB;
using Sqo.Documents.Indexes;
using Sqo.Documents.Queries;
using Sqo.Documents.Utils;
using Sqo.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents
{
    public class Bucket : IBucket
    {
        Siaqodb siaqodb;
        TagsIndexManager indexManag;
        const string dirtyEntitiesDB = "sys_dirtydb";
        private readonly object _locker = new object();
        public Bucket(string bucketName,Siaqodb siaqodb)
        {
            this.BucketName = "buk_"+bucketName;
            this.siaqodb = siaqodb;
            indexManag = new TagsIndexManager();
        }
        public string BucketName
        {
            get;
            set;
        }

        public void Delete(Document doc)
        {
            lock(_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    try
                    {
                        var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                        using (var db = lmdbTransaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create))
                        {
                            byte[] keyBytes = ByteConverter.StringToByteArray(doc.Key);
                            var oldTags = indexManag.PrepareUpdateIndexes(keyBytes, lmdbTransaction, db);

                            lmdbTransaction.Delete(db, keyBytes);
                            if (SiaqodbConfigurator.IsBucketSyncable(this.BucketName))
                            {
                                //  LOG CHANGES IF THE DELETE WAS MADE ON THE USER BUCKET
                                CreateDirtyEntity(DirtyOperation.Deleted, lmdbTransaction, keyBytes, doc.Version);
                            }
                            indexManag.UpdateIndexesAfterDelete(doc.Key, oldTags, lmdbTransaction, BucketName);
                            transaction.Commit();

                        }
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

        public void Delete(string key)
        {
            Document doc = this.Load(key);
            Delete(doc);
        }
       

        public IList<Document> Find(Query query)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
                    try
                    {
                        var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                        var qr = new QueryRunner(indexManag, lmdbTransaction, this.BucketName);
                        var keys = qr.Run(query);
                        List<Document> allFiltered = new List<Document>();
                        IEnumerable<string> keysLoaded = keys;
                        if (query.skip != null)
                            keysLoaded = keysLoaded.Skip(query.skip.Value);
                        if (query.limit != null)
                            keysLoaded = keysLoaded.Take(query.limit.Value);
                        foreach (string key in keysLoaded)
                        {
                            var obj = Get(key, lmdbTransaction);
                            allFiltered.Add(obj);
                        }
                        return allFiltered;
                    }
                    finally
                    {
                        transaction.Commit();
                    }
                }
            }
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
                using (var transaction = siaqodb.BeginTransaction())
                {
                    try
                    {
                        var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                        return Get(key, lmdbTransaction);
                    }
                    finally
                    {
                        transaction.Commit();
                    }
                }
            }
        }
        private Document Get(string key, LightningTransaction transaction)
        {

            using (var db = transaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create))
            {
                byte[] keyBytes = ByteConverter.StringToByteArray(key);
                byte[] crObjBytes = transaction.Get(db, keyBytes);
                if (crObjBytes != null)
                {
                    IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
                    return serializer.Deserialize(typeof(Document), crObjBytes) as Document;

                }
            }
            return null;


        }

    

        public IList<Document> LoadAll()
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
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

                        }
                        return list;

                    }
                    finally
                    {
                        transaction.Commit();
                    }


                }
            }
        }

        public IList<Document> LoadAll(int skip, int limit)
        {
            lock (_locker)
            {
                using (var transaction = siaqodb.BeginTransaction())
                {
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
                        transaction.Commit();
                    }


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
        public void Store(Document doc, ITransaction transaction)
        {
            lock (_locker)
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
                if (SiaqodbConfigurator.IsBucketSyncable(this.BucketName))
                {
                    CreateDirtyEntity(dop, lmdbTransaction, doc);
                }


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
        internal void StoreBatch(IList<Document> docs,bool isDirty)
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
            byte[] keyBytes = ByteConverter.StringToByteArray(obj.Key);
            CreateDirtyEntity(dop, transaction, keyBytes, obj.Version);
        }
        private void CreateDirtyEntity(DirtyOperation dirtyOperation, LightningTransaction transaction, byte[] keyBytes, string version)
        {
            DirtyEntityLmdb dirtyEntity = new DirtyEntityLmdb();
            dirtyEntity.DirtyOp = dirtyOperation;
            dirtyEntity.KeyBytes = keyBytes;
            dirtyEntity.OperationTime = DateTime.Now;
            dirtyEntity.Version = version;

            var db = transaction.OpenDatabase(dirtyEntitiesDB, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);

            IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
            byte[] dirtyEntityBytes = serializer.Serialize(dirtyEntity);

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
        public IDocQuery<T> Cast<T>() where T :Document
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
        public int EntityOID;
        public DirtyOperation DirtyOp;
        public DateTime OperationTime;
        public int OID
        {
            get;
            set;
        }

    }
    [System.Reflection.Obfuscation(Exclude = true)]
    class DirtyEntityLmdb : DirtyEntity
    {
        public byte[] KeyBytes { get; set; }
        public string Version { get; set; }
    }

    [System.Reflection.Obfuscation(Exclude = true)]
    internal class Anchor
    {
        public int OID { get; set; }
        public string AnchorValue { get; set; }

    }
}
