using System;
#if ASYNC
using System.Threading.Tasks;
#endif
using Sqo;
namespace Sqo.Transactions
{
    public interface ITransaction:IDisposable
    {
        void Commit();
        void Rollback();
#if ASYNC
        Task CommitAsync();
        Task RollbackAsync();
#endif
    }
}
