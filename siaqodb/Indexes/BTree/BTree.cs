using System;
using System.Collections.Generic;
using System.Text;
using Sqo;
using System.Collections;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Indexes
{
    internal class BTree<T>:IBTree<T>,IBTree where T:IComparable
    {
        #if UNITY3D
        public List<BTreeNode<T>> dirtyNodes = new List<BTreeNode<T>>();
#else
        public Dictionary<BTreeNode<T>, int> dirtyNodes = new Dictionary<BTreeNode<T>, int>(); 
#endif  
        public BTreeNode<T> Root;
        Siaqodb siaqodb;
        IndexInfo2 indexInfo;
        bool allowPersistence=true;

        public BTree(Siaqodb s)
        {
            this.siaqodb = s;
            Root = new BTreeNode<T>(this.siaqodb,this);
            
        }
        
        // Add a new item to the tree.
        public void AddItem(T new_key, int[] new_value)
        {
            if (this.Root.OID == 0)
            {
                this.Root.Persist();
               

            }
            T up_key = default(T);
            int[] up_value = null;
            BTreeNode<T> up_node = null;
            Root.AddItem(new_key, new_value, ref up_key, ref up_value, ref up_node);

            // See if there was a root bucket split.
            if (up_node != null)
            {
                BTreeNode<T> new_root = new BTreeNode<T>(siaqodb,this);
                
                new_root.Keys[0] = up_key;
                new_root.Values[0] = up_value;
                new_root.Children[0] = Root;
                new_root.Children[1] = up_node;
                new_root.NumKeysUsed = 1;

                Root = new_root;
                new_root.Persist();
                
                
            }
        }
#if ASYNC
        public async Task AddItemAsync(T new_key, int[] new_value)
        {
            if (this.Root.OID == 0)
            {
                await this.Root.PersistAsync().ConfigureAwait(false);


            }
            AddedItem<T> addedItem = new AddedItem<T>();
            await Root.AddItemAsync(new_key, new_value, addedItem).ConfigureAwait(false);

            // See if there was a root bucket split.
            if (addedItem.up_node != null)
            {
                BTreeNode<T> new_root = new BTreeNode<T>(siaqodb, this);

                new_root.Keys[0] = addedItem.up_key;
                new_root.Values[0] = addedItem.up_value;
                new_root.Children[0] = Root;
                new_root.Children[1] = addedItem.up_node;
                new_root.NumKeysUsed = 1;

                Root = new_root;
                await new_root.PersistAsync().ConfigureAwait(false);


            }
        }

#endif
        // Find this item.
        public int[] FindItem(T target_key)
        {
            return Root.FindItem(target_key);
        }
#if ASYNC
        public async Task<int[]> FindItemAsync(T target_key)
        {
            return await Root.FindItemAsync(target_key).ConfigureAwait(false);
        }
#endif
        public List<int> FindItemsLessThan(T target_key)
        {
            bool stop = false;
            return Root.FindItemsLessThan(target_key,false,ref stop);
        }
#if ASYNC
        public async Task<List<int>> FindItemsLessThanAsync(T target_key)
        {
            StopIndicator stop = new StopIndicator();
            return await Root.FindItemsLessThanAsync(target_key, false, stop).ConfigureAwait(false);
        }
#endif
        public List<int> FindItemsLessThanOrEqual(T target_key)
        {
            bool stop = false;
            return Root.FindItemsLessThan(target_key, true,ref stop);
        }
#if ASYNC
        public async Task<List<int>> FindItemsLessThanOrEqualAsync(T target_key)
        {
            StopIndicator stop = new StopIndicator();
            return await Root.FindItemsLessThanAsync(target_key, true, stop).ConfigureAwait(false);
        }
#endif
        public List<int> FindItemsBiggerThan(T target_key)
        {
            bool stop = false;
            return Root.FindItemsBiggerThan(target_key, false, ref stop);
        }
#if ASYNC
        public async Task<List<int>> FindItemsBiggerThanAsync(T target_key)
        {
            StopIndicator stop = new StopIndicator();
            return await Root.FindItemsBiggerThanAsync(target_key, false, stop).ConfigureAwait(false);
        }
#endif
        public List<int> FindItemsBiggerThanOrEqual(T target_key)
        {
            bool stop = false;
            return Root.FindItemsBiggerThan(target_key, true, ref stop);
        }
#if ASYNC
        public async Task<List<int>> FindItemsBiggerThanOrEqualAsync(T target_key)
        {
            StopIndicator stop = new StopIndicator();
            return await Root.FindItemsBiggerThanAsync(target_key, true, stop).ConfigureAwait(false);
        }
#endif
        public List<int> FindItemsStartsWith(T target_key)
        {
            bool stop = false;
            return Root.FindItemsStartsWith(target_key, ref stop);
        }
#if ASYNC
        public async Task<List<int>> FindItemsStartsWithAsync(T target_key)
        {
            StopIndicator stop = new StopIndicator();
            return await Root.FindItemsStartsWithAsync(target_key, stop).ConfigureAwait(false);
        }
#endif
        // Remove this item.
        public void RemoveItem(T target_key)
        {
            Root.RemoveItem(target_key);

            // See if the root now has no keys.
            if (Root.NumKeysUsed < 1)
            {
                if (Root.Children[0] != null) Root = Root.Children[0];
            }
        }
        public void RemoveOid(T target_key,int oid)
        {
            Root.RemoveOID(target_key,oid);

        }
#if ASYNC
        public async Task RemoveOidAsync(T target_key, int oid)
        {
            await Root.RemoveOIDAsync(target_key, oid).ConfigureAwait(false);

        }
#endif
       public void Dump()
        {
            Root.Dump(0);
        }
#if ASYNC
       public async Task DumpAsync()
       {
           await Root.DumpAsync(0).ConfigureAwait(false);
       }
     

#endif
       public List<T> DumpKeys()
       {
           return Root.DumpKeys();
       }
#if ASYNC
       public async Task<List<T>> DumpKeysAsync()
       {
           return await Root.DumpKeysAsync().ConfigureAwait(false);
       }
#endif
       public int NrNodes()
       {
           return 1 + Root.NrNodes();
       }
#if ASYNC
       public async Task<int> NrNodesAsync()
       {
           return 1 + await Root.NrNodesAsync().ConfigureAwait(false);
       }
#endif
       public void Persist()
       {
           if (allowPersistence)
           {
               if (dirtyNodes.Count > 0)
               {
#if UNITY3D
                  IEnumerable<BTreeNode<T>> dirtyNodesCol=dirtyNodes;
#else
                    IEnumerable<BTreeNode<T>> dirtyNodesCol=dirtyNodes.Keys;
#endif
                  foreach (BTreeNode<T> node in dirtyNodesCol)
                   {
                       siaqodb.StoreObject(node);
                   }

                   dirtyNodes.Clear();
                   if (this.indexInfo.RootOID != this.Root.OID)
                   {
                       this.indexInfo.RootOID = this.Root.OID;
                       siaqodb.StoreObject(this.indexInfo);
                   }
               }
           }
       }
#if ASYNC
       public async Task PersistAsync()
       {
           if (allowPersistence)
           {
               if (dirtyNodes.Count > 0)
               {
#if UNITY3D
                  IEnumerable<BTreeNode<T>> dirtyNodesCol=dirtyNodes;
#else
                   IEnumerable<BTreeNode<T>> dirtyNodesCol = dirtyNodes.Keys;
#endif
                   foreach (BTreeNode<T> node in dirtyNodesCol)
                   {
                       await siaqodb.StoreObjectAsync(node).ConfigureAwait(false);
                   }

                   dirtyNodes.Clear();
                   if (this.indexInfo.RootOID != this.Root.OID)
                   {
                       this.indexInfo.RootOID = this.Root.OID;
                       await siaqodb.StoreObjectAsync(this.indexInfo).ConfigureAwait(false);
                   }
               }

           }
       }

#endif
       public void AddItem(object new_key, int[] new_value)
       {
           this.AddItem((T)new_key, new_value);
       }
#if ASYNC
       public async Task AddItemAsync(object new_key, int[] new_value)
       {
           await this.AddItemAsync((T)new_key, new_value).ConfigureAwait(false);
       }
#endif
       public int[] FindItem(object target_key)
       {
           if (target_key != null)
           {
               if (target_key.GetType() != typeof(T))
               {
                   target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
               }
           }
           return this.FindItem((T)target_key);
       }
#if ASYNC
       public async Task<int[]> FindItemAsync(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return await this.FindItemAsync((T)target_key).ConfigureAwait(false);
       }
#endif
       public List<int> FindItemsLessThan(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return FindItemsLessThan((T)target_key);
       }
#if ASYNC
       public async Task<List<int>> FindItemsLessThanAsync(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return await FindItemsLessThanAsync((T)target_key).ConfigureAwait(false);
       }
#endif
       public List<int> FindItemsLessThanOrEqual(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return FindItemsLessThanOrEqual((T)target_key);
       }
#if ASYNC
       public async Task<List<int>> FindItemsLessThanOrEqualAsync(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return await FindItemsLessThanOrEqualAsync((T)target_key).ConfigureAwait(false);
       }
#endif
       public List<int> FindItemsBiggerThan(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return FindItemsBiggerThan((T)target_key);
       }
#if ASYNC
       public async Task<List<int>> FindItemsBiggerThanAsync(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return await FindItemsBiggerThanAsync((T)target_key).ConfigureAwait(false);
       }
#endif
       public List<int> FindItemsBiggerThanOrEqual(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return FindItemsBiggerThanOrEqual((T)target_key);
       }
#if ASYNC
       public async Task<List<int>> FindItemsBiggerThanOrEqualAsync(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return await FindItemsBiggerThanOrEqualAsync((T)target_key).ConfigureAwait(false);
       }
#endif
       public List<int> FindItemsStartsWith(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return FindItemsStartsWith((T)target_key);
       }
#if ASYNC
       public async Task<List<int>> FindItemsStartsWithAsync(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           return await FindItemsStartsWithAsync((T)target_key).ConfigureAwait(false);
       }
#endif
       public void RemoveItem(object target_key)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           RemoveItem((T)target_key);
       }
       public void RemoveOid(object target_key,int oid)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           RemoveOid((T)target_key,oid);
       }
#if ASYNC
       public async Task RemoveOidAsync(object target_key, int oid)
       {
           if (target_key.GetType() != typeof(T))
           {
               target_key = Sqo.Utilities.Convertor.ChangeType(target_key, typeof(T));
           }
           await RemoveOidAsync((T)target_key, oid).ConfigureAwait(false);
       }
#endif
       public void SetRoot(object root)
       {
           if (root.GetType() != typeof(BTreeNode<T>))
           {
               throw new InvalidOperationException("Root is not " + typeof(T).ToString() + " type");
           }
           this.Root = (BTreeNode<T>)root;

           this.Root.siaqodb = this.siaqodb;
           this.Root.btree = this;
       }
       public int GetRootOid()
       {
           return this.Root.OID;
       }
       public void SetIndexInfo(IndexInfo2 indexInfo)
       {
           this.indexInfo = indexInfo;
       }
       public void Drop(bool withAllNodes)
       {
           if (withAllNodes)
           {
               List<BTreeNode<T>> nodesDumped = this.Root.DumpNodes();
               foreach (BTreeNode<T> n in nodesDumped)
               {
                   if (n.OID > 0)
                   {
                       siaqodb.Delete(n);

                   }
               }
           }
           else
           {
               this.siaqodb.Delete(this.Root);
               this.siaqodb.Delete(this.indexInfo);
           }
       }
#if ASYNC
       public async Task DropAsync(bool withAllNodes)
       {
           if (withAllNodes)
           {
               List<BTreeNode<T>> nodesDumped = await this.Root.DumpNodesAsync().ConfigureAwait(false);
               foreach (BTreeNode<T> n in nodesDumped)
               {
                   if (n.OID > 0)
                   {
                       await siaqodb.DeleteAsync(n).ConfigureAwait(false);

                   }
               }
           }
           else
           {
               await this.siaqodb.DeleteAsync(this.Root).ConfigureAwait(false);
               await this.siaqodb.DeleteAsync(this.indexInfo).ConfigureAwait(false);
           }
       }
#endif
       public void AllowPersistance(bool allow)
       {
           this.allowPersistence = allow;
       }
    }
#if ASYNC
    class AddedItem<T> where T : IComparable
    {
        public T up_key;
        public int[] up_value;
        public BTreeNode<T> up_node;
    }
#endif

#if UNITY3D
    
    /// <summary>
    /// put this class just to be recognized by AOT compiler
    /// </summary>
    internal class DummyBtree
    {
        public BTree<int> bint;
        public BTree<uint> buint;
        public BTree<float> bfloat;
        public BTree<long> blong;
        public BTree<ulong> bulong;
        public BTree<short> bshort;
        public BTree<ushort> bushort;
        public BTree<byte> bbyte;
        public BTree<sbyte> bsbyte;
        public BTree<double> bdouble;
        public BTree<decimal> bdecimal;
        public BTree<char> bchar;
        public BTree<TimeSpan> bTimeSpan;
        public BTree<DateTime> bDateTime;
        public BTree<Guid> bGuid;
		public BTree<bool> bBool;
		public BTree<string> bStr;
        public BTreeNode<int> bnint=new BTreeNode<int>();
        public BTreeNode<uint> bnuint=new BTreeNode<uint>();
        public BTreeNode<float> bnfloat=new BTreeNode<float>();
        public BTreeNode<long> bnlong=new BTreeNode<long>();
        public BTreeNode<ulong> bnulong=new BTreeNode<ulong>();
        public BTreeNode<short> bnshort=new BTreeNode<short>();
        public BTreeNode<ushort> bnushort=new BTreeNode<ushort>();
        public BTreeNode<byte> bnbyte=new BTreeNode<byte>();
        public BTreeNode<sbyte> bnsbyte=new BTreeNode<sbyte>();
        public BTreeNode<double> bndouble=new BTreeNode<double>();
        public BTreeNode<decimal> bndecimal=new BTreeNode<decimal>();
        public BTreeNode<char> bnchar=new BTreeNode<char>();
        public BTreeNode<TimeSpan> bnTimeSpan=new BTreeNode<TimeSpan>();
        public BTreeNode<DateTime> bnDateTime=new BTreeNode<DateTime>();
        public BTreeNode<Guid> bnGuid=new BTreeNode<Guid>();
		public BTreeNode<bool> bnBool=new BTreeNode<bool>();
		public BTreeNode<string> bnStr=new BTreeNode<string>();
       
        public DummyBtree(Siaqodb siaqodb)
        {
            bint = new BTree<int>(siaqodb);
            buint = new BTree<uint>(siaqodb);
            bfloat = new BTree<float>(siaqodb);
            blong = new BTree<long>(siaqodb);
            bulong = new BTree<ulong>(siaqodb);
            bshort = new BTree<short>(siaqodb);
            bushort = new BTree<ushort>(siaqodb);
            bbyte = new BTree<byte>(siaqodb);
            bsbyte = new BTree<sbyte>(siaqodb);
            bdouble = new BTree<double>(siaqodb);
            bdecimal = new BTree<decimal>(siaqodb);
            bchar = new BTree<char>(siaqodb);
            bTimeSpan = new BTree<TimeSpan>(siaqodb);
            bDateTime = new BTree<DateTime>(siaqodb);
            bGuid = new BTree<Guid>(siaqodb);
			bBool=new BTree<bool>(siaqodb);
			bStr=new BTree<string>(siaqodb);
        }

    }
#endif
}

