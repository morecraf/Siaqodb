using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Sqo.Meta;
using System.Diagnostics;
using Sqo.Core;
using System.IO;
using Sqo.Queries;
using System.Reflection;
using Sqo.Exceptions;
using Sqo.Utilities;
using Sqo.Indexes;
using Sqo.Transactions;

using Sqo.Cache;
using LightningDB;
#if ASYNC
using System.Threading.Tasks;

#endif
#if WinRT
using Windows.Storage.Search;
using Windows.Storage;
#endif
#if SILVERLIGHT
	using System.IO.IsolatedStorage;
#endif


namespace Sqo
{
    [Obfuscation(Feature = "Apply to member * when event: renaming", Exclude = true)]
    partial class StorageEngine :IDisposable
    {
        #region VAR DECLARATIONS

#if WinRT
        internal StorageFolder storageFolder;
#endif
        internal string path;
        private object _syncRoot=new object();
        internal MetaCache metaCache;
        
        RawdataSerializer rawSerializer;
        internal IndexManager indexManager;
        CircularRefCache circularRefCache = new CircularRefCache();
        List<ATuple<Type, String>> includePropertiesCache;
        List<object> parentsComparison;
        bool useElevatedTrust;
        TransactionManager transactionManager;
        const string sys_dbs = "sdbs";

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
        private EventHandler<ComplexObjectEventArgs> needSaveComplexObject;
        public event EventHandler<ComplexObjectEventArgs> NeedSaveComplexObject
        {
            add
            {
                lock (_syncRoot)
                {
                    needSaveComplexObject += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    needSaveComplexObject -= value;
                }
            }

        }
		#if ASYNC
		private ComplexObjectEventHandler needSaveComplexObjectAsync;
		public event ComplexObjectEventHandler NeedSaveComplexObjectAsync
		{
			add
			{
				lock (_syncRoot)
				{
					needSaveComplexObjectAsync += value;
				}
			}
			remove
			{
				lock (_syncRoot)
				{
					needSaveComplexObjectAsync	 -= value;
				}
			}

		}
#endif
#else
        public event EventHandler<LoadingObjectEventArgs> LoadingObject;
        public event EventHandler<LoadedObjectEventArgs> LoadedObject;
        public event EventHandler<ComplexObjectEventArgs> NeedSaveComplexObject;
#if ASYNC
        public event ComplexObjectEventHandler NeedSaveComplexObjectAsync;
#endif
        
#endif
#if UNITY3D || CF || MONODROID
#else
        public event EventHandler<IndexesSaveAsyncFinishedArgs> IndexesSaveAsyncFinished;
#endif

        #endregion

        #region CTOR

        public StorageEngine(string path,TransactionManager transactionManager )
        {
            this.transactionManager = transactionManager;
            
            if (!SqoLicense.LicenseValid())
            {
                throw new InvalidLicenseException("License not valid!");
            }
            this.path = path;
            SerializerFactory.ClearCache(path);
            this.rawSerializer=new RawdataSerializer(this, useElevatedTrust);
            
        }
       
        public StorageEngine(string path,bool useElevatedTrust)
        {

            if (!Sqo.Utilities.SqoLicense.LicenseValid())
            {
                throw new InvalidLicenseException("License not valid!");
            }
            this.path = path;
            SerializerFactory.ClearCache(path);
            this.useElevatedTrust = useElevatedTrust;
            this.rawSerializer = new RawdataSerializer(this, useElevatedTrust);
           
        }

#if WinRT
        public StorageEngine(StorageFolder s)
        {

            if (!SqoLicense.LicenseValid())
            {
                  throw new InvalidLicenseException("License not valid!");
            }
            this.storageFolder = s;
            this.path = storageFolder.Path;
            SerializerFactory.ClearCache(this.storageFolder.Path);
            this.rawSerializer = new RawdataSerializer(this, useElevatedTrust);


        }
#endif


        #endregion

        #region TYPE MANAGEMENT

        public void SaveType(SqoTypeInfo ti)
        {

            var transaction = transactionManager.GetActiveTransaction();


            this.SaveType(ti, transaction);
        }
        public void SaveType(SqoTypeInfo ti, LightningTransaction transaction)
        {
            if (ti.Header.TID == 0)
            {
                ti.Header.TID = metaCache.GetNextTID();
            }
            var db = transaction.OpenDatabase(sys_dbs, DatabaseOpenFlags.Create);

            byte[] key = ByteConverter.StringToByteArray(GetFileByType(ti));
            transaction.Put(db, key, ObjectSerializer.SerializeType(ti));




        }
#if ASYNC
        public async Task SaveTypeAsync(SqoTypeInfo ti)
        {
            if (ti.Header.TID == 0)
            {
                ti.Header.TID = metaCache.GetNextTID();
            }
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            await serializer.SerializeTypeAsync(ti).ConfigureAwait(false);

        }
#endif
        private string GetFileByType(SqoTypeInfo ti)
        {
            if (ti.Header.version > -31 && ti.Type!=null) //version less than 3.1
            {
                return MetaHelper.GetOldFileNameByType(ti.Type);
            }
            else
            {
                return GetFileByType(ti.TypeName);
            }
        }
        private string GetFileByType(string typeName)
        {
            string customName = Cache.CacheCustomFileNames.GetFileName(typeName);
            if (customName != null)
            {
                return customName;
            }

            string assemblyName = typeName.Substring(typeName.LastIndexOf(',') + 1);
            string onlyTypeName = typeName.Substring(0, typeName.LastIndexOf(','));
            string fileName = onlyTypeName + "." + assemblyName;

            //fileName = fileName.GetHashCode().ToString();

#if SILVERLIGHT
            if (!SiaqodbConfigurator.UseLongDBFileNames && !fileName.StartsWith("Sqo.Indexes.BTreeNode"))
            {
                fileName = fileName.GetHashCode().ToString();
            }
#endif

            return fileName;
        }
       
        internal SqoTypeInfo GetSqoTypeInfoSoft(Type t)
        {
            SqoTypeInfo ti = null;
            Type objType = t;
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
#if !WinRT
        internal void LoadMetaDataTypesForManager()
        {

          /*  CacheCustomFileNames.AddFileNameForType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName, "rawdatainfo", false);
            ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, this.GetFileByType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName), this.useElevatedTrust);
            //LMDB_TODO
             SqoTypeInfo ti = seralizer.DeserializeSqoTypeInfo(true);
             if (ti != null)
             {
                 this.CompareSchema(ti);
             }*/


        }
#if ASYNC
        internal async Task LoadMetaDataTypesForManagerAsync()
        {
            string rawdatainfoName = MetaHelper.GetOldFileNameByType(typeof(Sqo.MetaObjects.RawdataInfo));
            if (MetaHelper.FileExists(this.path, rawdatainfoName, this.useElevatedTrust))
            {
                await this.UpgradeInternalSqoTypeInfosAsync(typeof(Sqo.MetaObjects.RawdataInfo), "rawdatainfo", false).ConfigureAwait(false);
            }
            else
            {
                CacheCustomFileNames.AddFileNameForType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName, "rawdatainfo", false);
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, this.GetFileByType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName), this.useElevatedTrust);
                SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);
                if (ti != null)
                {
                    await this.CompareSchemaAsync(ti).ConfigureAwait(false);
                }
            }

        }
#endif

#endif

        internal void LoadMetaDataTypes()
        {

            CacheCustomFileNames.AddFileNameForType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName, "rawdatainfo", false);
           
            var transaction=transactionManager.GetActiveTransaction();
            {

                string dbName = this.GetFileByType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName);
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, dbName, this.useElevatedTrust);

                var db = transaction.OpenDatabase(sys_dbs, DatabaseOpenFlags.Create);
                {
                    byte[] tinfoBuffer = transaction.Get(db,ByteConverter.StringToByteArray(dbName));
                    if (tinfoBuffer != null)
                    {
                        SqoTypeInfo ti = ObjectSerializer.DeserializeSqoTypeInfoFromBuffer(tinfoBuffer, true);
                        if (ti != null)
                        {
                            this.CompareSchema(ti);
                        }
                    }

                }

            }


        }
       
#if ASYNC
        internal async Task LoadMetaDataTypesAsync()
        {
            string rawdatainfoName = MetaHelper.GetOldFileNameByType(typeof(Sqo.MetaObjects.RawdataInfo));

            #if !WinRT
            if (MetaHelper.FileExists(this.path, rawdatainfoName, this.useElevatedTrust))
            {
                await this.UpgradeInternalSqoTypeInfosAsync(typeof(Sqo.MetaObjects.RawdataInfo), "rawdatainfo", false).ConfigureAwait(false);
                string indexinfoName = MetaHelper.GetOldFileNameByType(typeof(Sqo.Indexes.IndexInfo2));
                if (MetaHelper.FileExists(this.path, indexinfoName, this.useElevatedTrust))
                {
                    await this.UpgradeInternalSqoTypeInfosAsync(typeof(Sqo.Indexes.IndexInfo2), "indexinfo2", true).ConfigureAwait(false);
                }
            }

            else
#endif
            {
                CacheCustomFileNames.AddFileNameForType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName, "rawdatainfo", false);
                CacheCustomFileNames.AddFileNameForType(new SqoTypeInfo(typeof(Sqo.Indexes.IndexInfo2)).TypeName, "indexinfo2", false);
#if KEVAST
                CacheCustomFileNames.AddFileNameForType(new SqoTypeInfo(typeof(KeVaSt.KVSInfo)).TypeName, "KVSInfo", false);
#endif
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, this.GetFileByType(new SqoTypeInfo(typeof(Sqo.MetaObjects.RawdataInfo)).TypeName), this.useElevatedTrust);
                SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);
                if (ti != null)
                {
                    await this.CompareSchemaAsync(ti).ConfigureAwait(false);
                }


                seralizer = SerializerFactory.GetSerializer(path, this.GetFileByType(new SqoTypeInfo(typeof(Sqo.Indexes.IndexInfo2)).TypeName), this.useElevatedTrust);
                ti = await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);
                if (ti != null)
                {
                    await this.CompareSchemaAsync(ti).ConfigureAwait(false);
                }
#if KEVAST
                seralizer = SerializerFactory.GetSerializer(path, this.GetFileByType(new SqoTypeInfo(typeof(KeVaSt.KVSInfo)).TypeName), this.useElevatedTrust);
                ti = await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);
                if (ti != null)
                {
                    await this.CompareSchemaAsync(ti).ConfigureAwait(false);
                }
#endif
            }

        }
       
#endif
        
        internal List<SqoTypeInfo> LoadAllTypesForObjectManager()
        {
            string extension = ".sqo";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
            }
#if SILVERLIGHT

            if (!this.useElevatedTrust)
            {
                List<SqoTypeInfo> list = new List<SqoTypeInfo>();
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                if (isf.DirectoryExists(path))
                {

                }
                else
                {
                    isf.CreateDirectory(path);
                }
               
                string searchPath = Path.Combine(path, "*"+extension);
                string[] files = isf.GetFileNames(searchPath);

                foreach (string f in files)
                {
                    string typeName = f.Replace(extension, "");
             if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo") || typeName.StartsWith("Sqo.Indexes.BTreeNode"))//engine types
                {
                    continue;
                }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, useElevatedTrust);
                    SqoTypeInfo ti = seralizer.DeserializeSqoTypeInfo(false);
                    if (ti != null)
                    {
                        list.Add(ti);
                    }
                }
                return list;
            }
            else //elevatedTrust
            { 
#if SL4
             if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }


               

                List<SqoTypeInfo> list = new List<SqoTypeInfo>();

                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
                foreach (FileInfo f in di.EnumerateFiles("*"+extension))
                {
                    string typeName = f.Name.Replace(extension, "");
                    if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo") || typeName.StartsWith("Sqo.Indexes.BTreeNode"))//engine types
                {
                    continue;
                }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName,useElevatedTrust);

                    SqoTypeInfo ti = seralizer.DeserializeSqoTypeInfo(false);
                    if (ti != null)
                    {
                        list.Add(ti);
                    }
                }
                return list;
#else
                return null; // will never be here
#endif

            }
#elif WinRT
            //List<string> fileFilter = new List<string>();
          //  fileFilter.Add("*");
          //  QueryOptions qo = new QueryOptions();
            //qo.se = fileFilter;
           // qo.UserSearchFilter = extension;
            //StorageFileQueryResult resultQuery = storageFolder.CreateFileQueryWithOptions(qo);
            IReadOnlyList<IStorageFile> files = storageFolder.GetFilesAsync().AsTask().Result;

            List<SqoTypeInfo> list = new List<SqoTypeInfo>();


            foreach (IStorageFile f in files)
            {
                if (f.FileType != extension)
                    continue;
                string typeName = f.Name.Replace(extension, "");
                if (typeName.StartsWith("Sqo.Indexes.IndexInfo2.") || typeName.StartsWith("Sqo.MetaObjects.RawdataInfo."))//engine types
                {
                    continue;
                }
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(storageFolder.Path, typeName, useElevatedTrust);

                SqoTypeInfo ti =  seralizer.DeserializeSqoTypeInfo(false);
                if (ti != null)
                {
                    ti.FileNameForManager = typeName;
                    list.Add(ti);
                }
            }
            return list;
#else
            List<SqoTypeInfo> list = new List<SqoTypeInfo>();

            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(sys_dbs, DatabaseOpenFlags.Create);
                {
                   using (var cursor = transaction.CreateCursor(db))
                    {
                        var current = cursor.MoveNext();

                        while (current.HasValue)
                        {
                            byte[] keyBytes = current.Value.Key;
                            string typeName = ByteConverter.ByteArrayToString(keyBytes);
                            if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo") || typeName.StartsWith("Sqo.Indexes.BTreeNode"))//engine types
                            {
                                current = cursor.MoveNext();
                                continue;
                            }
                            byte[] tiBytes = current.Value.Value;
                            if (tiBytes != null)
                            {
                               list.Add(ObjectSerializer.DeserializeSqoTypeInfoFromBuffer(tiBytes,false));
                            }
                            current = cursor.MoveNext();
                        }
                    }
                   
                }
            }
            return list;
#endif
        }
       
#if ASYNC
        internal async Task<List<SqoTypeInfo>> LoadAllTypesForObjectManagerAsync()
        {
            string extension = ".sqo";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
            }
#if SILVERLIGHT

            if (!this.useElevatedTrust)
            {
                List<SqoTypeInfo> list = new List<SqoTypeInfo>();
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                if (isf.DirectoryExists(path))
                {

                }
                else
                {
                    isf.CreateDirectory(path);
                }
               
                string searchPath = Path.Combine(path, "*"+extension);
                string[] files = isf.GetFileNames(searchPath);

                foreach (string f in files)
                {
                    string typeName = f.Replace(extension, "");
             if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo") || typeName.StartsWith("Sqo.Indexes.BTreeNode"))//engine types
                {
                    continue;
                }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, useElevatedTrust);
                    SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(false).ConfigureAwait(false);
                    if (ti != null)
                    {
                        list.Add(ti);
                    }
                }
                return list;
            }
            else //elevatedTrust
            { 
#if SL4
             if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }


               

                List<SqoTypeInfo> list = new List<SqoTypeInfo>();

                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
                foreach (FileInfo f in di.EnumerateFiles("*"+extension))
                {
                    string typeName = f.Name.Replace(extension, "");
                    if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo") || typeName.StartsWith("Sqo.Indexes.BTreeNode"))//engine types
                {
                    continue;
                }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName,useElevatedTrust);

                    SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(false).ConfigureAwait(false);
                    if (ti != null)
                    {
                        list.Add(ti);
                    }
                }
                return list;
#else
                return null; // will never be here
#endif

            }
#elif WinRT
            //List<string> fileFilter = new List<string>();
           // fileFilter.Add("*");
            //QueryOptions qo = new QueryOptions();
           // qo.UserSearchFilter = extension;
            //StorageFileQueryResult resultQuery = storageFolder.CreateFileQueryWithOptions(qo);
            IReadOnlyList<IStorageFile> files = await storageFolder.GetFilesAsync();

            List<SqoTypeInfo> list = new List<SqoTypeInfo>();


            foreach (IStorageFile f in files)
            {
                if (f.FileType != extension)
                    continue;
                string typeName = f.Name.Replace(extension, "");
                if (typeName.StartsWith("Sqo.Indexes.IndexInfo2.") || typeName.StartsWith("Sqo.MetaObjects.RawdataInfo."))//engine types
                {
                    continue;
                }
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(storageFolder.Path, typeName, useElevatedTrust);

                SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(false).ConfigureAwait(false);
                if (ti != null)
                {
                    ti.FileNameForManager = typeName;
                    list.Add(ti);
                }
            }
            return list;
#else
            List<SqoTypeInfo> list = new List<SqoTypeInfo>();

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
            //TODO: throw exception
            FileInfo[] fi = di.GetFiles("*" + extension);

            foreach (FileInfo f in fi)
            {
                string typeName = f.Name.Replace(extension, "");
                if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo") || typeName.StartsWith("Sqo.Indexes.BTreeNode"))//engine types
                {
                    continue;
                }
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, useElevatedTrust);

                SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(false).ConfigureAwait(false);
                if (ti != null && !ti.TypeName.StartsWith("Sqo.Indexes.BTreeNode"))
                {
                    ti.FileNameForManager = typeName;
                    list.Add(ti);
                }
            }
            return list;
#endif
        }
       
#endif
        internal void LoadAllTypes()
        {
           
			
#if SILVERLIGHT
            string extension = ".sqo";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
            }

            if (!this.useElevatedTrust)
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (isf.DirectoryExists(path))
                {
                    //isf.Remove();
                    //isf = IsolatedStorageFile.GetUserStoreForApplication();

                    //isf.CreateDirectory(path);
                }
                else
                {
                    isf.CreateDirectory(path);
                }
                this.LoadMetaDataTypes();

                string searchPath = Path.Combine(path, "*"+extension);
                string[] files = isf.GetFileNames(searchPath);

                foreach (string f in files)
                {
                    string typeName = f.Replace(extension, "");
                    //System.Reflection.Assembly a = System.Reflection.Assembly.Load(typeName.Split(',')[1]);
                    //Type t = Type.GetType(typeName);
                    if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo"))//engine types
                    {
                        continue;
                    }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, useElevatedTrust);
                    SqoTypeInfo ti = seralizer.DeserializeSqoTypeInfo(true);
                   
                    if (ti != null)
                    {
                        if (this.GetFileByType(ti) != typeName)//check for custom fileName
                        {
                            continue;
                        }

                        this.CompareSchema(ti);
                    }
                   


                }
            }
            else //elevatedTrust
            {
#if SL4
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                this.LoadMetaDataTypes();

                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
                foreach (FileInfo f in di.EnumerateFiles("*"+extension))
                {
                    string typeName = f.Name.Replace(extension, "");
                    if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo"))//engine types
                    {
                        continue;
                    }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, useElevatedTrust);

                    SqoTypeInfo ti = seralizer.DeserializeSqoTypeInfo(true);

               
                    if (ti != null)
                    {
                        if (this.GetFileByType(ti) != typeName)//check for custom fileName
                        {
                            continue;
                        }

                        this.CompareSchema(ti);
                    }
             


                }
#endif

            }
#elif WinRT
            this.LoadMetaDataTypes();

            string extension = ".sqo";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
            }
            //List<string> fileFilter = new List<string>();
            ////fileFilter.Add("*");
            //QueryOptions qo = new QueryOptions();
            //qo.UserSearchFilter = extension;
            //StorageFileQueryResult resultQuery = storageFolder.CreateFileQueryWithOptions(qo);
            IReadOnlyList<StorageFile> files = storageFolder.GetFilesAsync().AsTask().Result;

            List<SqoTypeInfo> listToBuildIndexes = new List<SqoTypeInfo>();
            foreach (StorageFile f in files)
            {
                if (f.FileType != extension)
                    continue;

                string typeName = f.Name.Replace(extension, "");

                //Type t=Type.GetType(typeName);
                if (typeName.StartsWith("indexinfo2.") || typeName.StartsWith("rawdatainfo."))//engine types
                {
                    continue;
                }
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(storageFolder.Path, typeName, this.useElevatedTrust);

                SqoTypeInfo ti = seralizer.DeserializeSqoTypeInfo(true);




                if (ti != null)
                {
                    if (this.GetFileByType(ti) != typeName)//check for custom fileName
                    {
                        continue;
                    }

                    this.CompareSchema(ti);

                }


            }
#else

            if (Directory.Exists(path))
			{
				 this.LoadMetaDataTypes();
                 var transaction=transactionManager.GetActiveTransaction();
                 {
                     var db = transaction.OpenDatabase(sys_dbs, DatabaseOpenFlags.Create);
                     {
                         using (var cursor = transaction.CreateCursor(db))
                         {
                             var current = cursor.MoveNext();

                             while (current.HasValue)
                             {
                                 byte[] keyBytes = current.Value.Key;
                                 string typeName = ByteConverter.ByteArrayToString(keyBytes);
                                 if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo") || typeName.StartsWith("Sqo.Indexes.BTreeNode"))//engine types
                                 {
                                     current = cursor.MoveNext();
                                     continue;
                                 }
                                 byte[] tiBytes = current.Value.Value;
                                 if (tiBytes != null)
                                 {
                                     SqoTypeInfo ti= ObjectSerializer.DeserializeSqoTypeInfoFromBuffer(tiBytes, true);
                                     if (ti != null)
                                     {
                                         this.CompareSchema(ti);
                                     }
                                 }
                                 current = cursor.MoveNext();
                             }
                         }

                     }
                 }
			}
			else
			{ 
				
				throw new SiaqodbException("Invalid folder path!");
			}
#endif


        }
        
#if ASYNC
        internal async Task LoadAllTypesAsync()
        {


#if SILVERLIGHT
            string extension = ".sqo";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
            }

            if (!this.useElevatedTrust)
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (isf.DirectoryExists(path))
                {
                    //isf.Remove();
                    //isf = IsolatedStorageFile.GetUserStoreForApplication();

                    //isf.CreateDirectory(path);
                }
                else
                {
                    isf.CreateDirectory(path);
                }
                await this.LoadMetaDataTypesAsync().ConfigureAwait(false);

                string searchPath = Path.Combine(path, "*"+extension);
                string[] files = isf.GetFileNames(searchPath);

                foreach (string f in files)
                {
                    string typeName = f.Replace(extension, "");
                    //System.Reflection.Assembly a = System.Reflection.Assembly.Load(typeName.Split(',')[1]);
                    //Type t = Type.GetType(typeName);
                    if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo"))//engine types
                    {
                        continue;
                    }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, useElevatedTrust);
                    SqoTypeInfo ti =await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);
                   
                    if (ti != null)
                    {
                        if (this.GetFileByType(ti) != typeName)//check for custom fileName
                        {
                            continue;
                        }

                        await this.CompareSchemaAsync(ti).ConfigureAwait(false);
                    }
                   


                }
            }
            else //elevatedTrust
            {
#if SL4
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                await this.LoadMetaDataTypesAsync().ConfigureAwait(false);

                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
                foreach (FileInfo f in di.EnumerateFiles("*"+extension))
                {
                    string typeName = f.Name.Replace(extension, "");
                    if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo"))//engine types
                    {
                        continue;
                    }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, useElevatedTrust);

                    SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);

               
                    if (ti != null)
                    {
                        if (this.GetFileByType(ti) != typeName)//check for custom fileName
                        {
                            continue;
                        }

                       await this.CompareSchemaAsync(ti).ConfigureAwait(false);
                    }
             


                }
#endif

            }
#elif WinRT
            await this.LoadMetaDataTypesAsync().ConfigureAwait(false);
            string extension = ".sqo";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
            }
           
           // List<string> fileFilter = new List<string>();
            //fileFilter.Add("*");
            //QueryOptions qo = new QueryOptions();
            //qo.UserSearchFilter = extension;
            //StorageFileQueryResult resultQuery = storageFolder.CreateFileQueryWithOptions(qo);
            IReadOnlyList<StorageFile> files = await storageFolder.GetFilesAsync();

            List<SqoTypeInfo> listToBuildIndexes = new List<SqoTypeInfo>();
            foreach (StorageFile f in files)
            {
                if (f.FileType != extension)
                {
                    continue;
                }

                string typeName = f.Name.Replace(extension, "");

                //Type t=Type.GetType(typeName);
                if (typeName.StartsWith("indexinfo2.") || typeName.StartsWith("rawdatainfo."))//engine types
                {
                    continue;
                }
                ObjectSerializer seralizer = SerializerFactory.GetSerializer(storageFolder.Path, typeName, this.useElevatedTrust);

                SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);




                if (ti != null)
                {
                    if (this.GetFileByType(ti) != typeName)//check for custom fileName
                    {
                        continue;
                    }

                    await this.CompareSchemaAsync(ti).ConfigureAwait(false);

                }


            }

#else

            if (Directory.Exists(path))
            {
                await this.LoadMetaDataTypesAsync().ConfigureAwait(false);
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);

                //TODO: throw exception
                string extension = ".sqo";
                if (SiaqodbConfigurator.EncryptedDatabase)
                {
                    extension = ".esqo";
                }
                FileInfo[] fi = di.GetFiles("*" + extension);

                List<SqoTypeInfo> listToBuildIndexes = new List<SqoTypeInfo>();
                foreach (FileInfo f in fi)
                {
                    string typeName = f.Name.Replace(extension, "");

                    //Type t=Type.GetType(typeName);
                    if (typeName.StartsWith("indexinfo") || typeName.StartsWith("rawdatainfo"))//engine types
                    {
                        continue;
                    }
                    ObjectSerializer seralizer = SerializerFactory.GetSerializer(path, typeName, this.useElevatedTrust);
                    SqoTypeInfo ti = await seralizer.DeserializeSqoTypeInfoAsync(true).ConfigureAwait(false);
                    if (ti != null)
                    {
                        if (this.GetFileByType(ti) != typeName)//check for custom fileName
                        {
                            continue;
                        }
                        await this.CompareSchemaAsync(ti).ConfigureAwait(false);
                    }

                }
            }
            else
            {
                throw new SiaqodbException("Invalid folder path!");
            }
#endif

        }
        
#endif
        private void CompareSchema(SqoTypeInfo ti)
        {
            SqoTypeInfo actualType = MetaExtractor.GetSqoTypeInfo(ti.Type);
            if (!MetaExtractor.CompareSqoTypeInfos(actualType, ti))
            {
                ObjectTable table = this.LoadAll(ti);
                
                try
                {
                    actualType.Header.numberOfRecords = ti.Header.numberOfRecords;
                    actualType.Header.TID = ti.Header.TID;

                    this.SaveType(actualType);
                    ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(actualType), useElevatedTrust);
                    serializer.SaveObjectTable(actualType,ti, table,this.rawSerializer);

                    if (this.GetFileByType(actualType) != this.GetFileByType(ti))//< version 3.1 on SL
                    {
                        this.DropType(ti);
                    }
                    metaCache.AddType(actualType.Type, actualType);

                    this.Flush(actualType);
                    
                    
                }
                catch
                {
                    SiaqodbConfigurator.LogMessage("Type:" + ti.Type.ToString() + " cannot be upgraded, will be marked as 'Old'!",VerboseLevel.Error);
                    ti.IsOld = true;
                    this.SaveType(ti);
                    metaCache.AddType(ti.Type, ti);

                    
                }

            }
            else
            {
#if SILVERLIGHT
                //hack
                if (ti.Type==typeof(Indexes.IndexInfo2) && ti.Header.version > -35 )
                {
                    this.SaveType(actualType);
                    metaCache.AddType(actualType.Type, actualType);
                    return;
                }
#endif
                metaCache.AddType(ti.Type, ti);

            }
           
        }
#if ASYNC
        private async Task CompareSchemaAsync(SqoTypeInfo ti)
        {
            SqoTypeInfo actualType = MetaExtractor.GetSqoTypeInfo(ti.Type);
            if (!MetaExtractor.CompareSqoTypeInfos(actualType, ti))
            {
                ObjectTable table = await this.LoadAllAsync(ti).ConfigureAwait(false);
                bool typeWasSaved = true;
                try
                {
                    actualType.Header.numberOfRecords = ti.Header.numberOfRecords;
                    actualType.Header.TID = ti.Header.TID;

                    await this.SaveTypeAsync(actualType).ConfigureAwait(false);
                    ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(actualType), useElevatedTrust);
                    await serializer.SaveObjectTableAsync(actualType, ti, table, this.rawSerializer).ConfigureAwait(false);

                    if (this.GetFileByType(actualType) != this.GetFileByType(ti))//< version 3.1 on SL
                    {
                        await this.DropTypeAsync(ti).ConfigureAwait(false);
                    }
                    metaCache.AddType(actualType.Type, actualType);

                    await this.FlushAsync(actualType).ConfigureAwait(false);


                }
                catch
                {
                    SiaqodbConfigurator.LogMessage("Type:" + ti.Type.ToString() + " cannot be upgraded, will be marked as 'Old'!", VerboseLevel.Error);
                  
                    ti.IsOld = true;
                    typeWasSaved = false;


                }
                if (!typeWasSaved)
                {
                    await this.SaveTypeAsync(ti).ConfigureAwait(false) ;
                    metaCache.AddType(ti.Type, ti);
                }

            }
            else
            {
#if SILVERLIGHT
                //hack
                if (ti.Type==typeof(Indexes.IndexInfo2) && ti.Header.version > -35 )
                {
                    await this.SaveTypeAsync(actualType).ConfigureAwait(false);
                    metaCache.AddType(actualType.Type, actualType);
                    return;
                }
#endif
                metaCache.AddType(ti.Type, ti);

            }

        }
#endif
        internal bool DropType(SqoTypeInfo ti)
        {
            var transaction=transactionManager.GetActiveTransaction();
            {
                string dbname = this.GetFileByType(ti);
                var db = transaction.OpenDatabase(dbname, DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                transaction.DropDatabase(db, true);
                db = transaction.OpenDatabase(dbname, DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                var sysdb = transaction.OpenDatabase(sys_dbs, DatabaseOpenFlags.Create);

                byte[] val = transaction.Get(sysdb, ByteConverter.StringToByteArray(dbname));
                if (val != null)
                {
                    transaction.Delete(sysdb, ByteConverter.StringToByteArray(dbname));
                }

               
                return true;
            }
        }
#if ASYNC
        internal async Task<bool> DropTypeAsync(SqoTypeInfo ti)
        {
            return await this.DropTypeAsync(ti, false).ConfigureAwait(false);
        }
#endif
      
        
#if ASYNC
        internal async Task<bool> DropTypeAsync(SqoTypeInfo ti,bool claimFreeSpace)
        {
            
                if (claimFreeSpace)
                {
                    await this.MarkFreeSpaceAsync(ti).ConfigureAwait(false);
                }
                string fileName = "";
                if (SiaqodbConfigurator.EncryptedDatabase)
                {
                    fileName = this.path + Path.DirectorySeparatorChar + GetFileByType(ti) + ".esqo";
                }
                else
                {
                    fileName = this.path + Path.DirectorySeparatorChar + GetFileByType(ti) + ".sqo";
                }
#if SILVERLIGHT
                if (!this.useElevatedTrust)
                {
                    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                    if (isf.FileExists(fileName))
                    {
                        ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
                        await serializer.CloseAsync().ConfigureAwait(false);

                        try
                        {
                            isf.DeleteFile(fileName);

                            return true;
                        }
                        catch (IsolatedStorageException ex)
                        {
                            throw ex;
                        }
                    }
                }
                else
                {
                    if (File.Exists(fileName))
                    {
                        ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), this.useElevatedTrust);
                        await serializer.CloseAsync().ConfigureAwait(false);
                        File.Delete(fileName);
                        return true;
                    }   
                }
				

#elif MONODROID
			if (File.Exists(fileName))
			{
				ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), this.useElevatedTrust);
				serializer.Close();
				try
				{
					File.Delete(fileName);
					return true;
				}
				catch (UnauthorizedAccessException ex) //monodroid bug!!!:https://bugzilla.novell.com/show_bug.cgi?id=684172
				{
					SiaqodbConfigurator.LogMessage("File:"+fileName+" cannot be deleted,set size to zero!",VerboseLevel.Error);

					serializer.Open(this.useElevatedTrust);
					serializer.MakeEmpty();
					serializer.Close();
					return true;
				}
			}
               
#elif UNITY3D


                if (File.Exists(fileName))
                {
                    ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), this.useElevatedTrust);
                    serializer.Close();
                    try
                    {
                        File.Delete(fileName);
                        return true;
                    }
                    catch (UnauthorizedAccessException ex) //monodroid bug!!!:https://bugzilla.novell.com/show_bug.cgi?id=684172
                    {
                        SiaqodbConfigurator.LogMessage("File:"+fileName+" cannot be deleted,set size to zero!",VerboseLevel.Error);
                  
                        serializer.Open(this.useElevatedTrust);
                        serializer.MakeEmpty();
                        serializer.Close();
                        return true;
                    }
                }
#elif WinRT

                ObjectSerializer serializer = SerializerFactory.GetSerializer(this.storageFolder.Path, GetFileByType(ti), useElevatedTrust);
                serializer.Close();
                StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(this.storageFolder.Path);
                try
                {
                    StorageFile file = await storageFolder.GetFileAsync(Path.GetFileName(fileName));

                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (FileNotFoundException ex)
                {
                    return false;
                }
                return true;

               
#else
                if (File.Exists(fileName))
                {


                    ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), this.useElevatedTrust);
                    await serializer.CloseAsync().ConfigureAwait(false);
                    File.Delete(fileName);
                    return true;


                }


#endif
                return false;
            
        }
        
#endif
        internal SqoTypeInfo GetSqoTypeInfo(string typeName)
        {
            var transaction=transactionManager.GetActiveTransaction();
            {
                var sysdb = transaction.OpenDatabase(sys_dbs, DatabaseOpenFlags.Create);
                {
                    byte[] keyBytes=ByteConverter.StringToByteArray( GetFileByType(typeName));
                    byte[] tiBytes = transaction.Get(sysdb, keyBytes);
                    SqoTypeInfo ti = ObjectSerializer.DeserializeSqoTypeInfoFromBuffer(tiBytes,false);
                    return ti;
                }
                
            }
           
        }

        #endregion
        

        #region TRANSACTIONS
        

        internal byte[] GetObjectBytes(int oid,SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return serializer.ReadObjectBytes(oid, ti);
        }
#if ASYNC
        internal async Task<byte[]> GetObjectBytesAsync(int oid, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return await serializer.ReadObjectBytesAsync(oid, ti).ConfigureAwait(false);
        }
#endif
        #endregion

        #region EVENTS
#if UNITY3D
        protected virtual void OnLoadingObject(LoadingObjectEventArgs args)
        {
            if (loadingObject != null)
            {
                if (args.ObjectType != typeof(Sqo.MetaObjects.RawdataInfo) && args.ObjectType != typeof(Sqo.Indexes.IndexInfo2))
                {
                    loadingObject(this, args);
                }
            }
        }
        protected virtual void OnLoadedObject(int oid,object obj)
        {
            if (loadedObject != null)
            {
                if (obj.GetType() != typeof(Sqo.MetaObjects.RawdataInfo) && obj.GetType() != typeof(Sqo.Indexes.IndexInfo2))
                {
                    LoadedObjectEventArgs args = new LoadedObjectEventArgs(oid, obj);
                    loadedObject(this, args);
                }
            }
        }
        protected void OnNeedSaveComplexObject(ComplexObjectEventArgs args)
        {
            if (needSaveComplexObject != null)
            {
                needSaveComplexObject(this, args);
            }
        }
		#if ASYNC
		protected async Task OnNeedSaveComplexObjectAsync(ComplexObjectEventArgs args)
		{
			if (needSaveComplexObjectAsync != null)
			{
				await needSaveComplexObjectAsync(this, args).ConfigureAwait(false);
			}
		}
		#endif
#else
        protected virtual void OnLoadingObject(LoadingObjectEventArgs args)
        {
            if (LoadingObject != null)
            {
                if (args.ObjectType != typeof(Sqo.MetaObjects.RawdataInfo) )
                {
                    LoadingObject(this, args);
                }
            }
        }
        protected virtual void OnLoadedObject(int oid,object obj)
        {
            if (LoadedObject != null)
            {
                if (obj.GetType() != typeof(Sqo.MetaObjects.RawdataInfo) )
                {
                    LoadedObjectEventArgs args = new LoadedObjectEventArgs(oid, obj);
                    LoadedObject(this, args);
                }
            }
        }
       
        protected void OnNeedSaveComplexObject(ComplexObjectEventArgs args)
        {
            if (NeedSaveComplexObject != null)
            {
                NeedSaveComplexObject(this, args);
            }
        }
#if ASYNC
        protected async Task OnNeedSaveComplexObjectAsync(ComplexObjectEventArgs args)
        {
            if (NeedSaveComplexObjectAsync != null)
            {
                await NeedSaveComplexObjectAsync(this, args).ConfigureAwait(false);
            }
        }
#endif


#endif
#if UNITY3D || CF || MONODROID
#else
        protected  void OnIndexesSaveAsyncFinished(IndexesSaveAsyncFinishedArgs e)
        {
            if (this.IndexesSaveAsyncFinished != null)
            {
                this.IndexesSaveAsyncFinished(this, e);
            }
        }
#endif
        #endregion

        #region OPERATIONS

        internal void Close()
        {
            lock (_syncRoot)
            {
                SerializerFactory.CloseAll();
                rawSerializer.Close();
                transactionManager.Dispose();
                
            }
        }
        #if ASYNC
        internal async Task CloseAsync()
        {
            await SerializerFactory.CloseAllAsync().ConfigureAwait(false);
            await rawSerializer.CloseAsync().ConfigureAwait(false);

        }
#endif
        internal void Flush()
        {

            SerializerFactory.FlushAll();
            this.rawSerializer.Flush();
        }
#if ASYNC
        internal async Task FlushAsync()
        {

            await SerializerFactory.FlushAllAsync().ConfigureAwait(false);
            await this.rawSerializer.FlushAsync().ConfigureAwait(false);
        }
#endif
        internal void Flush(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.Flush();
        }
#if ASYNC
        internal async Task FlushAsync(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            await serializer.FlushAsync().ConfigureAwait(false);
        }
#endif
        




        internal ISqoFile GetRawFile()
        {
            return this.rawSerializer.File;
        }

        internal string GetFileName(SqoTypeInfo ti)
        {
            string fileName = "";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                fileName = this.path + Path.DirectorySeparatorChar + GetFileByType(ti) + ".esqo";
            }
            else
            {
                fileName = this.path + Path.DirectorySeparatorChar + GetFileByType(ti) + ".sqo";
            }
            return fileName;
        }
   
        #endregion
      

        public void Dispose()
        {
            this.transactionManager.Dispose();
        }
    }
    
}
