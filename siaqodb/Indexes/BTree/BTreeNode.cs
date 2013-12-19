using System;
using System.Collections.Generic;
using System.Text;
using Sqo.Attributes;
using Sqo;
using System.Linq;
using System.Diagnostics;
#if ASYNC
using System.Threading.Tasks;

#endif

namespace Sqo.Indexes
{

    [System.Reflection.ObfuscationAttribute(Feature = "renaming", ApplyToMembers = false)]
    [System.Reflection.ObfuscationAttribute(Feature="Apply to member * when field and private: renaming",Exclude=true)]
    internal class BTreeNode<T> where T : IComparable
    {
         [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public int OID { get; set; }
        [Ignore]
        private const int HALF_NUM_KEYS = 32;
        [Ignore]
        public const int KEYS_PER_NODE = 2 * HALF_NUM_KEYS;
        [Ignore]
        public const int CHILDREN_PER_NODE = KEYS_PER_NODE + 1;
        [Ignore]
        public bool childrenLoaded = false;
        [Ignore]
        public Siaqodb siaqodb;
        [Ignore]
        public BTree<T> btree;

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public int NumKeysUsed = 0;
        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public T[] Keys = new T[KEYS_PER_NODE];
        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public int[][] Values = new int[KEYS_PER_NODE][];
        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        private int[] _childrenOIDs = new int[CHILDREN_PER_NODE];

        
        [Ignore]
        private BTreeNode<T>[] _children = new BTreeNode<T>[CHILDREN_PER_NODE];

        public BTreeNode<T>[] Children
        {
            get
            {
                return _children;
            }
            set { this._children = value; }
        }
        public void CheckChildren()
        {
            if (!childrenLoaded)
            {
                int i = 0;
                foreach (int nodeOID in _childrenOIDs)
                {
                    if (nodeOID > 0)
                    {
                        _children[i] = siaqodb.LoadObjectByOID<BTreeNode<T>>(nodeOID);
                        _children[i].siaqodb = this.siaqodb;
                        _children[i].btree = this.btree;
                    }
                    i++;
                }
                childrenLoaded = true;
            }
        }
#if ASYNC
        public async Task CheckChildrenAsync()
        {
            if (!childrenLoaded)
            {
                int i = 0;
                foreach (int nodeOID in _childrenOIDs)
                {
                    if (nodeOID > 0)
                    {
                        _children[i] = await siaqodb.LoadObjectByOIDAsync<BTreeNode<T>>(nodeOID).ConfigureAwait(false);
                        _children[i].siaqodb = this.siaqodb;
                        _children[i].btree = this.btree;
                    }
                    i++;
                }
                childrenLoaded = true;
            }
        }
#endif
        public BTreeNode()
        {

        }
        public BTreeNode(Siaqodb siaqodb,BTree<T> btree)
        {
            this.btree = btree;
            this.childrenLoaded = true;
            this.siaqodb = siaqodb;
        }

        #region Find methods
        // Find an item in this subtree.
        public int[] FindItem(T target_key)
        {

            
            // Find the key after the spot where this item goes.
            int spot = 0;
            while (spot < NumKeysUsed)
            {
                // See if this key comes after the new value.
                if (this.Compare(Keys[spot], target_key) >= 0) break;
                spot++;
            }

            // If we found the item, return it.
            if ((spot < NumKeysUsed) && (this.Compare(Keys[spot], target_key) == 0)) return Values[spot];

            // If there's no link to follow, we're at a leaf
            // and didn't find it so return null.
            CheckChildren();
            if (Children[spot] == null) return null;

            // Look in the proper subtree.
            return Children[spot].FindItem(target_key);
        }
#if ASYNC
        public async Task<int[]> FindItemAsync(T target_key)
        {


            // Find the key after the spot where this item goes.
            int spot = 0;
            while (spot < NumKeysUsed)
            {
                // See if this key comes after the new value.
                if (this.Compare(Keys[spot], target_key) >= 0) break;
                spot++;
            }

            // If we found the item, return it.
            if ((spot < NumKeysUsed) && (this.Compare(Keys[spot], target_key) == 0)) return Values[spot];

            // If there's no link to follow, we're at a leaf
            // and didn't find it so return null.
            await CheckChildrenAsync().ConfigureAwait(false);
            if (Children[spot] == null) return null;

            // Look in the proper subtree.
            return await Children[spot].FindItemAsync(target_key).ConfigureAwait(false);
        }
#endif
        public List<int> FindItemsLessThan(T target_key,bool includeTarget,ref bool stop)
        {
            List<int> foundValues =new List<int>();
            
            if (stop) return foundValues;

            CheckChildren();
            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {

                    foundValues.AddRange(Children[i].FindItemsLessThan(target_key,includeTarget,ref stop));

                }
                if (stop) return foundValues;
                int c = this.Compare(Keys[i], target_key);
                if (c >= 0)
                {
                    if (includeTarget && c == 0)
                    {
                        foundValues.AddRange(Values[i]);
                    }
                    stop = true;
                    return foundValues;

                }

                foundValues.AddRange(Values[i]);
            }
            
            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    foundValues.AddRange(Children[NumKeysUsed].FindItemsLessThan(target_key,includeTarget,ref stop));

                }
            }
            return foundValues;
        }
#if ASYNC
        public async Task<List<int>> FindItemsLessThanAsync(T target_key, bool includeTarget, StopIndicator stopInd)
        {
            List<int> foundValues = new List<int>();

            if (stopInd.Stop) return foundValues;
            await CheckChildrenAsync().ConfigureAwait(false);

            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {

                    foundValues.AddRange(await Children[i].FindItemsLessThanAsync(target_key, includeTarget, stopInd).ConfigureAwait(false));

                }
                if (stopInd.Stop) return foundValues;
                int c = this.Compare(Keys[i], target_key);
                if (c >= 0)
                {
                    if (includeTarget && c == 0)
                    {
                        foundValues.AddRange(Values[i]);
                    }
                    stopInd.Stop = true;
                    return foundValues;

                }

                foundValues.AddRange(Values[i]);
            }

            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    foundValues.AddRange(await Children[NumKeysUsed].FindItemsLessThanAsync(target_key, includeTarget, stopInd).ConfigureAwait(false));

                }
            }
            return foundValues;
        }
#endif
        public List<int> FindItemsBiggerThan(T target_key, bool includeTarget, ref bool stop)
        {
            List<int> foundValues = new List<int>();
            CheckChildren();
            if (Children[NumKeysUsed] != null)
            {
                foundValues.AddRange(Children[NumKeysUsed].FindItemsBiggerThan(target_key, includeTarget, ref stop));
            }
            if (stop)
                return foundValues;

            for (int i = NumKeysUsed-1 ; i >= 0; i--)
            {
                int c = this.Compare(Keys[i], target_key);
               
                if (c <= 0)
                {
                    if (includeTarget && c == 0)
                    {
                        foundValues.AddRange(Values[i]);
                    }
                    stop = true;
                    return foundValues;

                }
                foundValues.AddRange(Values[i]);
                if (Children[i] != null)
                {
                    foundValues.AddRange(Children[i].FindItemsBiggerThan(target_key, includeTarget, ref stop));

                }

            }

            return foundValues;
        }
#if ASYNC
        public async Task<List<int>> FindItemsBiggerThanAsync(T target_key, bool includeTarget, StopIndicator stopInd)
        {
            List<int> foundValues = new List<int>();
            await CheckChildrenAsync().ConfigureAwait(false);

            if (Children[NumKeysUsed] != null)
            {
                foundValues.AddRange(await Children[NumKeysUsed].FindItemsBiggerThanAsync(target_key, includeTarget, stopInd).ConfigureAwait(false));
            }
            if (stopInd.Stop)
                return foundValues;

            for (int i = NumKeysUsed - 1; i >= 0; i--)
            {
                int c = this.Compare(Keys[i], target_key);

                if (c <= 0)
                {
                    if (includeTarget && c == 0)
                    {
                        foundValues.AddRange(Values[i]);
                    }
                    stopInd.Stop= true;
                    return foundValues;

                }
                foundValues.AddRange(Values[i]);
                if (Children[i] != null)
                {
                    foundValues.AddRange(await Children[i].FindItemsBiggerThanAsync(target_key, includeTarget, stopInd).ConfigureAwait(false));

                }

            }

            return foundValues;
        }
#endif
        public List<int> FindItemsStartsWith(T target_key, bool defaultComparer, StringComparison stringComparison, ref bool stop)
        {
            if (typeof(T) != typeof(String))
                throw new InvalidOperationException("Only string Keys may use this operation");

            string target_keyStr =Convert.ToString(target_key);
            CheckChildren();
            List<int> foundValues = new List<int>();

            if (Children[NumKeysUsed] != null)
            {
                foundValues.AddRange(Children[NumKeysUsed].FindItemsStartsWith(target_key,defaultComparer,stringComparison, ref stop));
            }
            if (stop)
                return foundValues;

            for (int i = NumKeysUsed - 1; i >= 0; i--)
            {
                string keyStr = Convert.ToString(Keys[i]);
                   
                if (defaultComparer)
                {
                    if (string.Compare(keyStr, target_keyStr) <= 0)
                    {
                        if (keyStr.StartsWith(target_keyStr))
                        {
                            foundValues.AddRange(Values[i]);
                        }
                        stop = true;
                        return foundValues;

                    }
                    if (keyStr.StartsWith(target_keyStr))
                    {
                        foundValues.AddRange(Values[i]);
                    }
                }
                else
                {
                    if (string.Compare(keyStr, target_keyStr, stringComparison) <= 0)
                    {
                        if (keyStr.StartsWith(target_keyStr, stringComparison))
                        {
                            foundValues.AddRange(Values[i]);
                        }
                        stop = true;
                        return foundValues;

                    }
                    if (keyStr.StartsWith(target_keyStr, stringComparison))
                    {
                        foundValues.AddRange(Values[i]);
                    }
                }
                if (Children[i] != null)
                {
                    foundValues.AddRange(Children[i].FindItemsStartsWith(target_key,defaultComparer,stringComparison, ref stop));

                }

            }

            return foundValues;
        }
#if ASYNC
        public async Task<List<int>> FindItemsStartsWithAsync(T target_key,bool defaultComparer,StringComparison stringComparison, StopIndicator stopInd)
        {
            if (typeof(T) != typeof(String))
                throw new InvalidOperationException("Only string Keys may use this operation");

            string target_keyStr = Convert.ToString(target_key);

            List<int> foundValues = new List<int>();
            await CheckChildrenAsync().ConfigureAwait(false);
            if (Children[NumKeysUsed] != null)
            {
                foundValues.AddRange(await Children[NumKeysUsed].FindItemsStartsWithAsync(target_key, defaultComparer, stringComparison, stopInd).ConfigureAwait(false));
            }
            if (stopInd.Stop)
                return foundValues;

            for (int i = NumKeysUsed - 1; i >= 0; i--)
            {
                string keyStr = Convert.ToString(Keys[i]);

                if (defaultComparer)
                {
                    if (string.Compare(keyStr, target_keyStr) <= 0)
                    {
                        if (keyStr.StartsWith(target_keyStr))
                        {
                            foundValues.AddRange(Values[i]);
                        }
                        stopInd.Stop = true;
                        return foundValues;

                    }
                    if (keyStr.StartsWith(target_keyStr))
                    {
                        foundValues.AddRange(Values[i]);
                    }
                }
                else
                {
                    if (string.Compare(keyStr, target_keyStr, stringComparison) <= 0)
                    {
                        if (keyStr.StartsWith(target_keyStr, stringComparison))
                        {
                            foundValues.AddRange(Values[i]);
                        }
                        stopInd.Stop = true;
                        return foundValues;

                    }
                    if (keyStr.StartsWith(target_keyStr, stringComparison))
                    {
                        foundValues.AddRange(Values[i]);
                    }
                }
                if (Children[i] != null)
                {
                    foundValues.AddRange(await Children[i].FindItemsStartsWithAsync(target_key,defaultComparer,stringComparison, stopInd).ConfigureAwait(false));

                }

            }

            return foundValues;
        }
#endif
        #endregion
        private int Compare(object a, object b)
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
                    b = Sqo.Utilities.Convertor.ChangeType(b, a.GetType());
                }
                c = ((IComparable)a).CompareTo(b);
            }
            return c;
        }
        #region Add Methods
        // Add a value to this subtree.
        public void AddItem(T new_key, int[] new_value, ref T up_key, ref int[] up_value, ref BTreeNode<T> up_node)
        {
            // Find the key after the spot where this item goes.
            int spot = 0;
            while (spot < NumKeysUsed)
            {

                
                if (this.Compare(Keys[spot],new_key) >= 0) break;
                spot++;
            }

            // See if we found it.
            if ((spot < NumKeysUsed) && (this.Compare(Keys[spot], new_key) == 0))
            {
                Array.Resize<int>(ref Values[spot], Values[spot].Length + 1);
                Values[spot][Values[spot].Length - 1] = new_value[0];
                up_key = default(T);
                up_value = null;
                up_node = null;
                this.Persist();
                return;

            }
            CheckChildren();
            // See if we are in a leaf node.
            if (Children[0] == null)
            {
                // This is a leaf.
                AddItemToNode(spot, new_key, new_value, null, ref up_key, ref up_value, ref up_node);
            } else {
                // This is not a leaf. Move into the proper subtree.
                Children[spot].AddItem(new_key, new_value, ref up_key, ref up_value, ref up_node);

                // See if we had a bucket split.
                if (up_node != null)
                {
                    // We had a bucket split. Add the new bucket here.
                    AddItemToNode(spot, up_key, up_value, up_node, ref up_key, ref up_value, ref up_node);
                }
            }
        }
#if ASYNC
        public async Task AddItemAsync(T new_key, int[] new_value, AddedItem<T> addedItem)
        {
            // Find the key after the spot where this item goes.
            int spot = 0;
            while (spot < NumKeysUsed)
            {


                if (this.Compare(Keys[spot], new_key) >= 0) break;
                spot++;
            }

            // See if we found it.
            if ((spot < NumKeysUsed) && (this.Compare(Keys[spot], new_key) == 0))
            {
                Array.Resize<int>(ref Values[spot], Values[spot].Length + 1);
                Values[spot][Values[spot].Length - 1] = new_value[0];
                addedItem.up_key = default(T);
                addedItem.up_value = null;
                addedItem.up_node = null;
                await this.PersistAsync().ConfigureAwait(false);
                return;

            }
            await CheckChildrenAsync().ConfigureAwait(false);
            // See if we are in a leaf node.
            if (Children[0] == null)
            {
                // This is a leaf.
                await AddItemToNodeAsync(spot, new_key, new_value, null, addedItem).ConfigureAwait(false);
            }
            else
            {
                // This is not a leaf. Move into the proper subtree.
                await Children[spot].AddItemAsync(new_key, new_value, addedItem).ConfigureAwait(false);

                // See if we had a bucket split.
                if (addedItem.up_node != null)
                {
                    // We had a bucket split. Add the new bucket here.
                    await AddItemToNodeAsync(spot, addedItem.up_key, addedItem.up_value, addedItem.up_node, addedItem).ConfigureAwait(false);
                }
            }
        }
#endif
      
        // Add the new item to this node, if it fits.
        private void AddItemToNode(int spot,
            T new_key, int[] new_value, BTreeNode<T> new_child,
            ref T up_key, ref int[] up_value, ref BTreeNode<T> up_child)
        {
            // See if we have room.
            if (NumKeysUsed < KEYS_PER_NODE)
            {
                // There is room here.
                AddItemInNodeWithRoom(spot, new_key, new_value, new_child);
                up_key = default(T);
                up_value = null;
                up_child = null;
            } else {
                // There is no room here.
                SplitNode(spot, new_key, new_value, new_child, ref up_key, ref up_value, ref up_child);
            }
        }
#if ASYNC
        // Add the new item to this node, if it fits.
        private async Task AddItemToNodeAsync(int spot,T new_key, int[] new_value, BTreeNode<T> new_child, AddedItem<T> addedItem)
        {
            // See if we have room.
            if (NumKeysUsed < KEYS_PER_NODE)
            {
                // There is room here.
                await AddItemInNodeWithRoomAsync(spot, new_key, new_value, new_child).ConfigureAwait(false);
                addedItem.up_key = default(T);
                addedItem.up_value = null;
                addedItem.up_node = null;
            }
            else
            {
                // There is no room here.
                await SplitNodeAsync(spot, new_key, new_value, new_child, addedItem).ConfigureAwait(false);
            }
        }
#endif
        // Add a new item in a leaf node that has room.
        private void AddItemInNodeWithRoom(int spot, T new_key, int[] new_value, BTreeNode<T> new_child)
        {
            CheckChildren();
            // Move the existing items over to make an empty spot at Children[after].
            Array.Copy(this.Keys, spot, this.Keys, spot + 1, NumKeysUsed - spot);
            Array.Copy(this.Values, spot, this.Values, spot + 1, NumKeysUsed - spot);
            Array.Copy(this.Children, spot + 1, this.Children, spot + 2, NumKeysUsed - spot);

            // Insert the new item.
            this.Keys[spot] = new_key;
            this.Values[spot] = new_value;
            this.Children[spot + 1] = new_child;
            NumKeysUsed++;

            this.Persist();
        }
#if ASYNC
        // Add a new item in a leaf node that has room.
        private async Task AddItemInNodeWithRoomAsync(int spot, T new_key, int[] new_value, BTreeNode<T> new_child)
        {
            // Move the existing items over to make an empty spot at Children[after].
            await CheckChildrenAsync().ConfigureAwait(false);
            Array.Copy(this.Keys, spot, this.Keys, spot + 1, NumKeysUsed - spot);
            Array.Copy(this.Values, spot, this.Values, spot + 1, NumKeysUsed - spot);
            Array.Copy(this.Children, spot + 1, this.Children, spot + 2, NumKeysUsed - spot);

            // Insert the new item.
            this.Keys[spot] = new_key;
            this.Values[spot] = new_value;
            this.Children[spot + 1] = new_child;
            NumKeysUsed++;

            await this.PersistAsync().ConfigureAwait(false);
        }
#endif
        // We don't have room. Split the node.
        private void SplitNode(int spot, T new_key, int[] new_value, BTreeNode<T> new_child, ref T up_key, ref int[] up_value, ref BTreeNode<T> up_child)
        {
            // Make arrays holding all of the keys, values, and children.
            T[] new_keys = new T[KEYS_PER_NODE + 1];
            Array.Copy(this.Keys, 0, new_keys, 0, spot);
            new_keys[spot] = new_key;
            Array.Copy(this.Keys, spot, new_keys, spot + 1, KEYS_PER_NODE - spot);

            int[][] new_values = new int[KEYS_PER_NODE + 1][];
            Array.Copy(this.Values, 0, new_values, 0, spot);
            new_values[spot] = new_value;
            Array.Copy(this.Values, spot, new_values, spot + 1, KEYS_PER_NODE - spot);

            BTreeNode<T>[] new_children = new BTreeNode<T>[CHILDREN_PER_NODE + 1];
            Array.Copy(this.Children, 0, new_children, 0, spot + 1);
            new_children[spot + 1] = new_child;
            Array.Copy(this.Children, spot + 1, new_children, spot + 2, KEYS_PER_NODE - spot);

            // Copy the first half of the items into this node.
            Array.Copy(new_keys, 0, this.Keys, 0, HALF_NUM_KEYS);
            Array.Copy(new_values, 0, this.Values, 0, HALF_NUM_KEYS);
            Array.Copy(new_children, 0, this.Children, 0, HALF_NUM_KEYS + 1);
            Array.Clear(this.Keys, HALF_NUM_KEYS, HALF_NUM_KEYS);
            Array.Clear(this.Values, HALF_NUM_KEYS, HALF_NUM_KEYS);
            Array.Clear(this.Children, HALF_NUM_KEYS + 1, HALF_NUM_KEYS);
            this.NumKeysUsed = HALF_NUM_KEYS;

            // Set the up key and value.
            up_key = new_keys[HALF_NUM_KEYS];
            up_value = new_values[HALF_NUM_KEYS];

            // Make the new node to pass up.
            up_child = new BTreeNode<T>(this.siaqodb,this.btree);
            Array.Copy(new_keys, HALF_NUM_KEYS + 1, up_child.Keys, 0, HALF_NUM_KEYS);
            Array.Copy(new_values, HALF_NUM_KEYS + 1, up_child.Values, 0, HALF_NUM_KEYS);
            Array.Copy(new_children, HALF_NUM_KEYS + 1, up_child.Children, 0, HALF_NUM_KEYS + 1);
            up_child.NumKeysUsed = HALF_NUM_KEYS;

            this.Persist();
            up_child.Persist();
            
        }
#if ASYNC
        private async Task SplitNodeAsync(int spot, T new_key, int[] new_value, BTreeNode<T> new_child, AddedItem<T> addedItem)
        {
            // Make arrays holding all of the keys, values, and children.
            T[] new_keys = new T[KEYS_PER_NODE + 1];
            Array.Copy(this.Keys, 0, new_keys, 0, spot);
            new_keys[spot] = new_key;
            Array.Copy(this.Keys, spot, new_keys, spot + 1, KEYS_PER_NODE - spot);

            int[][] new_values = new int[KEYS_PER_NODE + 1][];
            Array.Copy(this.Values, 0, new_values, 0, spot);
            new_values[spot] = new_value;
            Array.Copy(this.Values, spot, new_values, spot + 1, KEYS_PER_NODE - spot);

            BTreeNode<T>[] new_children = new BTreeNode<T>[CHILDREN_PER_NODE + 1];
            Array.Copy(this.Children, 0, new_children, 0, spot + 1);
            new_children[spot + 1] = new_child;
            Array.Copy(this.Children, spot + 1, new_children, spot + 2, KEYS_PER_NODE - spot);

            // Copy the first half of the items into this node.
            Array.Copy(new_keys, 0, this.Keys, 0, HALF_NUM_KEYS);
            Array.Copy(new_values, 0, this.Values, 0, HALF_NUM_KEYS);
            Array.Copy(new_children, 0, this.Children, 0, HALF_NUM_KEYS + 1);
            Array.Clear(this.Keys, HALF_NUM_KEYS, HALF_NUM_KEYS);
            Array.Clear(this.Values, HALF_NUM_KEYS, HALF_NUM_KEYS);
            Array.Clear(this.Children, HALF_NUM_KEYS + 1, HALF_NUM_KEYS);
            this.NumKeysUsed = HALF_NUM_KEYS;

            // Set the up key and value.
            addedItem.up_key = new_keys[HALF_NUM_KEYS];
            addedItem.up_value = new_values[HALF_NUM_KEYS];

            // Make the new node to pass up.
            addedItem.up_node = new BTreeNode<T>(this.siaqodb, this.btree);
            Array.Copy(new_keys, HALF_NUM_KEYS + 1, addedItem.up_node.Keys, 0, HALF_NUM_KEYS);
            Array.Copy(new_values, HALF_NUM_KEYS + 1, addedItem.up_node.Values, 0, HALF_NUM_KEYS);
            Array.Copy(new_children, HALF_NUM_KEYS + 1, addedItem.up_node.Children, 0, HALF_NUM_KEYS + 1);
            addedItem.up_node.NumKeysUsed = HALF_NUM_KEYS;

            await this.PersistAsync().ConfigureAwait(false);
            await addedItem.up_node.PersistAsync().ConfigureAwait(false);

        }
#endif
#endregion

        #region remove methods
        public void RemoveOID(T target_key,int oid)
        {
            int spot = 0;
            while (spot < NumKeysUsed)
            {
                // See if this key comes after the new value.
                if (this.Compare(Keys[spot], target_key) >= 0) break;
                spot++;
            }

            // If we found the item, return it.
            if ((spot < NumKeysUsed) && (this.Compare(Keys[spot], target_key) == 0))
            {
               int[] OIDs= Values[spot];
               int indexOf=Array.IndexOf<int>(OIDs,oid);
               if (indexOf != -1)
               {
                   Values[spot] = new int[OIDs.Length - 1];
                   int k = 0;
                   for (int i = 0; i < OIDs.Length; i++)
                   {
                       if (i == indexOf)
                       {
                           continue;
                       }
                       Values[spot][k] = OIDs[i];
                       k++;
                   }
                   this.Persist();
               }
               return;
            }
            CheckChildren();
            // If there's no link to follow, we're at a leaf
            // and didn't find it so return null.
            if (Children[spot] == null) return;

            // Look in the proper subtree.
            Children[spot].RemoveOID(target_key,oid);
        }
#if ASYNC
        public async Task RemoveOIDAsync(T target_key, int oid)
        {
            int spot = 0;
            while (spot < NumKeysUsed)
            {
                // See if this key comes after the new value.
                if (this.Compare(Keys[spot], target_key) >= 0) break;
                spot++;
            }

            // If we found the item, return it.
            if ((spot < NumKeysUsed) && (this.Compare(Keys[spot], target_key) == 0))
            {
                int[] OIDs = Values[spot];
                int indexOf = Array.IndexOf<int>(OIDs, oid);
                if (indexOf != -1)
                {
                    Values[spot] = new int[OIDs.Length - 1];
                    int k = 0;
                    for (int i = 0; i < OIDs.Length; i++)
                    {
                        if (i == indexOf)
                        {
                            continue;
                        }
                        Values[spot][k] = OIDs[i];
                        k++;
                    }
                    await this.PersistAsync().ConfigureAwait(false);
                }
                return;
            }

            // If there's no link to follow, we're at a leaf
            // and didn't find it so return null.
            await this.CheckChildrenAsync().ConfigureAwait(false);
            if (Children[spot] == null) return;

            // Look in the proper subtree.
            await Children[spot].RemoveOIDAsync(target_key, oid).ConfigureAwait(false);
        }
#endif
        // Remove this item.
        public void RemoveItem(T target_key)
        {
            // Find the key after the spot where this item goes.
            int spot = 0;
            while (spot < NumKeysUsed)
            {
                // See if this key comes after the new value.
                if (this.Compare(Keys[spot], target_key) >= 0) break;
                spot++;
            }

            // See if we found it.
            if ((spot < NumKeysUsed) && (this.Compare(Keys[spot], target_key) == 0))
            {
                // The item is here.
                // See if we are a leaf node.
                if (Children[0] == null)
                {
                    // We're a leaf node. Remove the item.
                    RemoveItemFromNode(spot);
                } else {
                    // We're not a leaf node.
                    // Find the rightmost item to the item's left.
                    T rightmost_key = default(T);
                    int[] rightmost_value = null;
                    Children[spot].SwapRightmost(target_key, ref rightmost_key, ref rightmost_value);

                    // Save the rightmost values.
                    Keys[spot] = rightmost_key;
                    Values[spot] = rightmost_value;

                    // Delete the rightmost item.
                    Children[spot].RemoveItem(target_key);
                }
            } else {
                // The item is not here.
                // See if we are a leaf node.
                if (Children[0] == null)
                {
                    // We didn't find the target key.
                    throw new Exception("Delete error. Cannot find item with key value '" + target_key + "'");
                }

                // Search deeper.
                Children[spot].RemoveItem(target_key);
            }

            // See if we are a leaf node.
            if (Children[0] != null)
            {
                // We're not a leaf.
                // See if our child got too small.
                if (Children[spot].NumKeysUsed < HALF_NUM_KEYS)
                {
                    // The child is too small.
                    // Try to redistribute.
                    if ((spot > 0) && (Children[spot - 1].NumKeysUsed > HALF_NUM_KEYS))
                    {
                        // Redistribute with the left sibling.
                        RebalanceSiblings(Children[spot - 1], Children[spot], ref Keys[spot - 1], ref Values[spot - 1]);
                    } else if ((spot < HALF_NUM_KEYS - 1) && (Children[spot + 1].NumKeysUsed > HALF_NUM_KEYS))
                    {
                        // Redistribute with the right sibling.
                        RebalanceSiblings(Children[spot], Children[spot + 1], ref Keys[spot], ref Values[spot]);
                    } else {
                        // We cannot redistribute. Merge.
                        if (spot > 0)
                        {
                            // Merge with the left sibling.
                            MergeSiblings(spot - 1, spot, Keys[spot - 1], Values[spot - 1]);
                        } else {
                            // Merge with the right sibling.
                            MergeSiblings(spot, spot + 1, Keys[spot], Values[spot]);
                        }
                    }
                }
            }
        }

        // Remove the item from the given spot.
        private void RemoveItemFromNode(int spot)
        {
            Array.Copy(this.Keys, spot + 1, this.Keys, spot, NumKeysUsed - spot - 1);
            Array.Copy(this.Values, spot + 1, this.Values, spot, NumKeysUsed - spot - 1);
            Array.Copy(this.Children, spot + 2, this.Children, spot + 1, NumKeysUsed - spot - 1);
            this.NumKeysUsed--;
            this.Keys[this.NumKeysUsed] = default(T);
            this.Values[this.NumKeysUsed] = null;
            this.Children[this.NumKeysUsed + 1] = null;
        }

        // Find the rightmost item in this node's subtree.
        private void SwapRightmost(T target_key, ref T rightmost_key, ref int[] rightmost_value)
        {
            // See if we are a leaf node.
            if (Children[0] == null)
            {
                // We're a leaf.
                // Get our rightmost item's data for return.
                rightmost_key = Keys[NumKeysUsed - 1];
                rightmost_value = Values[NumKeysUsed - 1];

                // Save the target key in this item.
                Keys[NumKeysUsed - 1] = target_key;
            } else {
                // We're not a leaf. Follow our rightmost link.
                Children[NumKeysUsed].SwapRightmost(target_key, ref rightmost_key, ref rightmost_value);
            }
        }

        // Move items between the two nodes to balance them.
        private static void RebalanceSiblings(BTreeNode<T> left_node, BTreeNode<T> right_node, ref T middle_key, ref int[] middle_value)
        {
            // Make arrays holding all of the keys, values, and children.
            int num_keys = left_node.NumKeysUsed + right_node.NumKeysUsed + 1;
            int mid = left_node.NumKeysUsed;
            T[] new_keys = new T[num_keys];
            Array.Copy(left_node.Keys, 0, new_keys, 0, left_node.NumKeysUsed);
            new_keys[mid] = middle_key;
            Array.Copy(right_node.Keys, 0, new_keys, mid + 1, right_node.NumKeysUsed);

            int[][] new_values = new int[num_keys][];
            Array.Copy(left_node.Values, 0, new_values, 0, left_node.NumKeysUsed);
            new_values[mid] = middle_value;
            Array.Copy(right_node.Values, 0, new_values, mid + 1, right_node.NumKeysUsed);

            BTreeNode<T>[] new_children = new BTreeNode<T>[num_keys + 1];
            Array.Copy(left_node.Children, 0, new_children, 0, left_node.NumKeysUsed + 1);
            Array.Copy(right_node.Children, 0, new_children, mid + 1, right_node.NumKeysUsed + 1);

            // Copy the first half of the items into the left node.
            int num_left = (int)((num_keys - 1) / 2);
            Array.Copy(new_keys, 0, left_node.Keys, 0, num_left);
            Array.Copy(new_values, 0, left_node.Values, 0, num_left);
            Array.Copy(new_children, 0, left_node.Children, 0, num_left + 1);
            Array.Clear(left_node.Keys, num_left, KEYS_PER_NODE - num_left);
            Array.Clear(left_node.Values, num_left, KEYS_PER_NODE - num_left);
            Array.Clear(left_node.Children, num_left + 1, KEYS_PER_NODE - num_left);
            left_node.NumKeysUsed = num_left;

            // Set the up key and value.
            middle_key = new_keys[num_left];
            middle_value = new_values[num_left];

            // Copy the remaining items into the right node.
            int num_right = num_keys - 1 - num_left;
            Array.Copy(new_keys, num_left + 1, right_node.Keys, 0, num_right);
            Array.Copy(new_values, num_left + 1, right_node.Values, 0, num_right);
            Array.Copy(new_children, num_left + 1, right_node.Children, 0, num_right + 1);
            right_node.NumKeysUsed = num_right;
        }

        // Merge these siblings.
        private void MergeSiblings(int left_spot, int right_spot, T middle_key, int[]  middle_value)
        {
            // Join the two children.
            int mid = Children[left_spot].NumKeysUsed;
            Children[left_spot].Keys[mid] = middle_key;
            Children[left_spot].Values[mid] = middle_value;

            Array.Copy(Children[right_spot].Keys, 0, Children[left_spot].Keys, mid + 1, Children[right_spot].NumKeysUsed);
            Array.Copy(Children[right_spot].Values, 0, Children[left_spot].Values, mid + 1, Children[right_spot].NumKeysUsed);
            Array.Copy(Children[right_spot].Children, 0, Children[left_spot].Children, mid + 1, Children[right_spot].NumKeysUsed + 1);
            Children[left_spot].NumKeysUsed += Children[right_spot].NumKeysUsed + 1;

            // Remove the right child's entry from our Children array.
            Array.Copy(this.Keys, left_spot + 1, this.Keys, left_spot, this.NumKeysUsed - left_spot - 1);
            Array.Copy(this.Values, left_spot + 1, this.Values, left_spot, this.NumKeysUsed - left_spot - 1);
            Array.Copy(this.Children, right_spot + 1, this.Children, right_spot, this.NumKeysUsed - right_spot);
            this.NumKeysUsed--;
            this.Keys[this.NumKeysUsed] = default(T);
            this.Values[this.NumKeysUsed] = null;
            this.Children[this.NumKeysUsed + 1] = null;
        }
        #endregion

        #region dump methods
        public void Dump(int indent)
        {
            CheckChildren();
            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {
                    Children[i].Dump(indent + 10);
                }
                Debug.WriteLine(new string(' ', indent) +Keys[i].ToString()+"="+ ArrayToString(Values[i]));
            }
            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    Children[NumKeysUsed].Dump(indent + 10);
                }
            }
        }
#if ASYNC
        public async Task DumpAsync(int indent)
        {
            await this.CheckChildrenAsync().ConfigureAwait(false);
            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {
                    await Children[i].DumpAsync(indent + 10).ConfigureAwait(false);
                }
                //Console.WriteLine(new string(' ', indent) +Keys[i].ToString()+"="+ ArrayToString(Values[i]));
            }
            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    await Children[NumKeysUsed].DumpAsync(indent + 10).ConfigureAwait(false);
                }
            }
        }
#endif
        public List<T> DumpKeys()
        {
            List<T> dumpedKeys = new List<T>();
            CheckChildren();
            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {
                    dumpedKeys.AddRange(Children[i].DumpKeys());
                }
                dumpedKeys.Add(Keys[i]);
            }
            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    dumpedKeys.AddRange(Children[NumKeysUsed].DumpKeys());
                }
            }
            return dumpedKeys;
        }
#if ASYNC
        public async Task<List<T>> DumpKeysAsync()
        {
            List<T> dumpedKeys = new List<T>();
            await this.CheckChildrenAsync().ConfigureAwait(false);
            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {
                    dumpedKeys.AddRange(await Children[i].DumpKeysAsync().ConfigureAwait(false));
                }
                dumpedKeys.Add(Keys[i]);
            }
            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    dumpedKeys.AddRange(await Children[NumKeysUsed].DumpKeysAsync().ConfigureAwait(false));
                }
            }
            return dumpedKeys;
        }
#endif
        public List<BTreeNode<T>> DumpNodes()
        {
            List<BTreeNode<T>> dumpedOids = new List<BTreeNode<T>>();
            CheckChildren();
            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {
                    dumpedOids.AddRange(Children[i].DumpNodes());
                }
                
            }
            dumpedOids.Add(this);
            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    dumpedOids.AddRange(Children[NumKeysUsed].DumpNodes());
                }
            }
            return dumpedOids;
        }
#if ASYNC
        public async Task<List<BTreeNode<T>>> DumpNodesAsync()
        {
            List<BTreeNode<T>> dumpedOids = new List<BTreeNode<T>>();
            await CheckChildrenAsync().ConfigureAwait(false);
            for (int i = 0; i < NumKeysUsed; i++)
            {
                if (Children[i] != null)
                {
                    dumpedOids.AddRange(await Children[i].DumpNodesAsync().ConfigureAwait(false));
                }

            }
            dumpedOids.Add(this);
            if (NumKeysUsed > 0)
            {
                if (Children[NumKeysUsed] != null)
                {
                    dumpedOids.AddRange(await Children[NumKeysUsed].DumpNodesAsync().ConfigureAwait(false));
                }
            }
            return dumpedOids;
        }
#endif
        private string ArrayToString(int[] arr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in arr)
            {
                sb.Append("|" + i.ToString());
            }
            return sb.ToString();
        }
        #endregion

       

        internal int NrNodes()
        {
            int nrNodes = 0;
            CheckChildren();
            for (int i = 0; i < CHILDREN_PER_NODE; i++)
            {

                if (Children[i] != null)
                {
                    nrNodes++;
                    nrNodes += Children[i].NrNodes();
                }

            }
            return nrNodes;
        }
#if ASYNC
        internal async Task<int> NrNodesAsync()
        {
            int nrNodes = 0;
            await this.CheckChildrenAsync().ConfigureAwait(false);
            for (int i = 0; i < CHILDREN_PER_NODE; i++)
            {

                if (Children[i] != null)
                {
                    nrNodes++;
                    nrNodes += await Children[i].NrNodesAsync().ConfigureAwait(false);
                }

            }
            return nrNodes;
        }
#endif
        public void Persist()
        {
            if (this.OID == 0)
            {
                this.OID = siaqodb.AllocateNewOID<BTreeNode<T>>();
            }
            CheckChildren();
            for (int i = 0; i < this.Children.Length; i++)
            {
                if (this.Children[i] != null)
                {

                    this._childrenOIDs[i] = this.Children[i].OID;
                }
            }

#if UNITY3D
            if (!btree.dirtyNodes.Contains(this))
            {
                btree.dirtyNodes.Add(this);
            }
#else
            btree.dirtyNodes[this] = this.OID;
#endif

        }
#if ASYNC
        public async Task PersistAsync()
        {
            if (this.OID == 0)
            {
                this.OID = await siaqodb.AllocateNewOIDAsync<BTreeNode<T>>().ConfigureAwait(false);
            }
            await this.CheckChildrenAsync().ConfigureAwait(false);
            for (int i = 0; i < this.Children.Length; i++)
            {
                if (this.Children[i] != null)
                {

                    this._childrenOIDs[i] = this.Children[i].OID;
                }
            }

            #if UNITY3D
            if (!btree.dirtyNodes.Contains(this))
            {
                btree.dirtyNodes.Add(this);
            }
          
			#else
            btree.dirtyNodes[this] = this.OID;
			#endif
        }
#endif
#if SILVERLIGHT
         [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
         [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
#endif
    }
#if ASYNC
    class StopIndicator
    {
        public bool Stop { get; set; }
    }
#endif
   
}
