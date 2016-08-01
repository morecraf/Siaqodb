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


#if SILVERLIGHT
	using System.IO.IsolatedStorage;
#endif


namespace Sqo
{
    [Obfuscation(Feature = "Apply to member * when event: renaming", Exclude = true)]
    partial class StorageEngine :IDisposable
    {
        #region VAR DECLARATIONS


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
		
#else
        public event EventHandler<LoadingObjectEventArgs> LoadingObject;
        public event EventHandler<LoadedObjectEventArgs> LoadedObject;
        public event EventHandler<ComplexObjectEventArgs> NeedSaveComplexObject;

        
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
       


        internal void LoadAllTypes()
        {
            var transaction = transactionManager.GetActiveTransaction();
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

                            byte[] tiBytes = current.Value.Value;
                            if (tiBytes != null)
                            {
                                SqoTypeInfo ti = ObjectSerializer.DeserializeSqoTypeInfoFromBuffer(tiBytes, true);
                                if (ti != null)
                                {
                                    this.CompareSchema(ti,transaction);
                                }
                            }
                            current = cursor.MoveNext();
                        }
                    }
                }
            }
        }


        private void CompareSchema(SqoTypeInfo ti,LightningTransaction transaction)
        {
            SqoTypeInfo actualType = MetaExtractor.GetSqoTypeInfo(ti.Type);
            if (!MetaExtractor.CompareSqoTypeInfos(actualType, ti))
            {
                ObjectTable table = this.LoadAll(ti,transaction);
                
                try
                {
                    actualType.Header.numberOfRecords = ti.Header.numberOfRecords;
                    actualType.Header.TID = ti.Header.TID;

                    this.SaveType(actualType);
                    ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(actualType), useElevatedTrust);

                    string dbname = this.GetFileByType(ti);
                    var db = transaction.OpenDatabase(dbname, DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                    serializer.SaveObjectTable(actualType,ti, table,this.rawSerializer,db,transaction);

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
        

      
        #region EVENTS
#if UNITY3D
        protected virtual void OnLoadingObject(LoadingObjectEventArgs args)
        {
			if (loadingObject != null) {
                
				loadingObject (this, args);
                
			}
        }
        protected virtual void OnLoadedObject(int oid,object obj)
        {
			if (loadedObject != null) {
               
				LoadedObjectEventArgs args = new LoadedObjectEventArgs (oid, obj);
				loadedObject (this, args);
                
			}
        }
        protected void OnNeedSaveComplexObject(ComplexObjectEventArgs args)
        {
            if (needSaveComplexObject != null)
            {
                needSaveComplexObject(this, args);
            }
        }
	
#else
        protected virtual void OnLoadingObject(LoadingObjectEventArgs args)
        {
            if (LoadingObject != null)
            {

                LoadingObject(this, args);

            }
        }
        protected virtual void OnLoadedObject(int oid,object obj)
        {
            if (LoadedObject != null)
            {

                LoadedObjectEventArgs args = new LoadedObjectEventArgs(oid, obj);
                LoadedObject(this, args);

            }
        }
       
        protected void OnNeedSaveComplexObject(ComplexObjectEventArgs args)
        {
            if (NeedSaveComplexObject != null)
            {
                NeedSaveComplexObject(this, args);
            }
        }



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
        
        internal void Flush()
        {

            SerializerFactory.FlushAll();
            this.rawSerializer.Flush();
        }

        internal void Flush(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.Flush();
        }


        
        #endregion
      

        public void Dispose()
        {
            this.transactionManager.Dispose();
        }
    }
    
}
