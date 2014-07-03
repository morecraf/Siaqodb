using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.Indexes;
using Sqo.Transactions;


namespace Sqo
{
    public class DotissiDB
    {
        private Siaqodb siaqodb;
        TagsIndexManager indexManager;
        private readonly object _locker = new object();
        private readonly AsyncLock _lockerAsync = new AsyncLock();
#if !WinRT
        public DotissiDB(string bucketPath)
        {
            SiaqodbConfigurator.EncryptedDatabase = true;
            this.siaqodb = new Siaqodb(bucketPath);
            indexManager = new TagsIndexManager(this.siaqodb);
            siaqodb.SetTagsIndexManager(indexManager);
           
        }

       
#endif
        private void CreateDirtyEntity(object obj, DirtyOperation dop)
        {
            this.CreateDirtyEntity(obj, dop, null);
        }
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop)
        {
            await this.CreateDirtyEntityAsync(obj, dop, null);
        }
        private void CreateDirtyEntity(object obj, DirtyOperation dop, ITransaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = this.siaqodb.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            if (transaction != null)
            {
                this.siaqodb.StoreObject(dirtyEntity, transaction);
            }
            else
            {
                this.siaqodb.StoreObject(dirtyEntity);
            }
        }
        private async Task CreateDirtyEntityAsync(object obj, DirtyOperation dop, ITransaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = this.siaqodb.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            if (transaction != null)
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity, transaction);
            }
            else
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity);
            }
        }
        private void CreateTombstoneDirtyEntity(object obj, int oid)
        {
            this.CreateTombstoneDirtyEntity(obj, oid, null);
        }
        private async Task CreateTombstoneDirtyEntityAsync(object obj, int oid)
        {
            await this.CreateTombstoneDirtyEntityAsync(obj, oid, null);
        }
        private void CreateTombstoneDirtyEntity(object obj, int oid, Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
            if (transaction != null)
            {
                this.siaqodb.StoreObject(dirtyEntity, transaction);
            }
            else
            {
                this.siaqodb.StoreObject(dirtyEntity);
            }
        }
        private async Task CreateTombstoneDirtyEntityAsync(object obj, int oid, Transaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
            if (transaction != null)
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity, transaction);
            }
            else
            {
                await this.siaqodb.StoreObjectAsync(dirtyEntity);
            }
        }
        public void Store(string key, object obj)
        {
            this.Store(key, obj, null,null);
        }


        public void Store(string key, object obj, object tags = null)
        {
            
            Dictionary<string, int> intTags = new Dictionary<string, int>();
            Dictionary<string, string> strTags = new Dictionary<string, string>();
            object o = tags;
            Type tagsType = o.GetType();

            /*PropertyInfo[] pi = tagsType.GetProperties();
            foreach (PropertyInfo p in pi)
            {
                if (p.PropertyType == typeof(string) )
                    strTags.Add(p.Name, p.GetValue(o).ToString());
                else if (p.PropertyType == typeof(int))
                    intTags.Add(p.Name,Convert.ToInt32( p.GetValue(o)));
            }
           
            this.Store(key, obj, strTags, intTags);*/
        }

        public void Store(string key, object obj, Dictionary<string, string> strTags, Dictionary<string, int> intTags)
        {
            DotissiObject dotissiObject = new DotissiObject();
            dotissiObject.Key = key;
            dotissiObject.IsDirty = true;
            dotissiObject.SetValue(obj);
            if (strTags != null)
            {
                dotissiObject.StrTags = strTags;
            }
            if (intTags != null)
            {
                dotissiObject.IntTags = intTags;
            }
            this.Store(dotissiObject);
        }
        public void Store(DotissiObject obj)
        {
            lock (this._locker)
            {
                if (obj.IsDirty)
                {
                    int oID = this.siaqodb.GetOID(obj);
                    if (oID == 0)
                    {
                        this.siaqodb.GetOIDForAMSByField(obj, "key");
                    }
                    oID = this.siaqodb.GetOID(obj);
                    DirtyOperation dop = (oID == 0) ? DirtyOperation.Inserted : DirtyOperation.Updated;
                    Dictionary<string, int> oldInts=null;
                    Dictionary<string, string> oldStrs = null;
                    if (dop == DirtyOperation.Updated)
                    {
                        oldInts = indexManager.PrepareUpdateIntIndexes(oID);
                        oldStrs = indexManager.PrepareUpdateStrIndexes(oID);
                    }
                    this.siaqodb.StoreObject(obj);
                    this.CreateDirtyEntity(obj, dop);
                    indexManager.UpdateIndexes(obj.OID, oldInts, obj.IntTags);
                    indexManager.UpdateIndexes(obj.OID, oldStrs, obj.StrTags);
                }
                else
                {
                    this.siaqodb.StoreObject(obj);
                }
                
            }
        }
        public IList<T> LoadAll<T>()
        {
            List<T> list = new List<T>();
            IList<DotissiObject> list2 = this.siaqodb.LoadAll<DotissiObject>();
            foreach (DotissiObject current in list2)
            {
                list.Add(current.GetValue<T>());
            }
            return list;
        }
        public IList<DotissiObject> LoadAll()
        {
            return this.siaqodb.LoadAll<DotissiObject>();
        }
        public T Load<T>(string key)
        {
            return default(T);
        }
        public DotissiObject Load(string key)
        {
            return null;
        }
        public ISqoQuery<T> Cast<T>()
        {
            return this.Query<T>();
        }
        public ISqoQuery<T> Query<T>()
        {
            return new SqoQuery<T>(this.siaqodb);
        }
    }
    public class DotissiObject
    {
        [Index]
        private string key;
        public int OID { get; set; }
        public bool ShouldSerializeOID()
        {
            return false;
        }
        public bool ShouldSerializeIsDirty()
        {
            return false;
        }
        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }
        public byte[] Document { get; set; }
        public byte[] Version { get; set; }
        private Dictionary<string, int> intTags;
        public Dictionary<string, int> IntTags { get { return intTags; } set { intTags = value; } }
        private Dictionary<string, string> strTags;
        public Dictionary<string, string> StrTags { get { return strTags; } set { strTags = value; } }

        public bool IsDirty { get; set; }
        
        internal DotissiObject(string key, byte[] document)
        {
            this.Key = key;
            this.Document = document;
        }
        public DotissiObject()
        {
        }
        public T GetValue<T>()
        {
            return (T)((object)this.GetValue(typeof(T)));
        }
        public object GetValue(Type type)
        {
            if (SiaqodbConfigurator.DocumentSerializer == null)
            {
                throw new SiaqodbException("Document serializer is not set, use SiaqodbConfigurator.SetDocumentSerializer method to set it");
            }
            return SiaqodbConfigurator.DocumentSerializer.Deserialize(type, this.Document);
        }
        public void SetValue<T>(T obj)
        {
            this.SetValue((object)obj);
        }
        public void SetValue(object obj)
        {
            if (SiaqodbConfigurator.DocumentSerializer == null)
            {
                throw new SiaqodbException("Document serializer is not set, use SiaqodbConfigurator.SetDocumentSerializer method to set it");
            }
            this.Document = SiaqodbConfigurator.DocumentSerializer.Serialize(obj);
            this.IsDirty = true;
        }
    }
    internal enum DirtyOperation
    {
        Inserted = 1,
        Updated,
        Deleted
    }
    internal class DirtyEntity
    {
        public int EntityOID;
        public DirtyOperation DirtyOp;
        public int OID
        {
            get;
            set;
        }
        public object GetValue(FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
    }
}
