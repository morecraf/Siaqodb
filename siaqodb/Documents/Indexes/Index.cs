using LightningDB;
using Sqo.Documents.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents.Indexes
{
    class Index : IIndex
    {
        string indexName;
        LightningTransaction transaction;
        LightningDatabase db;
        public Index(string indexName, LightningTransaction transaction)
        {
            this.indexName = indexName;
            this.transaction = transaction;
            db = transaction.OpenDatabase(indexName, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);
        }
        public void AddItem(object key, string value)
        {
            byte[] keyBytes = ByteConverter.GetBytes(key, key.GetType());

            transaction.Put(db, keyBytes, ByteConverter.GetBytes(value, value.GetType()));
        }
        public void DeleteItem(object key, string value)
        {

            byte[] keyBytes = ByteConverter.GetBytes(key, key.GetType());
            byte[] valueBytes = ByteConverter.GetBytes(value, value.GetType());

            transaction.Delete(db, keyBytes, valueBytes);
        }


        public IEnumerable<string> FindItem(object key)
        {
            
            byte[] keyBytes = ByteConverter.GetBytes(key, key.GetType());

            List<string> duplicates = new List<string>();
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, key.GetType());
                    if (Util.Compare(currentKey, key) == 0)
                    {
                        ReadDuplicates(firstKV, duplicates, cursor);
                    }

                }
            }
            if (duplicates.Count > 0)
                return duplicates;
            return null;
        }

        public List<string> FindItemsLessThanOrEqual(object start)
        {
           
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirst();
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    if (Util.Compare(currentKey, start) <= 0)
                    {
                        ReadDuplicates(firstKV, indexValues, cursor);
                    }
                }
                while (firstKV.HasValue)
                {
                    firstKV = cursor.MoveNext();
                    if (firstKV.HasValue)
                    {
                        object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                        int compareResult = Util.Compare(currentKey, start);
                        if (compareResult <= 0)
                        {
                            ReadDuplicates(firstKV, indexValues, cursor);
                        }
                        if (compareResult >= 0)
                            break;
                    }

                }

                return indexValues;
            }
        }

        public List<string> FindItemsBiggerThanOrEqual(object start)
        {
           
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    if (Util.Compare(currentKey, start) >= 0)
                    {
                        ReadDuplicates(firstKV, indexValues, cursor);
                    }
                }
                while (firstKV.HasValue)
                {
                    firstKV = cursor.MoveNext();
                    if (firstKV.HasValue)
                    {
                        ReadDuplicates(firstKV, indexValues, cursor);
                    }

                }

                return indexValues;
            }
        }
        public List<string> FindItemsBetween(object start, object end)
        {
            
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    if (Util.Compare(currentKey, start) >= 0)
                    {
                        ReadDuplicates(firstKV, indexValues, cursor);
                    }
                }
                while (firstKV.HasValue)
                {
                    firstKV = cursor.MoveNext();
                    if (firstKV.HasValue)
                    {
                        object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                        int compareResult = Util.Compare(currentKey, end);
                        if (compareResult <= 0)
                        {
                            ReadDuplicates(firstKV, indexValues, cursor);
                        }
                        if (compareResult >= 0)
                            break;
                    }

                }

                return indexValues;
            }
        }
        private void ReadDuplicates(KeyValuePair<byte[], byte[]>? firstKV, List<string> duplicates, LightningCursor cursor)
        {
            object currentVal = ByteConverter.ReadBytes(firstKV.Value.Value, typeof(string));
            duplicates.Add((string)currentVal);
            var currentDuplicate = cursor.MoveNextDuplicate();
            while (currentDuplicate.HasValue)
            {
                object currentValDup = ByteConverter.ReadBytes(currentDuplicate.Value.Value, typeof(string));
                duplicates.Add((string)currentValDup);
                currentDuplicate = cursor.MoveNextDuplicate();
            }
        }

        public void Dispose()
        {
            db.Close();
        }
    }
}
