using LightningDB;
using Sqo.Documents.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents.Indexes
{
    class TagsIndexManager
    {
        Dictionary<string, string> indexMetaInfo = new Dictionary<string, string>();
        string indexInfoDBName;
        public TagsIndexManager(string indexInfoDBName, Siaqodb siaqodb)
        {
            this.indexInfoDBName = indexInfoDBName;
            bool started;
            var transaction = siaqodb.transactionManager.GetActiveTransaction(out started);

            try
            {

                var lmdbTransaction = siaqodb.transactionManager.GetActiveTransaction();
                var db = lmdbTransaction.OpenDatabase(indexInfoDBName, DatabaseOpenFlags.Create);
                using (var cursor = lmdbTransaction.CreateCursor(db))
                {
                    var firstKV = cursor.MoveToFirst();
                    while (firstKV.HasValue)
                    {
                        string currentKey = (string)ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                        indexMetaInfo[currentKey] = currentKey;
                        firstKV = cursor.MoveNext();
                    }
                    if (started)
                    {
                        transaction.Commit();
                    }
                }
            }
            catch
            {
                if (started)
                {
                    transaction.Rollback();
                }
                throw;
            }

        }
        public Dictionary<string, object> PrepareUpdateIndexes(byte[] keyBytes, LightningTransaction transaction, LightningDatabase db)
        {
            byte[] crObjBytes = transaction.Get(db, keyBytes);
            if (crObjBytes != null)
            {
                IDocumentSerializer serializer = SiaqodbConfigurator.DocumentSerializer;
                Document obj = serializer.Deserialize(typeof(Document), crObjBytes) as Document;
                return obj.Tags ?? new Dictionary<string, object>();
            }
            return null;
        }
        public void UpdateIndexes(string crKey, Dictionary<string, object> oldTags, Dictionary<string, object> newTags, LightningTransaction transaction, string bucketName)
        {
            Index index = null;
            if (oldTags != null && oldTags.Count > 0)
            {
                foreach (string key in oldTags.Keys)
                {

                    if (newTags != null && newTags.ContainsKey(key))
                    {
                        object oldVal = oldTags[key];
                        if (newTags[key].GetType() != oldVal.GetType())
                        {
                            oldVal = Util.ChangeType(oldTags[key], newTags[key].GetType());
                        }
                        int c = ((IComparable)newTags[key]).CompareTo(oldVal);
                        if (c != 0)
                        {
                            index = this.GetIndex(bucketName, key, transaction);
                            index.DeleteItem(oldTags[key], crKey);
                            index.AddItem(newTags[key], crKey);
                        }
                    }
                    else//tag is removed
                    {
                        index = this.GetIndex(bucketName, key, transaction);
                        index.DeleteItem(oldTags[key], crKey);
                    }
                }
                if (newTags != null)
                {
                    foreach (string key in newTags.Keys)
                    {
                        if (!oldTags.ContainsKey(key))
                        {
                            index = this.GetIndex(bucketName, key, transaction);
                            index.AddItem(newTags[key], crKey);
                        }
                    }
                }

            }
            else//add
            {
                if (newTags != null)
                {
                    foreach (string key in newTags.Keys)
                    {
                        index = this.GetIndex(bucketName, key, transaction);
                        index.AddItem(newTags[key], crKey);
                    }
                }
            }
            if (index != null)
            {
                //index.Dispose();
            }
        }
        public void UpdateIndexesAfterDelete(string crKey, Dictionary<string, object> oldTags, LightningTransaction transaction, string bucketName)
        {
            if (oldTags != null && oldTags.Count > 0)
            {
                foreach (string key in oldTags.Keys)
                {
                    Index index = this.GetIndex(bucketName, key, transaction);
                    index.DeleteItem(oldTags[key], crKey);
                }
            }
        }
        private Index GetIndex(string bucket, string tagName, LightningTransaction transaction)
        {
            string indexName = bucket + "_tags_" + tagName;
            if (!indexMetaInfo.ContainsKey(indexName))
            {
                this.StoreIndexInfo(indexName, transaction);
                indexMetaInfo[indexName] = indexName;
            }
            return new Index(indexName, transaction);
        }

        private void StoreIndexInfo(string indexName, LightningTransaction transaction)
        {
            var db=transaction.OpenDatabase(indexInfoDBName, DatabaseOpenFlags.Create);
            byte[] keyBytes = ByteConverter.GetBytes(indexName, typeof(string));

            transaction.Put(db, keyBytes, ByteConverter.GetBytes(indexName, typeof(string)));
        }

        internal List<string> LoadKeysByIndex(Where query, string bucketName, LightningTransaction transaction)
        {
            Index index = this.GetIndex(bucketName, query.TagName, transaction);

            return IndexQueryFinder.FindKeys(index, query);

        }
        internal List<string> GetIndexes()
        {
            return indexMetaInfo.Keys.ToList();
        }
    }
}
