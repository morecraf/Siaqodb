using System;
using System.Collections.Generic;
using System.Text;
using Sqo;
using System.Collections;
using LightningDB;
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
        public string IndexName { get { return indexName; } }

        // Add a new item to the tree.
        public void AddItem(object new_key, int new_value)
        {
            throw new NotImplementedException();
        }

        // Find this item.
        public int[] FindItem(object target_key)
        {
            throw new NotImplementedException();
        }

        public List<int> FindItemsLessThan(object target_key)
        {
            throw new NotImplementedException();
        }

        public List<int> FindItemsLessThanOrEqual(object target_key)
        {
            throw new NotImplementedException();
        }

        public List<int> FindItemsBiggerThan(object target_key)
        {
            throw new NotImplementedException();
        }

        public List<int> FindItemsBiggerThanOrEqual(object target_key)
        {
            throw new NotImplementedException();
        }

        public List<int> FindItemsStartsWith(object target_key, bool defaultComparer, StringComparison stringComparison)
        {
            throw new NotImplementedException();
        }

       
        public void DeleteItem(object target_key,int oid)
        {
            throw new NotImplementedException();
        }
     


    }
}

