using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo.Indexes
{
    interface IBTree
    {
        void AddItem(object new_key, int[] new_value);
        int[] FindItem(object target_key);
        List<int> FindItemsLessThan(object target_key);
        List<int> FindItemsLessThanOrEqual(object target_key);
        List<int> FindItemsBiggerThan(object target_key);
        List<int> FindItemsBiggerThanOrEqual(object target_key);
        List<int> FindItemsStartsWith(object target_key,bool defaultComparer, StringComparison stringComparison);
        void RemoveItem(object target_key);
        void RemoveOid(object target_key,int oid);
        void SetRoot(object rootNode);
        int GetRootOid();
        void Persist();
        void SetIndexInfo(IndexInfo2 indexInfo);
        
        void Drop(bool withAllNodes);
        void AllowPersistance(bool allow);
#if ASYNC
        Task AddItemAsync(object new_key, int[] new_value);
        Task<int[]> FindItemAsync(object target_key);
        Task<List<int>> FindItemsLessThanAsync(object target_key);
        Task<List<int>> FindItemsLessThanOrEqualAsync(object target_key);
        Task<List<int>> FindItemsBiggerThanAsync(object target_key);
        Task<List<int>> FindItemsBiggerThanOrEqualAsync(object target_key);
        Task<List<int>> FindItemsStartsWithAsync(object target_key,bool defaultComparer,StringComparison stringComparison);
        Task RemoveOidAsync(object target_key, int oid);
        Task PersistAsync();
        Task DropAsync(bool withAllNodes);
       
#endif
    }
    interface IBTree<T>
    {
        void AddItem(T new_key, int[] new_value);
        int[] FindItem(T target_key);
        List<int> FindItemsLessThan(T target_key);
        List<int> FindItemsLessThanOrEqual(T target_key);
        List<int> FindItemsBiggerThan(T target_key);
        List<int> FindItemsBiggerThanOrEqual(T target_key);
        List<int> FindItemsStartsWith(T target_key, bool defaultComparer, StringComparison stringComparison);
        void RemoveItem(T target_key);
        List<T> DumpKeys();
      
        
    }
}