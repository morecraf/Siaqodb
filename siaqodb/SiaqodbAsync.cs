using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo
{
    public partial class Siaqodb : Sqo.ISiaqodb
    {

        public Task OpenAsync(string path)
        {
            return Task.Factory.StartNew(() =>
            {
                this.Open(path);

            });
        }
        public Task CloseAsync()
        {
            return Task.Factory.StartNew(() =>
            {
               this.Close();

            });
        }

        public Task<int> CountAsync<T>()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.Count<T>();

            });
        }

        public Task DeleteAsync(object obj)
        {
            return Task.Factory.StartNew(() =>
            {
                this.Delete(obj);

            });
        }

        public Task DeleteAsync(object obj, Transactions.ITransaction transaction)
        {
            return Task.Factory.StartNew(() =>
            {
                this.Delete(obj,transaction);

            });
        }

        public Task<bool> DeleteObjectByAsync(object obj, params string[] fieldNames)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.DeleteObjectBy(obj,fieldNames);

            });
        }

        public Task<bool> DeleteObjectByAsync(object obj, Transactions.ITransaction transaction, params string[] fieldNames)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.DeleteObjectBy(obj,transaction, fieldNames);

            });
        }

        public Task<bool> DeleteObjectByAsync(string fieldName, object obj)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.DeleteObjectBy(fieldName,obj);

            });
        }

        public Task<int> DeleteObjectByAsync(Type objectType, Dictionary<string, object> criteria)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.DeleteObjectBy(objectType, criteria);

            });
        }

        public Task<int> DeleteObjectByAsync<T>(Dictionary<string, object> criteria)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.DeleteObjectBy<T>(criteria);

            });
        }

        public Task DropTypeAsync(Type type)
        {
            return Task.Factory.StartNew(() =>
            {
                this.DropType(type);

            });
        }

       

        public Task DropTypeAsync<T>()
        {
             return Task.Factory.StartNew(() =>
            {
                this.DropType<T>();
            });
        }

       

        public Task FlushAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                this.Flush();
            });
        }

        public Task<List<MetaType>> GetAllTypesAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.GetAllTypes();

            });
        }

        public Task<IObjectList<T>> LoadAllAsync<T>()
        {
              return Task.Factory.StartNew(() => {
                  return this.LoadAll<T>();
            
            });
        }

        public Task<IObjectList<T>> LoadAllLazyAsync<T>()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadAllLazy<T>();

            });
        }

        public Task<List<int>> LoadAllOIDsAsync(MetaType type)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadAllOIDs(type);

            });
        }
        internal Task<List<int>> LoadAllOIDsAsync<T>()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadAllOIDs<T>();

            });
        }
        internal Task<IObjectList<T>> LoadAsync<T>(System.Linq.Expressions.Expression expression)
        {

            return Task.Factory.StartNew(() =>
            {
                return this.Load<T>(expression);

            });
        }
        internal Task<object> LoadValueAsync(int oid, string fieldName, Type type)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadValue(oid,fieldName,type);

            });
        }
        public Task<T> LoadObjectByOIDAsync<T>(int oid)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadObjectByOID<T>(oid);

            });
        }
        public Task<T> LoadObjectByOIDAsync<T>(int oid, List<string> properties)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadObjectByOID<T>(oid,properties);

            });
        }
        internal Task<object> LoadObjectByOIDAsync(Type t,int oid)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadObjectByOID(t,oid);

            });
        }

        public Task<List<int>> LoadOidsAsync<T>(System.Linq.Expressions.Expression expression)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadOids<T>(expression);

            });
        }

        public Task<object> LoadValueAsync(int oid, string fieldName, MetaType mt)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LoadValue(oid,fieldName,mt);

            });
        }


        public Task StoreObjectAsync(object obj)
        {
            return Task.Factory.StartNew(() =>
            {
                this.StoreObject(obj);

            });
        }

        public Task StoreObjectAsync(object obj, Transactions.ITransaction transaction)
        {
            return Task.Factory.StartNew(() =>
            {
                this.StoreObject(obj, transaction);

            });
        }

        public Task StoreObjectPartiallyAsync(object obj, params string[] properties)
        {
            return Task.Factory.StartNew(() =>
            {
                this.StoreObjectPartially(obj,properties);

            });
        }

        public Task StoreObjectPartiallyAsync(object obj, bool onlyReferences, params string[] properties)
        {
            return Task.Factory.StartNew(() =>
            {
                this.StoreObjectPartially(obj,onlyReferences, properties);

            });
        }

        public Task StoreObjectPartiallyAsync(object obj, Transactions.ITransaction transaction, params string[] properties)
        {
            return Task.Factory.StartNew(() =>
            {
                this.StoreObjectPartially(obj, transaction, properties);

            });
        }

        public Task StoreObjectPartiallyAsync(object obj, bool onlyReferences, Transactions.ITransaction transaction, params string[] properties)
        {
            return Task.Factory.StartNew(() =>
            {
                this.StoreObjectPartially(obj, onlyReferences, transaction, properties);

            });
        }

        public Task<bool> UpdateObjectByAsync(object obj, params string[] fieldNames)
        {
            return Task.Factory.StartNew(() =>
            {
               return this.UpdateObjectBy(obj, fieldNames);

            });
        }

        public Task<bool> UpdateObjectByAsync(object obj, Transactions.ITransaction transaction, params string[] fieldNames)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.UpdateObjectBy(obj,transaction, fieldNames);

            });
        }

        public Task<bool> UpdateObjectByAsync(string fieldName, object obj)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.UpdateObjectBy(fieldName, obj);

            });
        }
    }
}
