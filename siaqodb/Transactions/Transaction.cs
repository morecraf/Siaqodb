using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Exceptions;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Transactions
{
    public class Transaction:ITransaction
    {
        internal Guid ID;
        internal TransactionStatus status;
        
        internal Transaction()
        {
            ID = Guid.NewGuid();
        }
        /// <summary>
        /// Commit transaction to database
        /// </summary>
        public void Commit()
        {
            if (this.status == TransactionStatus.Closed)
            {
                throw new SiaqodbException("Transaction closed");
            }
            TransactionManager.CommitTransaction(this.ID);

        }
#if ASYNC
        /// <summary>
        /// Commit transaction to database
        /// </summary>
        public async Task CommitAsync()
        {
            if (this.status == TransactionStatus.Closed)
            {
                throw new SiaqodbException("Transaction closed");
            }
            await TransactionManager.CommitTransactionAsync(this.ID);

        }
#endif

        /// <summary>
        /// Rollback changes
        /// </summary>
        public void Rollback()
        {
            if (this.status == TransactionStatus.Closed)
            {
                throw new SiaqodbException("Transaction closed");
            }
            TransactionManager.RollbackTransaction(this.ID);
        }
#if ASYNC
        /// <summary>
        /// Rollback changes
        /// </summary>
        public async Task RollbackAsync()
        {
            if (this.status == TransactionStatus.Closed)
            {
                throw new SiaqodbException("Transaction closed");
            }
            await TransactionManager.RollbackTransactionAsync(this.ID);
        }
#endif
        

    }
    internal enum TransactionStatus { Open, Closed };
}
