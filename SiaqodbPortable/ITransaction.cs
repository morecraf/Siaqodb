using System;
using System.Threading.Tasks;
using Sqo;
namespace Sqo.Transactions
{
    public interface ITransaction
    {
        void Commit();
        System.Collections.Generic.IList<T> GetUnCommittedObjects<T>();
        System.Collections.Generic.IList<T> GetUnCommittedObjects<T>(bool includeDeletes);
        void Rollback();
#if ASYNC
        Task CommitAsync();
        Task RollbackAsync();
#endif
    }
}
