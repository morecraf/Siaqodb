using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.MetaObjects;
using Sqo.Core;
using System.IO;
using System.Linq.Expressions;
using Sqo.Transactions;
using System.Reflection;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo
{
    public class SiaqodbUtil
    {
     
        private static Dictionary<TypeOidPair<Type, int>, int> migrationCache;
        private static Dictionary<object, int> objectWithOidFieldOldOid;
        private static Dotissi.Siaqodb oldSqo;
        public static void Migrate(Siaqodb siaqodb)
        {
            migrationCache = new Dictionary<TypeOidPair<Type, int>, int>();
            objectWithOidFieldOldOid = new Dictionary<object, int>();
            var path = siaqodb.GetDBPath();

#if WinRT
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.StorageFolder.GetFolderFromPathAsync(path).AsTask().Result;
            oldSqo = new Dotissi.Siaqodb();
            oldSqo.Open(storageFolder);
#else
            System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(path);
            string[] extensions = { "esqr", "sqr", "esqo", "sqo" };
            if (Directory.GetFiles(path, "*.*")
                .Count(f => extensions.Contains(f.Split('.').Last())) <= 0)
            {
                return;
            }
            oldSqo = new Dotissi.Siaqodb(path);
#endif
            siaqodb.CheckIfSavedDelegate = CheckIfSaved;
            siaqodb.UpdateMigrationOid = UpdateMigrationCache;
            var allTypes = oldSqo.GetAllTypesInfo();
           // allTypes.Reverse();
            ITransaction transaction = null;
            try
            {
                transaction = siaqodb.BeginTransaction();
                foreach (var sqoType in allTypes)
                {
                    if (sqoType.Type == typeof(Sqo.MetaObjects.RawdataInfo)
                        || sqoType.TypeName.Contains("Dotissi.Indexes") || sqoType.TypeName.Contains("BTreeNode"))
                    {
                        continue;
                    }
                    var allOfType = oldSqo.LoadAll(sqoType);
                 
                    foreach (var toStore in allOfType)
                    {
                        if (CheckIfSaved(toStore) == -1)
                        {
                            // store the object
                            siaqodb.StoreObject(toStore, transaction);
                            // add the new oids in the micgration cache
                            var sqoTypeInfo = siaqodb.metaCache.GetSqoTypeInfo(toStore.GetType()); ;
                            UpdateMigrationCache(toStore,siaqodb.metaCache.GetOIDOfObject(toStore,sqoTypeInfo));
                        }
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw ex;
            }
            finally{
                oldSqo.Close();
                oldSqo = null;
                migrationCache = null;
                objectWithOidFieldOldOid = null;
            }
        }

        private static void UpdateMigrationCache(object obj,int newOid)
        {
            if(migrationCache == null){
                migrationCache = new Dictionary<TypeOidPair<Type, int>, int>();
            }
            var typeInfo = oldSqo.metaCache.GetSqoTypeInfo(obj.GetType());
            int oldOid = 0;
            if(objectWithOidFieldOldOid != null && objectWithOidFieldOldOid.ContainsKey(obj)){
                oldOid = objectWithOidFieldOldOid[obj];
            }else{
                oldOid = oldSqo.metaCache.GetOIDOfObject(obj, typeInfo);
            }
            
            var key = new TypeOidPair<Type, int>(obj.GetType(),oldOid);
            migrationCache[key] = newOid;
        }

        private static int CheckIfSaved(Type type, int oldOid)
        {
            if(migrationCache == null){
                migrationCache = new Dictionary<TypeOidPair<Type, int>, int>();
            }
            var key = new TypeOidPair<Type,int>(type,oldOid);
            if(migrationCache.ContainsKey(key)){
                return migrationCache[key];
            }
            migrationCache.Add(key,-1);
            return -1;
        }

        internal static int CheckIfSaved(object oldObject)
        {
            if(oldSqo == null){
                return -1;
            }
            var type = oldObject.GetType();
            var typeInfo = oldSqo.metaCache.GetSqoTypeInfo(type);
            
            var oldOid = oldSqo.metaCache.GetOIDOfObject(oldObject,typeInfo);
            var result = CheckIfSaved(oldObject.GetType(),oldOid);
           
            //reset the oid so that the object will be inserted
            var flags = BindingFlags.Instance | BindingFlags.Public;
            PropertyInfo pi = type.GetProperty("OID", flags);
            if (pi != null)
            {
                objectWithOidFieldOldOid[oldObject] = oldOid;
                pi.SetValue(oldObject, 0, null);
            }

            return result;
        }
    }
    class TypeOidPair<T1, T2>
    {
        public TypeOidPair(T1 type, T2 oldOid)
        {
            this.TypeName = type;
            this.Oid = oldOid;
        }
        internal T1 TypeName { get; set; }
        internal T2 Oid { get; set; }

        public override bool Equals(object obj)
        {
            var secondPair = obj as TypeOidPair<T1, T2>;
            if(secondPair == null){
                return false;
            }
            return TypeName.Equals(secondPair.TypeName) && Oid.Equals(secondPair.Oid); 
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + (int) (Oid.GetHashCode() ^ (Oid.GetHashCode() >> 32));
            result = prime * result + (int) (TypeName.GetHashCode() ^ (TypeName.GetHashCode() >> 32));
            return result;
        }
    }
}
