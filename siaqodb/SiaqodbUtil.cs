using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.MetaObjects;
using Sqo.Core;
using System.IO;
using System.Linq.Expressions;
using Sqo.Transactions;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo
{
    public class SiaqodbUtil
    {
#if WinRT
        public static void Migrate(Siaqodb siaqodb)
        {
            var path = siaqodb.GetDBPath();
           
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.StorageFolder.GetFolderFromPathAsync(path).AsTask().Result;
            IReadOnlyList<Windows.Storage.StorageFile> files = storageFolder.GetFilesAsync().AsTask().Result;

            var oldSqo = new Dotissi.Siaqodb();
            oldSqo.Open(storageFolder);

            var allTypes = oldSqo.GetAllTypesInfo();
            foreach (var sqoType in allTypes)
            {
                if (sqoType.Type == typeof(Sqo.MetaObjects.RawdataInfo)
                    || sqoType.TypeName.Contains("Dotissi.Indexes") || sqoType.TypeName.Contains("BTreeNode"))
                {
                    continue;
                }
                var allOfType = oldSqo.LoadAll(sqoType);
                ITransaction transaction = null;
                try
                {
                    transaction = siaqodb.BeginTransaction();
                    foreach (var toStore in allOfType)
                    {
                        siaqodb.StoreObject(toStore, transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                }
            }
            oldSqo.Close();


        }
#else
       public static void Migrate(Siaqodb siaqodb)
        {
            var path = siaqodb.GetDBPath();
            System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(path);
            string[] extensions = { "esqr", "sqr", "esqo", "sqo" };
            if (Directory.GetFiles(path, "*.*")
                .Count(f => extensions.Contains(f.Split('.').Last())) <= 0)
            {
                return;
            }


            var oldSqo = new Dotissi.Siaqodb(path);
            var allTypes = oldSqo.GetAllTypesInfo();
            foreach (var sqoType in allTypes)
            {
                if (sqoType.Type == typeof(Sqo.MetaObjects.RawdataInfo)
                    || sqoType.TypeName.Contains("Dotissi.Indexes") || sqoType.TypeName.Contains("BTreeNode"))
                {
                    continue;
                }
                var allOfType = oldSqo.LoadAll(sqoType);
                ITransaction transaction = null;
                try
                {
                    transaction = siaqodb.BeginTransaction();
                    foreach (var toStore in allOfType)
                    {
                        siaqodb.StoreObject(toStore, transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                }
            }
            oldSqo.Close();


        }
#endif
    }
        
   
}
