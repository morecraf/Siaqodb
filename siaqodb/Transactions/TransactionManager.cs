using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using Sqo.Core;
using Sqo.Exceptions;
using LightningDB;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Transactions
{
    class TransactionManager:IDisposable
    {
        LightningEnvironment env;
        const int OneMega = 1024 * 1024;
       
        public TransactionManager(string path)
        {
            this.env = new LightningEnvironment(path, EnvironmentOpenFlags.None);

            env.MapSize = 200 * OneMega;
            env.MaxDatabases = 200;

            env.Open();
        }
#if UNITY3D
        internal  Dictionary<Guid, TransactionInternal> transactions = new Dictionary<Guid, TransactionInternal>(new Sqo.Utilities.EqualityComparer<Guid>());
#else
        internal  Dictionary<Guid, TransactionInternal> transactions = new Dictionary<Guid, TransactionInternal>();
#endif

        public  readonly object _SyncRoot = new object();

        public Transaction BeginTransaction()
        {
            lock (_SyncRoot)
            {
                if (transactions.Count > 0)
                    throw new SiaqodbException("There is an active transactions, Commit or Abort it first");
                LightningDB.LightningTransaction transaction = env.BeginTransaction();
                Transaction trans = new Transaction(this);
                TransactionInternal trInt = new TransactionInternal(trans, transaction);

                transactions.Add(trans.ID, trInt);

                return trans;
            }

        }
        public Transaction GetActiveTransaction(out bool started)
        {
            lock (_SyncRoot)
            {
                started = false;
                if (transactions.Count != 1)
                {
                    started = true;
                    return this.BeginTransaction();
                }
                else
                {

                    foreach (Guid trId in transactions.Keys)
                    {
                        return transactions[trId].transaction;
                    }
                }
                return null;
            }
        }
        public LightningTransaction GetActiveTransaction()
        {
            lock (_SyncRoot)
            { 
                if(transactions.Count!=1)
                    throw new SiaqodbException("There is no active transaction!");
                foreach (Guid trId in transactions.Keys)
                {
                    return transactions[trId].lmdbTransaction;
                }
            }
            return null;
        }

        internal void CommitTransaction(Guid id)
        {


            lock (_SyncRoot)
            {
                TransactionInternal transactionInternal = transactions[id];

                transactionInternal.lmdbTransaction.Commit();
                transactionInternal.transaction.status = TransactionStatus.Closed;
                transactions.Remove(id);
            }


        }


        internal  void RollbackTransaction(Guid id)
        {
            lock (_SyncRoot)
            {
                if (transactions.ContainsKey(id))
                {
                    TransactionInternal transactionInternal = transactions[id];
                    try
                    {
                        transactionInternal.lmdbTransaction.Abort();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        transactions.Remove(id);
                        transactionInternal.transaction.status = TransactionStatus.Closed;
                    }
                }

            }
        }




        public void Dispose()
        {
            this.env.Dispose();
        }
    }
}

