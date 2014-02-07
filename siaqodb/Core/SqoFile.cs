using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#if ASYNC
using System.Threading.Tasks;
#endif


#if SILVERLIGHT
using System.IO.IsolatedStorage;
#endif

namespace Sqo.Core
{
    internal class SqoFile : ISqoFile
    {
        protected FileStream file;


        public virtual void Write(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            file.Write(buf, 0, buf.Length);
        }
        public virtual void Write(byte[] buf)
        {
            file.Write(buf, 0, buf.Length);
        }

        public virtual int Read(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            return file.Read(buf, 0, buf.Length);
        }
#if ASYNC
        public async Task WriteAsync(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            await file.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }
        public async Task WriteAsync(byte[] buf)
        {
            await file.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }
        public async Task<int> ReadAsync(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            return await file.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }
        public async Task FlushAsync()
        {
            if (!this.IsClosed)
            {
                await file.FlushAsync();

            }

        }
#endif
        public bool IsClosed
        {
            get { return this.isClosed; }
        }
        public virtual void Flush()
        {
            if (!this.IsClosed)
            {
                file.Flush();

            }

        }


        bool isClosed = false;
        public virtual void Close()
        {
            isClosed = true;
            file.Close();


        }

        public long Length
        {
            get { return file.Length; }
            set { file.SetLength(value); }
        }


        internal SqoFile(String filePath, bool readOnly)
        {

            file = new FileStream(filePath, FileMode.OpenOrCreate,
                                  readOnly ? FileAccess.Read : FileAccess.ReadWrite);


        }


    }
    /*public class PaginatedFile : ISqoFile
    {
        public const int PageCacheSize = 500;//2MB

        LruCache<long, Page> pagesQueue = new LruCache<long, Page>(PageCacheSize);
        protected FileStream file;
        public PaginatedFile(String filePath)
        {
            file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            pagesQueue.ItemRemoved += pagesQueue_ItemRemoved;
        }
        public void Write(long pos, byte[] buf)
        {
            pos = pos + Page.MetadataPageSize * ((pos / (Page.PageSize - Page.MetadataPageSize)) + 1);//inject metadatapage bytes
            Page firstPage = GetPageByPosition(pos);
            long positionInPage = pos % Page.PageSize;

            if (positionInPage + buf.Length <= Page.PageSize)//fit inside page
            {
                Array.Copy(buf, 0, firstPage.PageData, positionInPage, buf.Length);
                firstPage.IsDirty = true;
            }
            else//not fit in one page->split
            {
                Array.Copy(buf, 0, firstPage.PageData, positionInPage, Page.PageSize - positionInPage);
                firstPage.IsDirty = true;
                int remainLength = buf.Length - (Page.PageSize - (int)positionInPage);

                Page prevPage = firstPage;
                while (remainLength > 0)
                {
                    Page current = GetPageByPosition(prevPage.GetPosition() + Page.PageSize);
                    int positionInBuf = buf.Length - remainLength;

                    current.IsDirty = true;
                    if (remainLength <= (Page.PageSize - Page.MetadataPageSize))
                    {
                        Array.Copy(buf, (long)positionInBuf, current.PageData, Page.MetadataPageSize, remainLength);
                        break;
                    }
                    else//put on full page
                    {
                        Array.Copy(buf, (long)positionInBuf, current.PageData, Page.MetadataPageSize, Page.PageSize - Page.MetadataPageSize);
                        remainLength = remainLength - (Page.PageSize - Page.MetadataPageSize);
                    }

                    prevPage = current;
                }
            }
        }
        private long GetPositionInPageByRealPos(long pos, Page page)
        {
            long retPos = pos - ((pos / Page.PageSize) * Page.PageSize);
            return retPos;
        }
        private Page GetPageByPosition(long pos)
        {

            long pageNumber = (pos / Page.PageSize) + 1;
            Page pgFromQ;
            bool found = pagesQueue.TryGetValue(pageNumber, out pgFromQ);
            if (pgFromQ != null)
            {
                return pgFromQ;
            }
            else
            {
                Page pgFromD = new Page();
                pgFromD.PageNumber = pageNumber;
                ReadPage(pgFromD);
                pagesQueue.Add(pageNumber, pgFromD);
                return pgFromD;


            }
        }
        void pagesQueue_ItemRemoved(object sender, RemovedNodeEvenArgs<Page> e)
        {
            if (e.RemovedPage.IsDirty)
            {
                WritePage(e.RemovedPage);
            }
        }
        private void ReadPage(Page page)
        {
            if (page.GetPosition() + Page.PageSize > file.Length)
            {
                return;
            }
            file.Seek(page.GetPosition(), SeekOrigin.Begin);
            file.Read(page.PageData, 0, page.PageData.Length);

        }
        private void WritePage(Page page)
        {
            file.Seek(page.GetPosition(), SeekOrigin.Begin);
            file.Write(page.PageData, 0, page.PageData.Length);
        }
        public void Write(byte[] buf)
        {
            throw new NotImplementedException();
        }

        public int Read(long pos, byte[] buf)
        {
            pos = pos + Page.MetadataPageSize * ((pos / (Page.PageSize - Page.MetadataPageSize)) + 1);//inject metadatapage bytes
            Page firstPage = GetPageByPosition(pos);
            long positionInPage = pos % Page.PageSize;

            if (positionInPage + buf.Length <= Page.PageSize)//fit inside page
            {
                Array.Copy(firstPage.PageData, positionInPage, buf, 0, buf.Length);
            }
            else//not fit in one page->splited
            {
                Array.Copy(firstPage.PageData, positionInPage, buf, 0, Page.PageSize - positionInPage);

                int remainLength = buf.Length - (Page.PageSize - (int)positionInPage);

                Page prevPage = firstPage;
                while (remainLength > 0)
                {
                    Page current = GetPageByPosition(prevPage.GetPosition() + Page.PageSize);
                    int positionInBuf = buf.Length - remainLength;
                    if (remainLength <= (Page.PageSize - Page.MetadataPageSize))
                    {
                        Array.Copy(current.PageData, Page.MetadataPageSize, buf, (long)positionInBuf, remainLength);
                        break;
                    }
                    else//get from full page
                    {
                        Array.Copy(current.PageData, Page.MetadataPageSize, buf, (long)positionInBuf, Page.PageSize - Page.MetadataPageSize);
                        remainLength = remainLength - (Page.PageSize - Page.MetadataPageSize);
                    }

                    prevPage = current;
                }
            }
            return buf.Length;
        }

        public void Flush()
        {
            if (!IsClosed)
            {
                Page[] pages = pagesQueue.Dump();
                Array.Sort(pages);
                foreach (Page p in pages)
                {
                    if (p.IsDirty)
                    {
                        WritePage(p);
                        p.IsDirty = false;
                    }
                }
                file.Flush();
            }
        }
        bool isClosed = false;
        public bool IsClosed
        {
            get { return this.isClosed; }
        }
        public void Close()
        {
            isClosed = true;
            file.Close();
        }

        public long Length
        {
            get { return file.Length; }
            set { file.SetLength(value); }
        }

    }
    class Page:IComparable<Page>
    {
        public const int PageSize = 4 * 1024;
        public const int MetadataPageSize = 9;//checksum+pagenumber
        public byte[] PageData = new byte[PageSize];
        public long PageNumber;
        public bool IsDirty;
        public long GetPosition()
        {
            return PageNumber * Page.PageSize - Page.PageSize;
        }
        public int CompareTo(Page other)
        {
            return this.PageNumber.CompareTo(other.PageNumber);
        }
    }
    //http://blog.softwx.net/2012/06/exploring-linkedlists-via-lru-caches-2.html
    /// <summary>
    /// Simple implementation of an LRU Cache (Least Recently Used). This uses a
    /// private linked list implementation for it's use-ordered list.
    /// </summary>
    /// <remarks> This class is threadsafe, with this caveat: A common use case is to try and get
    /// the desired item, and if it's not in the cache, to obtain it externally, and then
    /// add it to the cache. It's possible that between the two calls, another thread does
    /// the same thing, for the same key, and adds its value before the other thread. In
    /// this case, the second add will simply overwrite the value placed first.
    ///
    /// LruCache is conservative with regard to creating linked list node object.
    /// Once the cache has filled, it re-uses the oldest node object to hold the incoming
    /// newest object. So over its entire lifetime, it will not create more than Capacity
    /// node objects.
    /// 
    /// Most methods and properties of LruCacheSlim have O(1) time complexity.</remarks>
    /// <typeparam name="TKey">Type of keys used to identify cached items.</typeparam>
    /// <typeparam name="TValue">Type of items being cached.</typeparam>
    internal sealed class LruCache<TKey, TValue> 
    {
        private object lockObj = new object();
        private int capacity;
        private Dictionary<TKey, Entry> cacheMap;
        private LruNode oldestNode; // oldestNode (least recently used) is the tail of a circular linked list of cache nodes.
        public event EventHandler<RemovedNodeEvenArgs<TValue>> ItemRemoved;
     
        private LruCache() { }

        /// <summary>
        /// Create a new instance of LruCache with the specified capacity.
        /// </summary>
        /// <param name="capacity">Maximum capacity of the cache.</param>
        public LruCache(int capacity)
        {
            this.capacity = capacity;
            this.cacheMap = new Dictionary<TKey, Entry>(capacity);
        }

        /// <summary>Gets the maximum capacity of the LruCache.</summary>
        public int Capacity { get { return this.capacity; } }

        /// <summary>Gets the count of items contained in the LruCache.</summary>
        public int Count { get { return this.cacheMap.Count; } }

        /// <summary>
        /// Clear the contents of the LruCache.
        /// </summary>
        /// <remarks>This method is an O(n) operation.</remarks>
        public void Clear()
        {
            lock (lockObj)
            {
                this.cacheMap.Clear();
                // break all links between nodes (may help garbage collection)
                var node = this.oldestNode;
                this.oldestNode = null;
                while (node != null)
                {
                    var temp = node;
                    node = node.Prev;
                    temp.Clear();
                }
            }
        }

        /// <summary>
        /// Determines whether the LruCache containse the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the LruCache.</param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            lock (lockObj)
            {
                return this.cacheMap.ContainsKey(key);
            }
        }

        /// <summary>
        /// Attempts to get the cached value associated with the specified key.
        /// </summary>
        /// <param name="key">The key used to identify the cached item.</param>
        /// <param name="value">When this method returns, contains the value
        /// associated with the specified key, otherwise the default value if the key
        /// is not in the cache.</param>
        /// <returns>True if the value associated with the specified key was found,
        /// otherwise false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (lockObj)
            {
                Entry entry;
                if (this.cacheMap.TryGetValue(key, out entry))
                {
                    Touch(entry.node);
                    value = entry.value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Adds the specified value to the LruCache, associated with the
        /// specified key, or replaces the value if the key already exists. 
        /// </summary>
        /// <param name="key">The key used to identify the cached item.</param>
        /// <param name="value">The value to be stored in the cache.</param>
        public void Add(TKey key, TValue value)
        {
            lock (lockObj)
            {
                Entry entry;
                if (!this.cacheMap.TryGetValue(key, out entry))
                {
                    LruNode node;
                    if (this.cacheMap.Count == capacity)
                    {
                        RemovedNodeEvenArgs<TValue> args = new RemovedNodeEvenArgs<TValue>();
                        args.RemovedPage = this.cacheMap[oldestNode.Key].value;
                        OnItemRemoved(args);
                        // cache full, so re-use the oldest node
                        node = this.oldestNode;
                        this.cacheMap.Remove(node.Key);
                        node.Key = key;
                        // oldest becomes the newest
                        this.oldestNode = node.Prev;
                    }
                    else
                    {
                        // room to create and insert a new node
                        node = new LruNode(key);
                        if (this.oldestNode != null)
                        {
                            // in a circular list, newest is next after oldest
                            node.InsertAfter(this.oldestNode);
                        }
                        else
                        {
                            // set up the first node in the linked list
                            this.oldestNode = node;
                        }
                    }
                    // map the key to the node
                    entry.node = node;
                    entry.value = value;
                    this.cacheMap.Add(key, entry);
                }
                else
                {
                    // key exists, replace value with that given
                    entry.value = value;
                    this.cacheMap[key] = entry;
                    Touch(entry.node);
                }
            }
        }
        protected void OnItemRemoved(RemovedNodeEvenArgs<TValue> args)
        {
            if (this.ItemRemoved != null)
            {
                this.ItemRemoved(this, args);
            }
        }
        /// <summary>
        /// Make the cache item with the specified key be the most recently used item.
        /// Must be called from code that is in a lock block.
        /// </summary>
        /// <param name="node">The cached item LruNode.</param>
        private void Touch(LruNode node)
        {
            if (node != this.oldestNode)
            {
                node.MoveAfter(this.oldestNode);
            }
            else
            {
                // since node is oldest, we make it newest by just saying previous node 
                // the oldest because our linked list is circular
                this.oldestNode = node.Prev;
            }
        }
        public TValue[] Dump()
        {
            TValue[] items = new TValue[cacheMap.Count];
            int i = 0;
            foreach (TKey key in this.cacheMap.Keys)
            {
                items[i] = this.cacheMap[key].value;
                i++;
            }
            return items;
        }
        private struct Entry
        {
            public LruNode node;
            public TValue value;
            public Entry(LruNode node, TValue value)
            {
                this.node = node;
                this.value = value;
            }
        }

        private class LruNode
        {
            /// <summary>Key that identifies a cache entry.</summary>
            public TKey Key { get; set; }
            public LruNode Prev { get; private set; }
            public LruNode Next { get; private set; }

            public LruNode(TKey key)
            {
                Key = key;
                Prev = Next = this;
            }

            public void MoveAfter(LruNode node)
            {
                if (node.Next != this)
                {
                    this.Next.Prev = this.Prev;
                    this.Prev.Next = this.Next;
                    InsertAfter(node);
                }
            }

            public void InsertAfter(LruNode node)
            {
                this.Prev = node;
                this.Next = node.Next;
                node.Next = this.Next.Prev = this;
            }

            public void Clear()
            {
                Key = default(TKey);
                Prev = Next = null;
            }

            public override string ToString()
            {
                return "LruNode<" + Key.ToString() + ">";
            }
        }
    }
    internal class RemovedNodeEvenArgs<TValue> : EventArgs
    {
        public TValue RemovedPage { get; set; }
    }*/
}
