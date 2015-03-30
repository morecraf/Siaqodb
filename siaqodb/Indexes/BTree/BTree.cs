using System;
using System.Collections.Generic;
using System.Text;
using Sqo;
using System.Collections;
using LightningDB;
using Sqo.Core;
using Sqo.Utilities;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Indexes
{
    internal class BTree : IBTree
    {


        string indexName;
        LightningTransaction transaction;
        LightningDatabase db;

        public BTree(string indexName, LightningTransaction transaction)
        {
            this.indexName = indexName;
            this.transaction = transaction;
            this.db = transaction.OpenDatabase(indexName, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);

        }
        public void Drop()
        {
            transaction.DropDatabase(db, true);
        }
        public string IndexName { get { return indexName; } }

        // Add a new item to the tree.
        public void AddItem(object new_key, int new_value)
        {
            byte[] key=ByteConverter.GetBytes(new_key,new_key.GetType());
            transaction.Put(db,key , ByteConverter.IntToByteArray(new_value));
        }

        // Find this item.
        public IEnumerable<int> FindItem(object key)
        {
            byte[] keyBytes = ByteConverter.GetBytes(key, key.GetType());

            List<int> duplicates = new List<int>();
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, key.GetType());
                    if (Compare(currentKey, key) == 0)
                    {
                        ReadDuplicates(firstKV, duplicates, cursor);
                    }

                }
            }
            if (duplicates.Count > 0)
                return duplicates;
            return null;
        }

        public List<int> FindItemsLessThan(object start)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirst();
                List<int> indexValues = new List<int>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    if (Compare(currentKey, start) < 0)
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
                        int compareResult = Compare(currentKey, start);
                        if (compareResult < 0)
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

        public List<int> FindItemsLessThanOrEqual(object start)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirst();
                List<int> indexValues = new List<int>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    if (Compare(currentKey, start) <= 0)
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
                        int compareResult = Compare(currentKey, start);
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

        public List<int> FindItemsBiggerThan(object start)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<int> indexValues = new List<int>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    if (Compare(currentKey, start) > 0)
                    {
                        ReadDuplicates(firstKV, indexValues, cursor);
                    }
                    else //move cursor after all duplicates
                    {
                        var currentDuplicate = cursor.MoveNextDuplicate();
                        while (currentDuplicate.HasValue)
                        {
                            currentDuplicate = cursor.MoveNextDuplicate();
                        }

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

        public List<int> FindItemsBiggerThanOrEqual(object start)
        {
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<int> indexValues = new List<int>();
                if (firstKV.HasValue)
                {
                    object currentKey = ByteConverter.ReadBytes(firstKV.Value.Key, start.GetType());
                    if (Compare(currentKey, start) >= 0)
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

        public List<int> FindItemsStartsWith(object target_key, bool defaultComparer, StringComparison stringComparison)
        {
            string start=(string)target_key;
            byte[] keyBytes = ByteConverter.GetBytes(start, start.GetType());
            using (var cursor = transaction.CreateCursor(this.db))
            {
                var firstKV = cursor.MoveToFirstAfter(keyBytes);
                List<int> indexValues = new List<int>();

                while (firstKV.HasValue)
                {
                    string currentKey = (string)ByteConverter.ReadBytes(firstKV.Value.Key, typeof(string));
                    if (this.StringCompare(currentKey, start, defaultComparer, stringComparison) >= 0)
                    {
                        if (this.StringStartsWith(currentKey, start, defaultComparer, stringComparison))
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
        private int StringCompare(string a,string b,bool defaultComparer, StringComparison stringComparison)
        {
            if (defaultComparer)
            {
                return string.Compare(a, b);
            }
            else
            {
                return string.Compare(a, b, stringComparison);
            }
        }
        private bool StringStartsWith(string a, string startsWith, bool defaultComparer, StringComparison stringComparison)
        {
            if (defaultComparer)
                return a.StartsWith(startsWith);
            else
                return a.StartsWith(startsWith, stringComparison);
        }
        public void DeleteItem(object key,int oid)
        {
            byte[] keyBytes = ByteConverter.GetBytes(key, key.GetType());
            byte[] valueBytes = ByteConverter.GetBytes(oid, typeof(int));

            transaction.Delete(db, keyBytes, valueBytes);
        }
        private void ReadDuplicates(KeyValuePair<byte[], byte[]>? firstKV, List<int> duplicates, LightningCursor cursor)
        {
            object currentVal = ByteConverter.ReadBytes(firstKV.Value.Value, typeof(int));
            duplicates.Add((int)currentVal);
            var currentDuplicate = cursor.MoveNextDuplicate();
            while (currentDuplicate.HasValue)
            {
                object currentValDup = ByteConverter.ReadBytes(currentDuplicate.Value.Value, typeof(int));
                duplicates.Add((int)currentValDup);
                currentDuplicate = cursor.MoveNextDuplicate();
            }
        }

        public static int Compare(object a, object b)
        {
            int c = 0;
            if (a == null || b == null)
            {
                if (a == b)
                    c = 0;
                else if (a == null)
                    c = -1;
                else if (b == null)
                    c = 1;
            }
            else
            {
                if (b.GetType() != a.GetType())
                {
                    b = Convertor.ChangeType(b, a.GetType());
                }
                c = ((IComparable)a).CompareTo(b);
            }
            return c;
        }
    }
}

