using System;
namespace Sqo
{
    public interface ISiaqodb :IDisposable
    {
        Sqo.Transactions.ITransaction BeginTransaction();
        ISqoQuery<T> Cast<T>();
        void Close();
        int Count<T>();
        void Delete(object obj);
        void Delete(object obj, bool delete_nested);
        void Delete(object obj, Sqo.Transactions.ITransaction transaction);
        void Delete(object obj, Sqo.Transactions.ITransaction transaction, bool delete_nested);
        event EventHandler<DeletedEventsArgs> DeletedObject;
        bool DeleteObjectBy(object obj, params string[] fieldNames);
        bool DeleteObjectBy(object obj, Sqo.Transactions.ITransaction transaction, params string[] fieldNames);
        bool DeleteObjectBy(string fieldName, object obj);
        int DeleteObjectBy(Type objectType, System.Collections.Generic.Dictionary<string, object> criteria);
        int DeleteObjectBy<T>(System.Collections.Generic.Dictionary<string, object> criteria);
        event EventHandler<DeletingEventsArgs> DeletingObject;
        void DropType(Type type);
        void DropType<T>();
        void DropAllTypes();
        

        void Flush();
        System.Collections.Generic.List<MetaType> GetAllTypes();
        string GetDBPath();
        int GetOID(object obj);
        IObjectList<T> LoadAll<T>();
        IObjectList<T> LoadAllLazy<T>();
        System.Collections.Generic.List<int> LoadAllOIDs(MetaType type);
        event EventHandler<LoadedObjectEventArgs> LoadedObject;
         event EventHandler<LoadingObjectEventArgs> LoadingObject;
        T LoadObjectByOID<T>(int oid);
        System.Collections.Generic.List<int> LoadOids<T>(System.Linq.Expressions.Expression expression);
        object LoadValue(int oid, string fieldName, MetaType mt);
        ISqoQuery<T> Query<T>();
        event EventHandler<SavedEventsArgs> SavedObject;
        event EventHandler<SavingEventsArgs> SavingObject;

        void StoreObject(object obj);
        void StoreObject(object obj, Sqo.Transactions.ITransaction transaction);
        void StoreObjectPartially(object obj, params string[] properties);
        void StoreObjectPartially(object obj, bool onlyReferences, params string[] properties);
        bool UpdateObjectBy(object obj, params string[] fieldNames);
        bool UpdateObjectBy(object obj, Sqo.Transactions.ITransaction transaction, params string[] fieldNames);
        bool UpdateObjectBy(string fieldName, object obj);

#if ASYNC
        System.Threading.Tasks.Task CloseAsync();
        System.Threading.Tasks.Task<int> CountAsync<T>();
        System.Threading.Tasks.Task DeleteAsync(object obj);
        System.Threading.Tasks.Task DeleteAsync(object obj, Sqo.Transactions.ITransaction transaction);
        System.Threading.Tasks.Task<bool> DeleteObjectByAsync(object obj, params string[] fieldNames);
        System.Threading.Tasks.Task<bool> DeleteObjectByAsync(object obj, Sqo.Transactions.ITransaction transaction, params string[] fieldNames);
        System.Threading.Tasks.Task<bool> DeleteObjectByAsync(string fieldName, object obj);
        System.Threading.Tasks.Task<int> DeleteObjectByAsync(Type objectType, System.Collections.Generic.Dictionary<string, object> criteria);
        System.Threading.Tasks.Task<int> DeleteObjectByAsync<T>(System.Collections.Generic.Dictionary<string, object> criteria);
        System.Threading.Tasks.Task DropTypeAsync(Type type);
        System.Threading.Tasks.Task DropTypeAsync<T>();
        System.Threading.Tasks.Task FlushAsync();
        System.Threading.Tasks.Task<System.Collections.Generic.List<MetaType>> GetAllTypesAsync();
        System.Threading.Tasks.Task<IObjectList<T>> LoadAllAsync<T>();
        System.Threading.Tasks.Task<IObjectList<T>> LoadAllLazyAsync<T>();
        System.Threading.Tasks.Task<System.Collections.Generic.List<int>> LoadAllOIDsAsync(MetaType type);
        System.Threading.Tasks.Task<T> LoadObjectByOIDAsync<T>(int oid);
        System.Threading.Tasks.Task<System.Collections.Generic.List<int>> LoadOidsAsync<T>(System.Linq.Expressions.Expression expression);
        System.Threading.Tasks.Task<object> LoadValueAsync(int oid, string fieldName, MetaType mt);
       System.Threading.Tasks.Task StoreObjectAsync(object obj);
        System.Threading.Tasks.Task StoreObjectAsync(object obj, Sqo.Transactions.ITransaction transaction);
        System.Threading.Tasks.Task StoreObjectPartiallyAsync(object obj, params string[] properties);
        System.Threading.Tasks.Task StoreObjectPartiallyAsync(object obj, bool onlyReferences, params string[] properties);
        System.Threading.Tasks.Task<bool> UpdateObjectByAsync(object obj, params string[] fieldNames);
        System.Threading.Tasks.Task<bool> UpdateObjectByAsync(object obj, Sqo.Transactions.ITransaction transaction, params string[] fieldNames);
        System.Threading.Tasks.Task<bool> UpdateObjectByAsync(string fieldName, object obj);

#endif

    }
}
