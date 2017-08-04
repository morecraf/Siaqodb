using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Exceptions;

#if  ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Sqo.Transactions
{
    /// <summary>
    /// Siaqodb uses transactions internally for each call to StoreObject and StoreObjectPartially.
    /// This makes each call commit records and index updates to the database.
    /// This is normally not a problem, however if you application needs to update many records at once, 
    /// then a transaction will improve your performace considerably.
    /// </summary>
    public class Transaction:ITransaction
    {
        public Guid ID { get; set; }
        internal TransactionStatus status;
        public string Name { get; set; }
        TransactionManager transactionManager;
        
        internal Transaction(TransactionManager manager)
        {
            ID = Guid.NewGuid();
            Name = ID.ToString();
            this.transactionManager = manager;
        }

        internal Transaction(TransactionManager manager, string name)
        {
            ID = Guid.NewGuid();
            Name = name;
            this.transactionManager = manager;
        }

        /// <summary>
        /// Commit transaction to database
        /// </summary>
        public void Commit()
        {
            if (this.status == TransactionStatus.Closed)
            {
                throw new SiaqodbException("Transaction already closed", null, Name, ID);
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
                throw new SiaqodbException("Transaction already closed", null, Name, ID);
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
