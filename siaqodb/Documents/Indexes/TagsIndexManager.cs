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
            return new Index(indexName, transaction);
        }
        internal List<string> LoadKeysByIndex(Where query, string bucketName, LightningTransaction transaction)
        {
            using (Index index = this.GetIndex(bucketName, query.TagName, transaction))
            {
                return IndexQueryFinder.FindKeys(index, query);
            }
        }
    }
}
