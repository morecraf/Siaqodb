using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using Sqo;
using Sqo.Meta;
using Sqo.Queries;
using System.Linq.Expressions;
using Sqo.Exceptions;
using Sqo.Utilities;
using Sqo.Cache;
using System.Reflection;
using System.Threading;
using Sqo.Indexes;



namespace Sqo
{
    /// <summary>
    /// Main class of siaqodb database engine responsible for storing, retrieving ,deleting objects on database files
    /// </summary>

    [Obfuscation(Feature = "Apply to member * when event: all", Exclude = false,ApplyToMembers=true)]
    public partial class Siaqodb : Sqo.ISiaqodb
	{

        readonly object _syncRoot = new object();

        private readonly object _locker = new object();

        

        string path;
        StorageEngine storageEngine;
        Cache.CacheForManager cacheForManager;
        internal Cache.MetaCache metaCache;
        IndexManager indexManager;
        bool opened;
        internal List<object> circularRefCache = new List<object>();
        bool storeOnlyReferencesOfListItems;//used only in StoreObjectPartially to store only references of list items
        internal Transactions.TransactionManager transactionManager;
        /// <summary>
        /// Raised before an object is saved in database
        /// </summary>
#if UNITY3D
        private EventHandler<SavingEventsArgs> savingObject;
        public event EventHandler<SavingEventsArgs> SavingObject
        {
            add
            {
                lock (_syncRoot)
                {
                    savingObject += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    savingObject -= value;
                }
            }

        }
#else
        public event EventHandler<SavingEventsArgs> SavingObject;
#endif

        /// <summary>
        /// Raised after an object is saved in database
        /// </summary>
#if UNITY3D
        private EventHandler<SavedEventsArgs> savedObject;
        public event EventHandler<SavedEventsArgs> SavedObject
        {
            add
            {
                lock (_syncRoot)
                {
                    savedObject += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    savedObject -= value;
                }
            }

        }
#else
        public event EventHandler<SavedEventsArgs> SavedObject;
#endif


        /// <summary>
        /// Raised before an object is deleted from database
        /// </summary>
#if UNITY3D
        private EventHandler<DeletingEventsArgs> deletingObject;
        public event EventHandler<DeletingEventsArgs> DeletingObject
        {
            add
            {
                lock (_syncRoot)
                {
                    deletingObject += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    deletingObject -= value;
                }
            }

        }
#else
        		public event EventHandler<DeletingEventsArgs> DeletingObject;
#endif


        /// <summary>
        /// Raised after an object is deleted from database
        /// </summary>

#if UNITY3D
        private EventHandler<DeletedEventsArgs> deletedObject;
        public event EventHandler<DeletedEventsArgs> DeletedObject
        {
            add
            {
                lock (_syncRoot)
                {
                    deletedObject += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    deletedObject -= value;
                }
            }

        }
#else
        	public event EventHandler<DeletedEventsArgs> DeletedObject;
#endif


        /// <summary>
        /// Raised before an object is loaded from database
        /// </summary>
#if UNITY3D
        private EventHandler<LoadingObjectEventArgs> loadingObject;
        public event EventHandler<LoadingObjectEventArgs> LoadingObject
        {
            add
            {
                lock (_syncRoot)
                {
                    loadingObject += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    loadingObject -= value;
                }
            }

        }
#else
         public event EventHandler<LoadingObjectEventArgs> LoadingObject;
#endif


        /// <summary>
        /// Raised after object is loaded from database
        /// </summary>
#if UNITY3D
        private EventHandler<LoadedObjectEventArgs> loadedObject;
        public event EventHandler<LoadedObjectEventArgs> LoadedObject
        {
            add
            {
                lock (_syncRoot)
                {
                    loadedObject += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    loadedObject -= value;
                }
            }

        }
#else
          public event EventHandler<LoadedObjectEventArgs> LoadedObject;
#endif

 #if UNITY3D || CF || MONODROID
#else
          public event EventHandler<IndexesSaveAsyncFinishedArgs> IndexesSaveAsyncFinished;
#endif
        /// <summary>
     /// Create a new instance of Siaqodb, database is not opened yet
     /// </summary>
        public Siaqodb()
        {
            
        }
        
        
        /// <summary>
        /// Create a new instance of Siaqodb and open the database
        /// </summary>
        /// <param name="path">Physical folder name where objects are stored</param>

        public Siaqodb(string path)
        {
            
            this.Open(path);
        }
        /// <summary>
        /// Create a new instance of Siaqodb and open the database with a maximum size defined 
        /// </summary>
        /// <param name="path">Physical folder name where objects are stored</param>
        /// <param name="maxDatabaseSize">max database size in bytes (default 50 MB )</param>
        public Siaqodb(string path, long maxDatabaseSize)
        {
            this.Open(path, maxDatabaseSize, 50);
        }
        /// <summary>
        /// Create a new instance of Siaqodb and open the database with a maximum size defined and provided number of sub-databases
        /// </summary>
        /// <param name="path">Physical folder name where objects are stored</param>
        /// <param name="maxDatabaseSize">max database size in bytes (default 50 MB )</param>
        /// <param name="maxSubDatabases">max number of subdatabases(default 50); two subdatabases are created per Type; every index will use a separate subdatabase</param>
        public Siaqodb(string path, long maxDatabaseSize, int maxSubDatabases)
        {
            this.Open(path, maxDatabaseSize, maxSubDatabases);
        }
#if SL4
       /// <summary>
        ///Create a new instance of Siaqodb, open database for OOB mode
       /// </summary>
       /// <param name="folderName">database folder name</param>
       /// <param name="specialFolder">special folder name for OOB mode ex.:MyDocuments, MyPictures, etc</param>
        public Siaqodb(string folderName,Environment.SpecialFolder specialFolder)
        {
           
            this.Open(folderName,specialFolder);
        }
#endif

#if !WinRT
        internal Siaqodb(string path, bool cacheTypes)
        {
             
            
            this.opened = true;
            this.path = path;
            transactionManager = new Transactions.TransactionManager(path,20*1024*1024,200);
            storageEngine = new StorageEngine(this.path,transactionManager);
            indexManager = new IndexManager(this);
            storageEngine.indexManager = indexManager;

            storageEngine.NeedSaveComplexObject += new EventHandler<Core.ComplexObjectEventArgs>(storageEngine_NeedSaveComplexObject);


            storageEngine.LoadingObject += new EventHandler<LoadingObjectEventArgs>(storageEngine_LoadingObject);
            storageEngine.LoadedObject += new EventHandler<LoadedObjectEventArgs>(storageEngine_LoadedObject);
#if UNITY3D || CF || MONODROID
#else

            storageEngine.IndexesSaveAsyncFinished += new EventHandler<IndexesSaveAsyncFinishedArgs>(storageEngine_IndexesSaveAsyncFinished);
#endif
            this.metaCache = new MetaCache();
            storageEngine.metaCache = this.metaCache;
            
            cacheForManager = new Sqo.Cache.CacheForManager();
        }
#endif

        internal Siaqodb(string path, string managerOption)
        {
            
            this.opened = true;
            this.path = path;
            
#if  SILVERLIGHT
            storageEngine = new StorageEngine(this.path, true);
#else
            transactionManager = new Transactions.TransactionManager(path,20*1024*1024,200);
            storageEngine = new StorageEngine(this.path, transactionManager);
#endif
            
            indexManager = new IndexManager(this);
            storageEngine.indexManager = indexManager;

            storageEngine.NeedSaveComplexObject += new EventHandler<Core.ComplexObjectEventArgs>(storageEngine_NeedSaveComplexObject);

            storageEngine.LoadedObject+=new EventHandler<LoadedObjectEventArgs>(storageEngine_LoadedObject);
            storageEngine.LoadingObject+=new EventHandler<LoadingObjectEventArgs>(storageEngine_LoadingObject);

            this.metaCache = new MetaCache();
            storageEngine.metaCache = this.metaCache;
            using (var transaction = transactionManager.BeginTransaction())
            {
                storageEngine.LoadAllTypes();
                List<SqoTypeInfo> typesForIndexes = this.metaCache.DumpAllTypes();
                this.BuildIndexes(typesForIndexes);
                transaction.Commit();
            }

         
            cacheForManager = new Sqo.Cache.CacheForManager();
        }

        private void BuildIndexes(List<SqoTypeInfo> typesForIndexes)
        {
            var transaction=transactionManager.GetActiveTransaction();
            this.indexManager.BuildAllIndexes(typesForIndexes, transaction);
            
            
        }
        /// <summary>
        /// Open database 
        /// </summary>
        /// <param name="path"></param>
        public void Open(string path)
        {
            this.Open(path,50*1024*1024,50);
        }
/// <summary>
        /// Open database 
/// </summary>
/// <param name="path">database folder</param>
        /// <param name="maxDatabaseSize">max database size in bytes (default 50 MB )</param>
        /// <param name="maxSubDatabases">max number of subdatabases(default 50); two subdatabases are created per Type; every index will use a separate subdatabase</param>
        public void Open(string path, long maxDatabaseSize, int maxSubDatabases)
        {

            this.opened = true;
            this.path = path;
            this.metaCache = new MetaCache();


            transactionManager = new Transactions.TransactionManager(path, maxDatabaseSize, maxSubDatabases);
            storageEngine = new StorageEngine(this.path, transactionManager);
            indexManager = new IndexManager(this);
            storageEngine.indexManager = indexManager;

            storageEngine.metaCache = this.metaCache;
            storageEngine.NeedSaveComplexObject += new EventHandler<Core.ComplexObjectEventArgs>(storageEngine_NeedSaveComplexObject);
            storageEngine.LoadingObject += new EventHandler<LoadingObjectEventArgs>(storageEngine_LoadingObject);
            storageEngine.LoadedObject += new EventHandler<LoadedObjectEventArgs>(storageEngine_LoadedObject);
#if UNITY3D || CF || MONODROID
#else
            storageEngine.IndexesSaveAsyncFinished += new EventHandler<IndexesSaveAsyncFinishedArgs>(storageEngine_IndexesSaveAsyncFinished);
#endif
            using (var transaction = transactionManager.BeginTransaction())
            {
                storageEngine.LoadAllTypes();
                List<SqoTypeInfo> typesForIndexes = this.metaCache.DumpAllTypes();
                this.BuildIndexes(typesForIndexes);
                cacheForManager = new Sqo.Cache.CacheForManager();
                transaction.Commit();
            }

        }
        

        #region EVENTs_HND

#if UNITY3D || CF || MONODROID
#else
        void storageEngine_IndexesSaveAsyncFinished(object sender, IndexesSaveAsyncFinishedArgs e)
        {
            this.OnIndexesSaveAsyncFinished(e);
        }
#endif
        void storageEngine_LoadedObject(object sender, LoadedObjectEventArgs e)
        {
            this.OnLoadedObject(e);
        }

        void storageEngine_LoadingObject(object sender, LoadingObjectEventArgs e)
        {
            this.OnLoadingObject(e);
        }

        internal delegate int SimpleDelegate(object obj);
        internal SimpleDelegate CheckIfSavedDelegate;

        internal delegate void UpdateDelegate(object obj, int oid);
        internal UpdateDelegate UpdateMigrationOid;

        void storageEngine_NeedSaveComplexObject(object sender, Core.ComplexObjectEventArgs e)
        {
            if (e.ComplexObject == null)
            {
                return;
            }

            SqoTypeInfo ti = this.GetSqoTypeInfoToStoreObject(e.ComplexObject);
            
            if (ti != null)
            {
                /* used only on migration to avoid saving an object twice and get an inconsistent schema */
                if (CheckIfSavedDelegate != null )
                {
                    var savedOid = CheckIfSavedDelegate(e.ComplexObject);
                    if (savedOid != -1)
                    {
                        e.SavedOID = savedOid;
                        e.TID = ti.Header.TID;
                        //circularRefCache.Add(e.ComplexObject);
                        return;
                    }
                }

                int oid = -1;
                if (e.ReturnOnlyOid_TID)
                {
                    oid = metaCache.GetOIDOfObject(e.ComplexObject, ti);
                }
                else if (circularRefCache.Contains(e.ComplexObject))
                {
                    oid = metaCache.GetOIDOfObject(e.ComplexObject, ti);
                    if (UpdateMigrationOid != null)
                    {
                        UpdateMigrationOid(e.ComplexObject, oid);
                    }
                }
                else
                {
                    circularRefCache.Add(e.ComplexObject);

                    oid = metaCache.GetOIDOfObject(e.ComplexObject, ti);
                    bool inserted = oid == 0;
                    if (this.storeOnlyReferencesOfListItems && !inserted)
                    {
                        //skip save object and keep only reference
                    }
                    else
                    {
                        oid = storageEngine.SaveObject(e.ComplexObject, ti,e.Transaction);
                    }
                   
                    SavedEventsArgs saved = new SavedEventsArgs(e.ComplexObject.GetType(), e.ComplexObject);
                    saved.Inserted = inserted;
                    this.OnSavedObject(saved);
                    if (UpdateMigrationOid != null)
                    {
                        UpdateMigrationOid(e.ComplexObject, oid);
                    }
                }
                e.SavedOID = oid;
                e.TID = ti.Header.TID;
            }
        }

#if UNITY3D
        protected virtual void OnSavingObject(SavingEventsArgs e)
		{
			if (savingObject != null)
			{
				 
				savingObject(this, e);

			}
		}
		protected virtual void OnSavedObject(SavedEventsArgs e)
		{
			if (savedObject != null) {
				savedObject (this, e);
			}

		}
		protected virtual void OnDeletingObject(DeletingEventsArgs e)
		{
			if (deletingObject != null)
			{
				deletingObject(this, e);
			}
		}
		protected virtual void OnDeletedObject(DeletedEventsArgs e)
		{
			if (deletedObject != null)
			{
				deletedObject(this, e);
			}
		}
        protected virtual void OnLoadingObject(LoadingObjectEventArgs e)
        {
            if (loadingObject != null)
            {
                loadingObject(this, e);
            }
        }
        protected virtual void OnLoadedObject(LoadedObjectEventArgs e)
        {
            if (loadedObject != null)
            {
                loadedObject(this, e);
            }
        }
#else
        protected virtual void OnSavingObject(SavingEventsArgs e)
		{
            if (SavingObject != null)
            {
                SavingObject(this, e);
            }
		}
		protected virtual void OnSavedObject(SavedEventsArgs e)
		{
            if (SavedObject != null)
            {
                SavedObject(this, e);
            }
		}
		protected virtual void OnDeletingObject(DeletingEventsArgs e)
		{
			if (DeletingObject != null)
			{
				DeletingObject(this, e);
			}
		}
		protected virtual void OnDeletedObject(DeletedEventsArgs e)
		{
			if (DeletedObject != null)
			{
				DeletedObject(this, e);
			}
		}
        protected virtual void OnLoadingObject(LoadingObjectEventArgs e)
        {
            if (LoadingObject != null)
            {
                LoadingObject(this, e);
            }
        }
        protected virtual void OnLoadedObject(LoadedObjectEventArgs e)
        {
            if (LoadedObject != null)
            {
                LoadedObject(this, e);
            }
        }
        
#endif
        #if UNITY3D || CF || MONODROID
#else
        protected virtual void OnIndexesSaveAsyncFinished(IndexesSaveAsyncFinishedArgs e)
        {
            if (this.IndexesSaveAsyncFinished != null)
            {
                this.IndexesSaveAsyncFinished(this, e);
            }
        }
#endif

        #endregion
        
        /// <summary>
        /// Insert or update object; if object is loaded from database and this method is called then update will occur, if object is new created then insert will occur
        /// </summary>
        /// <param name="obj">Object to be stored</param>
		public void StoreObject(object obj)
		{
            lock (_locker)
            {
                using (var transaction = transactionManager.BeginTransaction())
                {
                    this.StoreObject(obj, transaction);
                    transaction.Commit();
                }
            }
		}

        /// <summary>
        ///  Insert or update object partially, only provided properties are saved
        /// </summary>
        /// <param name="obj">object of which properties will be stored</param>
        /// <param name="properties">properties to be stored</param>
        public void StoreObjectPartially(object obj,params string[] properties)
        {
            this.StoreObjectPartially(obj, false, properties);
        }

        /// <summary>
        /// Insert or update object partially by a Transaction, only provided properties are saved 
        /// </summary>
        /// <param name="obj">object of which properties will be stored</param>
        /// <param name="transaction">Transaction object</param>
        /// <param name="properties">properties to be stored</param>
        public void StoreObjectPartially(object obj, Transactions.ITransaction transaction, params string[] properties)
        {
            this.StoreObjectPartially(obj, false, transaction, properties);
        }

        /// <summary>
        ///  Insert or update object partially, only provided properties are saved 
        /// </summary>
        /// <param name="obj">object of which properties will be stored</param>
        /// <param name="properties">properties to be stored</param>
        ///<param name="onlyReferences">if true,it will store only references to complex objects</param>
        public void StoreObjectPartially(object obj,bool onlyReferences, params string[] properties)
        {
            lock (_locker)
            {
                this.storeOnlyReferencesOfListItems = onlyReferences;
                using (var transaction = transactionManager.BeginTransaction())
                {
                    SqoTypeInfo ti = this.GetSqoTypeInfoToStoreObject(obj);
                    if (ti != null)
                    {
                        circularRefCache.Clear();
                        circularRefCache.Add(obj);
                        storageEngine.SaveObjectPartially(obj, ti, properties);


                    }
                    transaction.Commit();
                    this.storeOnlyReferencesOfListItems = false;
                }
            }
        }

        /// <summary>
        /// Insert or update object partially by a Transaction, only provided properties are saved 
        /// </summary>
        /// <param name="obj">object of which properties will be stored</param>
        /// <param name="onlyReferences">if true,it will store only references to complex objects</param>
        /// <param name="transaction">Transaction object</param>
        /// <param name="properties">properties to be stored</param>
        public void StoreObjectPartially(object obj, bool onlyReferences, Transactions.ITransaction transaction, params string[] properties)
        {
            lock (_locker)
            {
                this.storeOnlyReferencesOfListItems = onlyReferences;
                SqoTypeInfo ti = this.GetSqoTypeInfoToStoreObject(obj);
                if (ti != null)
                {
                    circularRefCache.Clear();
                    circularRefCache.Add(obj);
                    storageEngine.SaveObjectPartially(obj, ti, properties);
                }
                this.storeOnlyReferencesOfListItems = false;
            }
        }

        /// <summary>
        /// Insert or update object by a Transaction; if object is loaded from database and this method is called then update will occur, if object is new created then insert will occur
        /// </summary>
        /// <param name="obj">Object to be stored</param>
        /// <param name="transaction">Transaction object</param>

        public void StoreObject(object obj,Transactions.ITransaction transaction)
        {
            lock (_locker)
            {
                LightningDB.LightningTransaction lmdbTr = transactionManager.GetActiveTransaction();
                   
                SqoTypeInfo ti = this.GetSqoTypeInfoToStoreObject(obj);
                if (ti != null)
                {
                    circularRefCache.Clear();

                    circularRefCache.Add(obj);
                    bool inserted = false;
#if UNITY3D
                    if (this.savedObject != null)//optimization 
                    {
                        inserted = metaCache.GetOIDOfObject(obj, ti) == 0;
                    }
#else
                    if (this.SavedObject != null)//optimization 
                    {
                        inserted = metaCache.GetOIDOfObject(obj, ti) == 0;
                    }
#endif
                    storageEngine.SaveObject(obj, ti, lmdbTr);
                    SavedEventsArgs saved = new SavedEventsArgs(obj.GetType(), obj);
                    saved.Inserted = inserted;
                    this.OnSavedObject(saved);
                }
            }
        }

       
        private SqoTypeInfo GetSqoTypeInfoToStoreObject(object obj)
        {
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }
            Type objType = obj.GetType();
            SavingEventsArgs ev = new SavingEventsArgs(objType, obj);

            this.OnSavingObject(ev);
            if (ev.Cancel)
            {
                return null;
            }

            return this.GetSqoTypeInfoToStoreObject(obj.GetType());
        }


        private SqoTypeInfo GetSqoTypeInfoToStoreObject(Type objType)
        {
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }
            SqoTypeInfo ti = null;


            if (this.metaCache.Contains(objType))
            {
                ti = this.metaCache.GetSqoTypeInfo(objType);
            }
            else
            {
                ti = MetaExtractor.GetSqoTypeInfo(objType);
                storageEngine.SaveType(ti, transactionManager.GetActiveTransaction());
               
                this.metaCache.AddType(objType, ti);
                this.indexManager.BuildIndexes(ti, transactionManager.GetActiveTransaction());

            }
            if (ti.IsOld)
            {
                throw new TypeChangedException("Actual runtime Type:" + ti.Type.Name + "is different than Type stored in DB, in current version is not supported automatically type changing, to fix this, modify your class like it was when u saved objects in DB");
            }
            return ti;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal IObjectList<T> Load<T>(System.Linq.Expressions.Expression expression)
        {
            lock (_locker)
            {
                bool started;
                var transaction = transactionManager.GetActiveTransaction(out started);
                try
                {

                    SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();

                    List<int> oids = LoadOids<T>(expression);
                    var list = storageEngine.LoadByOIDs<T>(oids, ti);
                    return list;
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }

        }
        /// <summary>
        /// Load all objects of Type provided
        /// </summary>
        /// <typeparam name="T">Type of objects to be loaded from database</typeparam>
        /// <returns>List of objects retrieved from database</returns>
        public IObjectList<T> LoadAll<T>()
        {
            lock (_locker)
            {
                bool started;
                var transaction = transactionManager.GetActiveTransaction(out started);
                try
                {
                    
                    SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
                    var all= storageEngine.LoadAll<T>(ti);
                    return all;
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }
        }
        public IObjectList<T> LoadAll<T>(Transactions.ITransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");
            lock (_locker)
            {
                SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
                return storageEngine.LoadAll<T>(ti);
            }
        }
        /// <summary>
        /// Load object from database by OID provided
        /// </summary>
        /// <typeparam name="T">The Type of object to be loaded</typeparam>
        /// <param name="oid">oid of object</param>
        /// <returns>the object stored in database with oid provided</returns>
        public T LoadObjectByOID<T>(int oid)
        {
            lock (_locker)
            {
                 bool started;
                var transaction = transactionManager.GetActiveTransaction(out started);
                try
                {

                    SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
                    var ret = storageEngine.LoadObjectByOID<T>(ti, oid);
                    return ret;

                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }
        }

        internal T LoadObjectByOID<T>(int oid,List<string> properties)
        {
            lock (_locker)
            {
                bool started;
                var transaction = transactionManager.GetActiveTransaction(out started);
                try
                {
                    SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
                    T g= (T)storageEngine.LoadObjectByOID(ti, oid, properties);
                    return g;
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Close database
        /// </summary>
        public void Close()
        {
            lock (_locker)
            {
                this.opened = false;
                this.metaCache = null;
                this.storageEngine.Close();
                if (documentStore != null)
                    documentStore.ClearCache();
            }
        }
        public void Dispose()
        {
            this.opened = false;
            this.metaCache = null;
            this.storageEngine.Close();
            
        }

        /// <summary>
        /// Flush buffered data to database
        /// </summary>
		public void Flush()
		{
            lock (_locker)
            {
                this.storageEngine.Flush();
            }
		}

        /// <summary>
        /// Cast method to be used in LINQ queries
        /// </summary>
        /// <typeparam name="T">Type over which LINQ will take action</typeparam>
        /// <returns></returns>
        public  ISqoQuery<T> Cast<T>()
        {
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }
           return new SqoQuery<T>(this);
        }
        /// <summary>
        /// Query method to be used in LINQ queries
        /// </summary>
        /// <typeparam name="T">Type over which LINQ will take action</typeparam>
        /// <returns></returns>
        public ISqoQuery<T> Query<T>()
        {
            return this.Cast<T>();
        }
        /// <summary>
        /// Load OIDs by expression
        /// </summary>
        /// <typeparam name="T">Type for which OIDs will be loaded</typeparam>
        /// <param name="expression">filter expression</param>
        /// <returns>List of OIDs</returns>
        public List<int> LoadOids<T>(System.Linq.Expressions.Expression expression)
        {
            lock (_locker)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException("expression");
                }
                bool started;
                var transaction = transactionManager.GetActiveTransaction(out started);
                try
                {
                    SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
                    QueryTranslator t = new QueryTranslator(this.storageEngine, ti);
                    ICriteria criteria = t.Translate(expression);
                    return criteria.GetOIDs();
                }
                finally
                {
                    if (started)
                        transaction.Commit();
                }
            }
        }

		internal List<int> LoadAllOIDs<T>()
		{
            lock (_locker)
            {
                using (var transaction = transactionManager.BeginTransaction())
                {
                    SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
                    var alloids= storageEngine.LoadAllOIDs(ti);
                    transaction.Commit();
                    return alloids;
                }
            }
		}


		/// <summary>
		/// Load all object OIDs of MetaType provided
		/// </summary>
		/// <param name="type">meta type Load by method GetAllTypes()</param>
		/// <returns></returns>
		public List<int> LoadAllOIDs(MetaType type)
		{
            lock (_locker)
            {
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }
                using (var transaction = transactionManager.BeginTransaction())
                {
                    var oids = storageEngine.LoadAllOIDs(type.Name);
                    transaction.Commit();
                    return oids;

                }
            }
		}

        internal SqoTypeInfo GetSqoTypeInfo<T>()
        {
           
            Type objType = typeof(T);
            return this.GetSqoTypeInfo(objType);
        }

		internal SqoTypeInfo GetSqoTypeInfo(Type t)
		{
			SqoTypeInfo ti = null;
			Type objType =t;
			if (this.metaCache.Contains(objType))
			{
                ti = metaCache.GetSqoTypeInfo(objType);
			}
			else
			{
				ti = MetaExtractor.GetSqoTypeInfo(objType);
			}
			return ti;
		}


       


        internal object LoadValue(int oid, string fieldName, Type type)
        {
            bool started;
            var transaction = transactionManager.GetActiveTransaction(out started);
            try
            {
                var val = this.LoadValue(oid, fieldName, type, transactionManager.GetActiveTransaction());
                return val;
            }
            finally
            {
                if (started)
                    transaction.Commit();
            }
        }
        internal object LoadValue(int oid, string fieldName, Type type,LightningDB.LightningTransaction transaction)
        {

            SqoTypeInfo ti = this.GetSqoTypeInfo(type);
            return storageEngine.LoadValue(oid, fieldName, ti,transaction);

        }

		/// <summary>
		/// Load value of a field of an object identified by OID provided
		/// </summary>
		/// <param name="oid">OID of object</param>
		/// <param name="fieldName">fieldName</param>
		/// <param name="mt">MetaType</param>
		/// <returns></returns>
		public object LoadValue(int oid, string fieldName, MetaType mt)
		{
            lock (_locker)
            {
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }
                using (var transaction = transactionManager.BeginTransaction())
                {
                    if (!cacheForManager.Contains(mt.Name))
                    {
                        SqoTypeInfo ti = storageEngine.GetSqoTypeInfo(mt.Name);
                        cacheForManager.AddType(mt.Name, ti);
                    }
                    SqoTypeInfo tinf = cacheForManager.GetSqoTypeInfo(mt.Name);
                    var value = storageEngine.LoadValue(oid, fieldName, tinf);
                    transaction.Commit();
                    return value;
                }
            }

		}

        /// <summary>
        /// Delete an object from database
        /// </summary>
        /// <param name="obj">Object to be deleted</param>
		public void Delete(object obj)
		{
            lock (_locker)
            {
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }
                Type t = obj.GetType();
                using (var transaction = transactionManager.BeginTransaction())
                {
                    SqoTypeInfo ti = this.GetSqoTypeInfo(t);
                    bool deleted = DeleteObjInternal(obj, ti, transaction);
                    transaction.Commit();
                }
            }
		}

        /// <summary>
        /// Delete an object from database using a Transaction
        /// </summary>
        /// <param name="obj">Object to be deleted</param>
        /// <param name="transaction">Transaction</param>
        public void Delete(object obj, Transactions.ITransaction transaction)
        {
            lock (_locker)
            {
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }

                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }
               
                if (((Transactions.Transaction)transaction).status == Transactions.TransactionStatus.Closed)
                {
                    throw new SiaqodbException("Transaction closed!");
                }
                Type t = obj.GetType();
                SqoTypeInfo ti = this.GetSqoTypeInfo(t);
                DeleteObjInternal(obj, ti, transaction);
            }
        }

        /// <summary>
        /// Delete an object from database by a certain field(ex:ID that come from server)
        /// </summary>
        /// <param name="obj">Object to be deleted</param>
        /// <param name="fieldName">Names of field that this method will lookup for object to delete it</param>
        public bool DeleteObjectBy(string fieldName,object obj)
        {
            return this.DeleteObjectBy(obj, fieldName);
        }

        /// <summary>
        /// Delete an object from database by a certain field(ex:ID that come from server)
        /// </summary>
        /// <param name="obj">Object to be deleted</param>
        /// <param name="fieldNames">Names of fields that this method will lookup for object to delete it</param>
        public bool DeleteObjectBy(object obj,params string[] fieldNames)
        {
            lock (_locker)
            {
                using (var transaction = transactionManager.BeginTransaction())
                {
                    bool deleted = this.DeleteObjectBy(obj, transaction, fieldNames);
                    transaction.Commit();
                    return deleted;
                }
            }
        }

        /// <summary>
        /// Delete an object from database by a certain field(ex:ID that come from server)
        /// </summary>
        /// <param name="obj">Object to be deleted</param>
        /// <param name="fieldNames">Names of fields that this method will lookup for object to delete it</param>
        /// <param name="transaction">Transaction object</param>

        public bool DeleteObjectBy(object obj, Transactions.ITransaction transaction, params string[] fieldNames)
        {
            lock (_locker)
            {
                if (fieldNames == null || fieldNames.Length == 0)
                {
                    throw new ArgumentNullException("fieldNames");
                }
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }


                if (transaction != null)
                {
                    if (((Transactions.Transaction)transaction).status == Transactions.TransactionStatus.Closed)
                    {
                        throw new SiaqodbException("Transaction closed!");
                    }
                }
               
                Type t = obj.GetType();
                SqoTypeInfo ti = this.GetSqoTypeInfo(t);
                DeletingEventsArgs delEv = new DeletingEventsArgs(ti.Type, -1);//we don't know it
                this.OnDeletingObject(delEv);
                if (delEv.Cancel)
                {
                    return false;
                }
               
                int OID_deleted = storageEngine.DeleteObjectBy(fieldNames, obj, ti, transactionManager.GetActiveTransaction());
                DeletedEventsArgs deletedEv = new DeletedEventsArgs(ti.Type, OID_deleted);
                this.OnDeletedObject(deletedEv);

                return OID_deleted > 0;
            }
        }

         /// <summary>
        /// Delete an object from database by a criteria
        /// </summary>
        /// <param name="criteria">Pairs of fields-values to lookup for object to delete it</param>
        /// <returns>Number of objects deleted</returns>
        public int DeleteObjectBy(Type objectType,Dictionary<string, object> criteria)
        {
            lock (_locker)
            {
                if (criteria == null || criteria.Keys.Count == 0)
                {
                    throw new ArgumentNullException("criteria");
                }
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }

                SqoTypeInfo ti = this.GetSqoTypeInfo(objectType);
                DeletingEventsArgs delEv = new DeletingEventsArgs(ti.Type, -1);//we don't know it
                this.OnDeletingObject(delEv);
                if (delEv.Cancel)
                {
                    return 0;
                }
                using (var transaction = transactionManager.BeginTransaction())
                {
                    List<int> oidsDeleted = storageEngine.DeleteObjectBy(ti, criteria);
                    transaction.Commit();
                    foreach (int oid in oidsDeleted)
                    {
                        DeletedEventsArgs deletedEv = new DeletedEventsArgs(ti.Type, oid);
                        this.OnDeletedObject(deletedEv);
                    }
                    
                    
                    return oidsDeleted.Count;
                }
                
            }
        }
        /// <summary>
        /// Delete an object from database by a criteria
        /// </summary>
        /// <param name="criteria">Pairs of fields-values to lookup for object to delete it</param>
        /// <returns>Number of objects deleted</returns>
        public int DeleteObjectBy<T>(Dictionary<string,object> criteria)
        {
            return DeleteObjectBy(typeof(T), criteria);
        }

        
        /// <summary>
        /// Delete all objects of Type provided
        /// </summary>
        /// <typeparam name="T">Type of objects to be deleted</typeparam>
		public void DropType<T>()
		{
            this.DropType(typeof(T));
		}

        /// <summary>
        /// Delete all objects of Type provided
        /// </summary>
        /// <param name="type">Type of objects to be deleted</param>>
        public void DropType(Type type)
        {
            lock (_locker)
            {
                using (var transaction = transactionManager.BeginTransaction())
                {
                    this.DropType(type, transaction);
                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// Delete all objects of Type provided
        /// </summary>
        /// <param name="type">Type of objects to be deleted</param>>
        public void DropType(Type type,Sqo.Transactions.ITransaction transaction )
        {
            lock (_locker)
            {
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }

                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                if (((Transactions.Transaction)transaction).status == Transactions.TransactionStatus.Closed)
                {
                    throw new SiaqodbException("Transaction closed!");
                }
                SqoTypeInfo ti = this.GetSqoTypeInfo(type);
                storageEngine.DropType(ti);
                indexManager.DropIndexes(ti, transactionManager.GetActiveTransaction());
                this.metaCache.Remove(type);
            }
        }


		internal object LoadObjectByOID(Type type,int oid)
		{
            using (var transaction = transactionManager.BeginTransaction())
            {
                SqoTypeInfo ti = this.GetSqoTypeInfo(type);
                var obj =storageEngine.LoadObjectByOID(ti, oid);
                transaction.Commit();
                return obj;
            }
		}

        /// <summary>
        /// Return all Types from database folder
        /// </summary>
        /// <returns>List of MetaType objects</returns>
		public List<MetaType> GetAllTypes()
		{
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }

            using (var transaction = transactionManager.BeginTransaction())
            {
                List<MetaType> list = new List<MetaType>();
                List<SqoTypeInfo> tiList = storageEngine.LoadAllTypesForObjectManager();
                foreach (SqoTypeInfo ti in tiList)
                {
                    MetaType mt = new MetaType();
                    mt.Name = ti.TypeName;
                    mt.TypeID = ti.Header.TID;
                    mt.FileName = ti.FileNameForManager;
                    foreach (FieldSqoInfo fi in ti.Fields)
                    {
                        MetaField mf = new MetaField();

                        mf.FieldType = fi.AttributeType;
                        mf.Name = fi.Name;
                        mt.Fields.Add(mf);
                    }
                    list.Add(mt);
                }
                transaction.Commit();
                return list;
            }

		}

        /// <summary>
        /// Return number of objects of Type provided
        /// </summary>
        /// <typeparam name="T">Type of objects</typeparam>
        /// <returns></returns>
        public int Count<T>()
        {
            lock (_locker)
            {
                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }
                using (var transaction = transactionManager.BeginTransaction())
                {
                    SqoTypeInfo ti = this.GetSqoTypeInfo<T>();
                    var count= storageEngine.Count(ti);
                    transaction.Commit();
                    return count;
                }
            }
        }

        
     
        
       
        /// <summary>
        /// Update an object in database by a certain Field(eq: ID that come from a server)
        /// </summary>
        /// <param name="fieldName">FieldName by which update is made(eq an ID)</param>
        /// <param name="obj">object that has all values but not OID to update it in database</param>
        /// <returns>true if object was updated and false if object was not found in database</returns>
        public bool UpdateObjectBy(string fieldName, object obj)
        {
            return this.UpdateObjectBy(obj, fieldName);
        }

        /// <summary>
        /// Update an object in database by certain Fields(eq: ID that come from a server)
        /// </summary>
        /// <param name="fieldNames">name of fields by which update is made(eq an ID)</param>
        /// <param name="obj">object that has all values but not OID to update it in database</param>
        /// <returns>true if object was updated and false if object was not found in database</returns>
        
        public bool UpdateObjectBy(object obj,params string[] fieldNames)
        {
            lock (_locker)
            {
                using (var transaction = transactionManager.BeginTransaction())
                {
                    bool updated = this.UpdateObjectBy(obj, transaction, fieldNames);
                    transaction.Commit();
                    return updated;
                }
            }
        }

        /// <summary>
        /// Update an object in database by certain Fields(eq: ID that come from a server)
        /// </summary>
        /// <param name="fieldNames">name of fields by which update is made(eq an ID)</param>
        /// <param name="obj">object that has all values but not OID to update it in database</param>
        /// <param name="transaction">Transaction object</param>
        /// <returns>true if object was updated and false if object was not found in database</returns>
        public bool UpdateObjectBy(object obj, Sqo.Transactions.ITransaction transaction, params string[] fieldNames)
        {
            lock (_locker)
            {
                if (fieldNames == null || fieldNames.Length == 0)
                {
                    throw new ArgumentNullException("fieldsName");
                }

                if (transaction != null)
                {
                    if (((Transactions.Transaction)transaction).status == Transactions.TransactionStatus.Closed)
                    {
                        throw new SiaqodbException("Transaction closed!");
                    }
                }

              
                SqoTypeInfo ti = GetSqoTypeInfoToStoreObject(obj);
                if (ti != null)
                {

                    bool stored = storageEngine.UpdateObjectBy(fieldNames, obj, ti, transactionManager.GetActiveTransaction());

                    SavedEventsArgs saved = new SavedEventsArgs(obj.GetType(), obj);
                    saved.Inserted = false;
                    this.OnSavedObject(saved);

                    return stored;
                }
                return false;
            }
        }

        internal bool UpdateField(int oid,MetaType metaType, string field, object value)
        {
            using (var transaction = transactionManager.BeginTransaction())
            {

                if (!opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }
                if (!cacheForManager.Contains(metaType.Name))
                {
                    SqoTypeInfo ti = storageEngine.GetSqoTypeInfo(metaType.Name);
                    cacheForManager.AddType(metaType.Name, ti);
                }
                SqoTypeInfo tinf = cacheForManager.GetSqoTypeInfo(metaType.Name);

                var result = storageEngine.SaveValue(oid, field, tinf, value, transactionManager.GetActiveTransaction());
                transaction.Commit();
                return result;
            }

           
        }
        

        #region private methods
        private bool DeleteObjInternal(object obj, SqoTypeInfo ti, Transactions.ITransaction transaction)
        {
            int oid = metaCache.GetOIDOfObject(obj, ti);
            if (oid <= 0 || oid > ti.Header.numberOfRecords)
            {
                throw new SiaqodbException("Object not exists in database!");
            }

            DeletingEventsArgs delEv = new DeletingEventsArgs(ti.Type, oid);
            this.OnDeletingObject(delEv);
            if (delEv.Cancel)
            {
                return false;
            }
           
            storageEngine.DeleteObject(obj, ti, transactionManager.GetActiveTransaction());

            DeletedEventsArgs deletedEv = new DeletedEventsArgs(ti.Type, oid);
            this.OnDeletedObject(deletedEv);
            return true;
        }

        internal SqoTypeInfo CheckDBAndGetSqoTypeInfo<T>()
        {
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }
            SqoTypeInfo ti = this.GetSqoTypeInfo<T>();
            if (ti.IsOld)
            {
                throw new TypeChangedException("Actual runtime Type:" + ti.Type.Name + "is different than Type stored in DB, in current version is not supported automatically type changing, to fix this, modify your class like it was when u saved objects in DB");
            }
            return ti;
        }
        private SqoTypeInfo CheckDBAndGetSqoTypeInfo(Type type)
        {
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }
            SqoTypeInfo ti = this.GetSqoTypeInfo(type);
            if (ti.IsOld)
            {
                throw new TypeChangedException("Actual runtime Type:" + ti.Type.Name + "is different than Type stored in DB, in current version is not supported automatically type changing, to fix this, modify your class like it was when u saved objects in DB");
            }
            return ti;
        }
    #endregion


        internal List<object> LoadDirtyObjects(Type type)
        {
            lock (_locker)
            {
                SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo(type);
                Where w = new Where("isDirty", OperationType.Equal, true);
                w.StorageEngine = this.storageEngine;
                w.ParentSqoTypeInfo = ti;
                w.ParentType.Add(w.ParentSqoTypeInfo.Type);
                List<int> oidsDirty = w.GetOIDs();

                Where wDelete = new Where("isTombstone", OperationType.Equal, true);
                wDelete.StorageEngine = this.storageEngine;
                wDelete.ParentSqoTypeInfo = ti;
                wDelete.ParentType.Add(wDelete.ParentSqoTypeInfo.Type);
                //TODO LMDB
                //List<int> oidsDeleted = this.storageEngine.LoadFilteredDeletedOids(wDelete, ti);

                //oidsDirty.AddRange(oidsDeleted);

                return this.storageEngine.LoadByOIDs(oidsDirty, ti);
            }
        }


        /// <summary>
        /// return current database path
        /// </summary>
        /// <returns>The database folder path</returns>
        public string GetDBPath()
        {
            return this.path;
        }
        /// <summary>
        /// Start a database Transaction to be used on insert/update/delete objects
        /// </summary>
        /// <returns> Transaction object</returns>
        public Transactions.ITransaction BeginTransaction()
        {
            lock (_locker)
            {
                this.circularRefCache.Clear();
                return transactionManager.BeginTransaction();
            }
        }
      
        

      
       
        internal void Flush<T>()
        {
            SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
            storageEngine.Flush(ti);

        }


        internal void DeleteObjectByMeta(int oid, MetaType metaType)
        {
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }
            using (var transaction = transactionManager.BeginTransaction())
            {
                if (!cacheForManager.Contains(metaType.Name))
                {
                    SqoTypeInfo ti = storageEngine.GetSqoTypeInfo(metaType.Name);
                    cacheForManager.AddType(metaType.Name, ti);
                }
                SqoTypeInfo tinf = cacheForManager.GetSqoTypeInfo(metaType.Name);

                storageEngine.DeleteObjectByOID(oid, tinf);
                transaction.Commit();
            }

        }

        internal int InsertObjectByMeta(MetaType metaType)
        {
            if (!opened)
            {
                throw new SiaqodbException("Database is closed, call method Open() to open it!");
            }
            using (var transaction = transactionManager.BeginTransaction())
            {
                if (!cacheForManager.Contains(metaType.Name))
                {
                    SqoTypeInfo ti = storageEngine.GetSqoTypeInfo(metaType.Name);
                    cacheForManager.AddType(metaType.Name, ti);
                }
                SqoTypeInfo tinf = cacheForManager.GetSqoTypeInfo(metaType.Name);
                var result = storageEngine.InsertObjectByMeta(tinf);
                transaction.Commit();
                return result;
            }

        }

        
        /// <summary>
        /// Load all objects in Lazy mode, objects are activated/read from db when it is accessed
        /// by index or by enumerator
        /// </summary>
        /// <typeparam name="T">Type of objects to be loaded from database</typeparam>
        /// <returns>LazyObjectList of objects</returns>
        public IObjectList<T> LoadAllLazy<T>()
        {
            lock (_locker)
            {
                SqoTypeInfo ti = CheckDBAndGetSqoTypeInfo<T>();
                List<int> oids = storageEngine.LoadAllOIDs(ti);
                return new LazyObjectList<T>(this, oids);
            }
        }

        internal void LoadObjectOIDAndTID(int oid, string fieldName, MetaType mt,ref List<int> listOIDs,ref int TID)
        {
            using (var transaction = transactionManager.BeginTransaction())
            {
                if (!cacheForManager.Contains(mt.Name))
                {
                    SqoTypeInfo ti = storageEngine.GetSqoTypeInfo(mt.Name);
                    cacheForManager.AddType(mt.Name, ti);
                }
                SqoTypeInfo tinf = cacheForManager.GetSqoTypeInfo(mt.Name);
                FieldSqoInfo fi = MetaHelper.FindField(tinf.Fields, fieldName);
                if (fi.AttributeTypeId == MetaExtractor.complexID || fi.AttributeTypeId == MetaExtractor.documentID)
                {
                    KeyValuePair<int, int> kv = storageEngine.LoadOIDAndTID(oid, fi, tinf);
                    listOIDs.Add(kv.Key);
                    TID = kv.Value;
                }
                else if (fi.AttributeTypeId - MetaExtractor.ArrayTypeIDExtra == MetaExtractor.complexID)
                {
                    List<KeyValuePair<int, int>> list = storageEngine.LoadComplexArray(oid, fi, tinf);
                    if (list.Count > 0)
                    {
                        TID = list[0].Value;
                        foreach (KeyValuePair<int, int> kv in list)
                        {
                            listOIDs.Add(kv.Key);
                        }
                    }
                }
                transaction.Commit();
            }

        }
       

        internal void LoadTIDofComplex(int oid, string fieldName, MetaType mt, ref int TID, ref bool isArray)
        {
            using (var transaction = transactionManager.BeginTransaction())
            {
                if (!cacheForManager.Contains(mt.Name))
                {
                    SqoTypeInfo ti = storageEngine.GetSqoTypeInfo(mt.Name);
                    cacheForManager.AddType(mt.Name, ti);
                }
                SqoTypeInfo tinf = cacheForManager.GetSqoTypeInfo(mt.Name);
                FieldSqoInfo fi = MetaHelper.FindField(tinf.Fields, fieldName);
                if (fi.AttributeTypeId == MetaExtractor.complexID || fi.AttributeTypeId == MetaExtractor.documentID)
                {
                    KeyValuePair<int, int> kv = storageEngine.LoadOIDAndTID(oid, fi, tinf);
                    TID = kv.Value;
                    isArray = false;
                }
                else if (fi.AttributeTypeId - MetaExtractor.ArrayTypeIDExtra == MetaExtractor.complexID)
                {
                    isArray = true;
                    TID = storageEngine.LoadComplexArrayTID(oid, fi, tinf);
                }
                else if (fi.AttributeTypeId - MetaExtractor.ArrayTypeIDExtra == MetaExtractor.jaggedArrayID)
                {
                    isArray = true;
                    TID = -32;
                }
                else if (fi.AttributeTypeId == MetaExtractor.dictionaryID)
                {
                    TID = -31;
                }
                transaction.Commit();
            }

        }

       
        /// <summary>
        /// Get OID of object, if the Type of object has not defined OID property then object and OID are weak cached during object load from database and this value is returned,
        /// otherwise it is returned value of the OID property 
        /// </summary>
        /// <param name="obj">The object for which OID is returned</param>
        /// <returns>The OID associated with object that is stored in database</returns>
        public int GetOID(object obj)
        {
            lock (_locker)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException("obj");
                }

                SqoTypeInfo ti = this.CheckDBAndGetSqoTypeInfo(obj.GetType());
                return metaCache.GetOIDOfObject(obj, ti);
            }
        }
        internal void SetDatabaseFileName(string fileName, MetaType type)
        {

            Cache.CacheCustomFileNames.AddFileNameForType(type.Name, fileName, false);

        }
        internal void GetOIDForAMSByField(object obj, string fieldName)
        {
            SqoTypeInfo ti = GetSqoTypeInfoToStoreObject(obj.GetType());
            List<int> oids = storageEngine.LoadOidsByField(ti, fieldName, obj);

            if (oids.Count > 1)
            {
                throw new SiaqodbException("Many objects with this field value exists is database.");
            }
            else if (oids.Count == 1)
            {
                metaCache.SetOIDToObject(obj, oids[0], ti);
            }
        }

        /// <summary>
        /// Returns OIDs and values for a field
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="fi"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        internal List<ATuple<int, object>> GetAllValues(SqoTypeInfo ti, FieldSqoInfo fi, LightningDB.LightningTransaction transaction)
        {
            return storageEngine.GetAllValues(ti, fi, transaction);
        }
        public Stat DbInfo
        {

            get
            {
                if (!this.opened)
                {
                    throw new SiaqodbException("Database is closed, call method Open() to open it!");
                }
                return new Stat
                {
                    UsedSize = transactionManager.EnvUsedSize(),
                    MaxSize=transactionManager.EnvMaxSize(),
                    MaxSubDatabases=transactionManager.EnvMaxDatabases(),
                    Path=this.GetDBPath()
                };
            }
        }

        #region Anchor (SyncFRW proivider)
        internal void SaveAnchor(string key, byte[] value)
        {
            bool started;
            var tr=transactionManager.GetActiveTransaction(out started);
            try
            {
                storageEngine.SaveAnchor(key, value, transactionManager.GetActiveTransaction());
            }
            finally
            {
                if (started)
                    tr.Commit();
            }

        }
        internal byte[] GetAnchor(string key)
        {
            bool started;
            var tr = transactionManager.GetActiveTransaction(out started);
            try
            {
                return storageEngine.GetAnchor(key, transactionManager.GetActiveTransaction());
            }
            finally
            {
                if (started)
                    tr.Commit();
            }
        }
        internal void DropAnchor(string key)
        {
            bool started;
            var tr = transactionManager.GetActiveTransaction(out started);
            try
            {
                storageEngine.DropAnchor(key, transactionManager.GetActiveTransaction());
            }
            finally
            {
                if (started)
                    tr.Commit();
            }
        }
        #endregion

        #region Documents
        Documents.DocumentStore documentStore;
        public Documents.DocumentStore Documents
        {
            get
            {
                if (documentStore == null)
                    documentStore = new Sqo.Documents.DocumentStore(this);
                return documentStore;
            }
        }
        #endregion
        public class Stat
        {
           /// <summary>
           /// Database used size (in bytes)
           /// </summary>
            public long UsedSize { get; internal set; }
            
            /// <summary>
            /// Max database size ( in bytes)
            /// </summary>
            public long MaxSize { get; internal set; }
            /// <summary>
            /// Max sub databases, Siaqodb use 2 sub-databases per Type and an additional sub-database for each index
            /// </summary>
            public int MaxSubDatabases { get; internal set; }
            /// <summary>
            /// Full db folder path
            /// </summary>
            public string Path { get; internal set; }
            /// <summary>
            /// Free space (in bytes)
            /// </summary>
            public long FreeSpace { get { return MaxSize - UsedSize; } }
            public override string ToString()
            {
                double free=this.MaxSize - UsedSize ;
                double mbfree = free> 0 ? free / ((double)1024*1024) : 0;
                mbfree=Math.Round(mbfree,2);
                double total=this.MaxSize/((double)1024*1024);
                total = Math.Round(total, 2);
                return string.Format(@"Database:{0}\data.mdb has {1} MB free space out of {2} MB", this.Path,mbfree,total);
            }
        }
    }
    public static class SqoStringExtensions
    {
        /// <summary>
        ///  Returns a value indicating whether the specified System.String object occurs
        ///    within this string.A parameter specifies the type of search
        ///     to use for the specified string.
        /// </summary>
        /// <param name="stringObj">Input string</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType"> One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>true if the value parameter occurs within this string, or if value is the
        ///     empty string (""); otherwise, false.</returns>
        public static bool Contains(this string stringObj, string value, StringComparison comparisonType)
        {
            return stringObj.IndexOf(value, comparisonType) != -1;
        }

    }
   
}
