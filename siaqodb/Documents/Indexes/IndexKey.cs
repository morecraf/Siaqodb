using LightningDB;
using Sqo.Documents.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents.Indexes
{
    class IndexKey : IIndex
    {
        LightningTransaction transaction;
        LightningDatabase db;

        public IndexKey(LightningDatabase db, LightningTransaction transaction)
        {
            this.db = db;
            this.transaction = transaction;
        }
        public List<string> FindItem(object key)
        {
            byte[] keyBytes = ByteConverter.GetBytes(key, key.GetType());
            byte[] crObjBytes = transaction.Get(db, keyBytes);
            if (crObjBytes != null)
            {
                List<string> indexedValues = new List<string>();
                indexedValues.Add((string)key);
                return indexedValues;
            }
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
                        indexValues.Add((string)currentKey);
                    }

                    current = cursor.MoveNext();
                }

                return indexValues;
            }
        }
        public List<string> FindItemsBetweenExceptStart(object start, object end)
        {
            return FindItemsBetweenStartEnd(start, end, false, true);
        }

        public List<string> FindItemsBetweenExceptEnd(object start, object end)
        {
            return FindItemsBetweenStartEnd(start, end, true, false);
        }

        public List<string> FindItemsBetweenExceptStartEnd(object start, object end)
        {
            return FindItemsBetweenStartEnd(start, end, false, false);
        }
        public List<string> FindItemsBetween(object start, object end)
        {
            return FindItemsBetweenStartEnd(start, end, true, true);
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
                    if (compareResult > 0 || (alsoEqualStart && compareResult==0))
                    {
                        indexValues.Add((string)currentKey);
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
                            indexValues.Add((string)currentKey);
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
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                    int compareResult = Util.Compare(currentKey, start);
                    if (compareResult > 0 || (alsoEqual && compareResult==0))
                    {
                        indexValues.Add((string)currentKey);
                    }
                }
                while (firstKV.HasValue)
                {
                    firstKV = cursor.MoveNext();
                    if (firstKV.HasValue)
                    {
                        object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                        indexValues.Add((string)currentKey);
                    }

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
        private List<string> FindItemsLess(object start,bool alsoEqual)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirst();
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                    int compareResult = Util.Compare(currentKey, start);
                    if (compareResult < 0 || (alsoEqual && compareResult==0))
                    {
                        indexValues.Add((string)currentKey);
                    }
                }
                while (firstKV.HasValue)
                {
                    firstKV = cursor.MoveNext();
                    if (firstKV.HasValue)
                    {
                        object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                        int compareResult = Util.Compare(currentKey, start);
                        if (compareResult < 0 || (alsoEqual && compareResult == 0))
                        {
                            indexValues.Add((string)currentKey);
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
                        if (currentKey.StartsWith(start))
                        {
                            indexValues.Add((string)currentKey);
                        }
                        else
                            return indexValues;
                    }
                    firstKV = cursor.MoveNext();
                }

                return indexValues;
            }
        }


        public void Dispose()
        {
        }

       

       
       

       
    }
}
