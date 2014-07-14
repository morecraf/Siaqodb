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
using System.Collections;


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
            this.Store(key, obj, null);
        }


        public void Store(string key, object obj, object tags = null)
        {

            Dictionary<string, object> tags_Dict = null ;
           if (tags != null)
           {
               tags_Dict = new Dictionary<string, object>();
               object o = tags;
               Type tagsType = o.GetType();

               PropertyInfo[] pi = tagsType.GetProperties();
               foreach (PropertyInfo p in pi)
               {
                   tags_Dict.Add(p.Name, p.GetValue(o));
               }
           }

            this.Store(key, obj, tags_Dict);
        }

        public void Store(string key, object obj, Dictionary<string,object> tags)
        {
            DotissiObject dotissiObject = new DotissiObject();
            dotissiObject.Key = key;
            dotissiObject.IsDirty = true;
            dotissiObject.SetValue(obj);
            if (tags != null)
            {
                foreach (string tagName in tags.Keys)
                {
                    dotissiObject.SetTag(tagName, tags[tagName]);
                }
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
                    Dictionary<string, object> oldTags=null;
                    if (dop == DirtyOperation.Updated)
                    {
                        oldTags = indexManager.PrepareUpdateIndexes(oID);
                    }
                    this.siaqodb.StoreObject(obj);
                    this.CreateDirtyEntity(obj, dop);
                    indexManager.UpdateIndexes(obj.OID, oldTags, obj.GetAllTags());
                    
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
        private Dictionary<string, long> tags_Int;
        public Dictionary<string, long> Tags_Int{get { return tags_Int; } set { tags_Int = value; }}

        private Dictionary<string, DateTime> tags_DateTime;
        public Dictionary<string, DateTime> Tags_DateTime { get { return tags_DateTime; } set { tags_DateTime = value; } }

        private Dictionary<string, string> tags_String;
        public Dictionary<string, string> Tags_String { get { return tags_String; } set { tags_String = value; } }

        private Dictionary<string, double> tags_Double;
        public Dictionary<string, double> Tags_Double { get { return tags_Double; } set { tags_Double = value; } }

        private Dictionary<string, bool> tags_Bool;
        public Dictionary<string, bool> Tags_Bool { get { return tags_Bool; } set { tags_Bool = value; } }

        public void SetTag(string tagName, object value)
        {
            Type type = value.GetType();
            if (type == typeof(int) || type == typeof(long))
            {
                if (Tags_Int == null)
                    Tags_Int = new Dictionary<string, long>();
                Tags_Int.Add(tagName, (long)value);
            }
            else if (type == typeof(DateTime))
            {
                if (Tags_DateTime == null)
                    Tags_DateTime = new Dictionary<string, DateTime>();
                Tags_DateTime.Add(tagName, (DateTime)value);
            }

            else if (type == typeof(double) || type == typeof(float))
            {
                if (Tags_Double == null)
                    Tags_Double = new Dictionary<string, double>();
                Tags_Double.Add(tagName, (double)value);
            }
            else if (type == typeof(string))
            {
                if (Tags_String == null)
                    Tags_String = new Dictionary<string, string>();
                Tags_String.Add(tagName, (string)value);
            }
          
            else if (type == typeof(bool))
            {
                if (Tags_Bool == null)
                    Tags_Bool = new Dictionary<string, bool>();
                Tags_Bool.Add(tagName, (bool)value);
            }
            else
            {
                throw new SiaqodbException("Tag type:" + type.ToString() + " not supported.");
            }
           
        }
        internal Dictionary<string, object> GetAllTags()
        {
            Dictionary<string, object> tags = new Dictionary<string, object>();
            CopyDictionary(tags, this.Tags_Int);
            CopyDictionary(tags, this.Tags_String);
            CopyDictionary(tags, this.Tags_DateTime);
            CopyDictionary(tags, this.Tags_Double);
            CopyDictionary(tags, this.Tags_Bool);
            return tags;
        }
        private void CopyDictionary(Dictionary<string, object> tags, IDictionary dict_to_copy)
        {
            if (dict_to_copy != null)
            {

                foreach (string key in dict_to_copy.Keys)
                {
                    tags.Add(key, dict_to_copy[key]);
                }

            }
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
