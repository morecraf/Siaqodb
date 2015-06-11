using Sqo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Dotissi.Transactions
{
    public class Transaction:Sqo.Transactions.ITransaction
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
#if ASYNC_LMDB
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
#if ASYNC_LMDB
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
        /// <summary>
        /// Load un-committen objects,except deleted instances
        /// </summary>
        /// <typeparam name="T">Type of un-committen objects</typeparam>
        /// <returns></returns>
        public IList<T> GetUnCommittedObjects<T>()
        {
            if (this.status == TransactionStatus.Closed)
            {
                throw new SiaqodbException("Transaction closed");
            }
            IList<Transactions.TransactionObject> instances = Transactions.TransactionManager.transactions[this.ID].transactionObjects.Where(
                trObj => trObj.Operation == Transactions.TransactionObject.OperationType.InsertOrUpdate && trObj.currentObject.GetType() == typeof(T)).ToList();
            List<T> uncommittedInstances = new List<T>();
            foreach (Transactions.TransactionObject trObj in instances)
            {
                uncommittedInstances.Add((T)trObj.currentObject);
            }
            return uncommittedInstances;

        }
        /// <summary>
        /// Load un-committen objects
        /// </summary>
        /// <typeparam name="T">Type of un-committen objects</typeparam>
        /// <param name="includeDeletes">If true, will be returned also deleted objects within the transaction</param>
        /// <returns></returns>
        public IList<T> GetUnCommittedObjects<T>(bool includeDeletes)
        {
            if (this.status == TransactionStatus.Closed)
            {
                throw new SiaqodbException("Transaction closed");
            }
            IList<Transactions.TransactionObject> instances = null;
            if (includeDeletes)
            {
                instances = Transactions.TransactionManager.transactions[this.ID].transactionObjects.Where(
                trObj => trObj.currentObject.GetType() == typeof(T)).ToList();
            }
            else
            {
                instances = Transactions.TransactionManager.transactions[this.ID].transactionObjects.Where(
                  trObj => trObj.Operation == Transactions.TransactionObject.OperationType.InsertOrUpdate && trObj.currentObject.GetType() == typeof(T)).ToList();
            
            }
            List<T> uncommittedInstances = new List<T>();
            foreach (Transactions.TransactionObject trObj in instances)
            {
                uncommittedInstances.Add((T)trObj.currentObject);
            }
            return uncommittedInstances;

        }


        public void Dispose()
        {
           
        }
    }
    internal enum TransactionStatus { Open, Closed };
}
