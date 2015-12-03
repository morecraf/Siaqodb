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
       
        public TransactionManager(string path,long maxSize,int maxDbs)
        {
			#if MONODROID
			this.env = new LightningEnvironment(path, EnvironmentOpenFlags.None);
			#else
			this.env = new LightningEnvironment(path, EnvironmentOpenFlags.NoLock);
			#endif
            env.MapSize = maxSize;
            env.MaxDatabases = maxDbs;
            
            env.Open();
            decimal occupiedPercent = (decimal)this.EnvUsedSize() * 100 / (decimal)this.EnvMaxSize();
            long tempMaxSize = this.EnvMaxSize();
            if (occupiedPercent > SiaqodbConfigurator.AutoGrowthThresholdPercent)
            {
                //resize
                env.Close();
#if MONODROID
			    this.env = new LightningEnvironment(path, EnvironmentOpenFlags.None);
#else
                this.env = new LightningEnvironment(path, EnvironmentOpenFlags.NoLock);
#endif
                env.MapSize = tempMaxSize + SiaqodbConfigurator.AutoGrowthSize;
                env.MaxDatabases = maxDbs;

                env.Open();

            }
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


        public long EnvUsedSize()
        {
            return env.UsedSize;
        }
        public long EnvMaxSize()
        {
            return env.MapSize;
        }
        public int EnvMaxDatabases()
        {
            return env.MaxDatabases;
        }
        public void Dispose()
        {
            this.env.Dispose();
        }
    }
}

