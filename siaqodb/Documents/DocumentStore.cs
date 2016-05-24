using LightningDB;
using Sqo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sqo.Documents
{
    public class DocumentStore
    {
        Siaqodb siaqodb;
        Dictionary<string, Bucket> cache = new Dictionary<string, Bucket>();
        private readonly object _locker = new object();
        const string sys_buckets = "sys_allbuckets";
        public DocumentStore(Siaqodb siaqodb)
        {
            this.siaqodb = siaqodb;
        }
        public IBucket this[string bucketName]
        {
            get
            {
                return this.GetBucket(bucketName);
            }
        }
        public IBucket GetBucket(string bucketName)
        {
            lock(_locker)
            {
              
                if (!cache.ContainsKey(bucketName))
                {
                    cache[bucketName] = new Bucket(bucketName, siaqodb);
                    this.StoreMetaBucket(bucketName);
                }
                return cache[bucketName];
            }
        }
        public void DropBucket(string bucketName)
        {
            lock (_locker)
            {
                Bucket buk = this.GetBucket(bucketName) as Bucket;
                buk.Drop();
                if (cache.ContainsKey(bucketName))
                {
                    cache.Remove(bucketName);
                }
            }
        }
        private void StoreMetaBucket(string bucketName)
        {
            using (var transaction = siaqodb.BeginTransaction())
            {
                var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                var db = lmdbTransaction.OpenDatabase(sys_buckets, DatabaseOpenFlags.Create);
                byte[] keyBytes = ByteConverter.GetBytes(bucketName, typeof(string));
                lmdbTransaction.Put(db, keyBytes, keyBytes);
                transaction.Commit();
            }
        }
        public List<string> GetAllBuckets()
        {
            lock (_locker)
            {
                var buckets = new List<string>();
                using (var transaction = siaqodb.BeginTransaction())
                {
                    var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                    var db = lmdbTransaction.OpenDatabase(sys_buckets, DatabaseOpenFlags.Create);

                    using (var cursor = lmdbTransaction.CreateCursor(db))
                    {
                        var current = cursor.MoveNext();

                        while (current.HasValue)
                        {

                            byte[] bucketNameBytes = current.Value.Key;
                            string bucketName = ByteConverter.ByteArrayToString(bucketNameBytes);
                            buckets.Add(bucketName);

                            current = cursor.MoveNext();
                        }
                    }
                    transaction.Commit();
                    return buckets;
                }
            }
        }
        internal void ClearCache()
        {
            cache.Clear();
        }

    }
}
