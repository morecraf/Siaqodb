using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Exceptions;
using System.Threading.Tasks;
#if  ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Sqo.Transactions
{
    public class Transaction:ITransaction
    {
        internal Guid ID;
        internal TransactionStatus status;
        TransactionManager transactionManager;
        
        internal Transaction(TransactionManager manager)
        {
            ID = Guid.NewGuid();
            this.transactionManager = manager;
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
            transactionManager.CommitTransaction(this.ID);

        }
#if ASYNC_LMDB
        /// <summary>
        /// Commit transaction to database
        /// </summary>
        public Task CommitAsync()
        {

            return Task.Factory.StartNew(() =>
            {
                this.Commit();

            });


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
            transactionManager.RollbackTransaction(this.ID);
        }
#if ASYNC_LMDB
        /// <summary>
        /// Rollback changes
        /// </summary>
        public Task RollbackAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                this.Rollback();

            });
        }
#endif



        public void Dispose()
        {
            transactionManager.RollbackTransaction(this.ID);
        }
    }
    internal enum TransactionStatus { Open, Closed };
}
