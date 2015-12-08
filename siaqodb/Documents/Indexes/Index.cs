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


        public List<string> FindItem(object key)
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
        public List<string> FindAllExcept(object key)
        {
            byte[] keyBytes = ByteConverter.GetBytes(key, key.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var current = cursor.MoveNext();
                List<string> indexValues = new List<string>();
                while (current.HasValue)
                {

                    object currentKey = ByteConverter.ReadBytes(current.Value.Key, key.GetType());
                    int compareResult = Util.Compare(currentKey, key);
                    if (compareResult != 0)
                    {
                        ReadDuplicates(current, indexValues, cursor);
                    }

                    current = cursor.MoveNext();
                }

                return indexValues;
            }
        }

        public List<string> FindItemsLessThan(object start)
        {
            return FindItemsLess(start, false);
        }
        public List<string> FindItemsLessThanOrEqual(object start)
        {

            return FindItemsLess(start, true);
        }
        private List<string> FindItemsLess(object start, bool alsoEqual)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirst();
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    int compareResult = Util.Compare(currentKey, start);
                    if  (compareResult < 0 || (alsoEqual && compareResult == 0))
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
                        if (compareResult < 0 || (alsoEqual && compareResult == 0))
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
        public List<string> FindItemsBiggerThan(object start)
        {
            return this.FindItemsBigger(start, false);
        }
        public List<string> FindItemsBiggerThanOrEqual(object start)
        {
            return this.FindItemsBigger(start, true);
        }
        private List<string> FindItemsBigger(object start,bool alsoEqual)
        {
           
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    int compareResult = Util.Compare(currentKey, start);
                    if (compareResult  > 0 || (alsoEqual && compareResult==0))
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
            return this.FindItemsBetweenStartEnd(start, end, true, true);
        }
        public List<string> FindItemsBetweenExceptStart(object start, object end)
        {
            return this.FindItemsBetweenStartEnd(start, end, false, true);
        }
        public List<string> FindItemsBetweenExceptEnd(object start, object end)
        {
            return this.FindItemsBetweenStartEnd(start, end, true, false);
        }
        public List<string> FindItemsBetweenExceptStartEnd(object start, object end)
        {
            return this.FindItemsBetweenStartEnd(start, end, false, false);
        }
        private List<string> FindItemsBetweenStartEnd(object start, object end,bool alsoEqualStart,bool alsoEqualEnd)
        {
            
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    int compareResult = Util.Compare(currentKey, start);
                    if (compareResult > 0 || (alsoEqualStart && compareResult == 0))
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
                        if (compareResult < 0 || (alsoEqualEnd && compareResult == 0))
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
        public List<string> FindItemsStartsWith(object target_key)
        {
            string start = (string)target_key;
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<string> indexValues = new List<string>();

                while (firstKV.HasValue)
                {
                    string currentKey = (string)ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                    if (string.Compare(currentKey, start) >= 0)
                    {
                        if (currentKey.StartsWith( start))
                        {
                            ReadDuplicates(firstKV, indexValues, cursor);
                        }
                        else
                            return indexValues;
                    }
                    firstKV = cursor.MoveNext();
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
