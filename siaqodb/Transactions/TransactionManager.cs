using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using Sqo.Core;
using Sqo.Exceptions;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Transactions
{
    static class TransactionManager
    {
#if UNITY3D
        internal static Dictionary<Guid, TransactionInternal> transactions = new Dictionary<Guid, TransactionInternal>(new Sqo.Utilities.EqualityComparer<Guid>());
#else
        internal static Dictionary<Guid, TransactionInternal> transactions = new Dictionary<Guid, TransactionInternal>();
#endif

        public static readonly object _SyncRoot = new object();

        public static Transaction BeginTransaction(LightningDB.LightningTransaction transaction)
        {
            lock (_SyncRoot)
            {
                Transaction trans = new Transaction();
                TransactionInternal trInt = new TransactionInternal(trans, transaction);

                transactions.Add(trans.ID, trInt);

                return trans;
            }

        }

        internal static void CommitTransaction(Guid id)
        {


            lock (_SyncRoot)
            {
                TransactionInternal transactionInternal = transactions[id];

                transactionInternal.lmdbTransaction.Commit();
                transactionInternal.transaction.status = TransactionStatus.Closed;
                transactions.Remove(id);
            }


        }


        internal static void RollbackTransaction(Guid id)
        {
            lock (_SyncRoot)
            {
                TransactionInternal transactionInternal = transactions[id];
                transactionInternal.lmdbTransaction.Abort();
                transactions.Remove(id);
                transactionInternal.transaction.status = TransactionStatus.Closed;

            }
        }
        internal static LightningDB.LightningTransaction GetLMDBTransaction(Guid id)
        {
            return transactions[id].lmdbTransaction;
        }


    }
}

