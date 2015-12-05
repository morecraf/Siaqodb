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
        public IEnumerable<string> FindItem(object key)
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
                        if (compareResult <= 0)
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

        public List<string> FindItemsBiggerThanOrEqual(object start)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                    if (Util.Compare(currentKey, start) >= 0)
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

        public List<string> FindItemsLessThanOrEqual(object start)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirst();
                List<string> indexValues = new List<string>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                    if (Util.Compare(currentKey, start) <= 0)
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
                        if (compareResult <= 0)
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


        public void Dispose()
        {
        }
    }
}
