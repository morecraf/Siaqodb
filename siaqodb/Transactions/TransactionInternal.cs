using System;
using System.Collections.Generic;
using Sqo.Core;
using Sqo.Meta;
using LightningDB;

namespace Sqo.Transactions
{
    class TransactionInternal
    {
        internal Transaction transaction;
        internal LightningTransaction lmdbTransaction;
        public TransactionInternal(Transaction sTransaction,LightningDB.LightningTransaction lmdbTransaction)
        {
            this.transaction = sTransaction;
            this.lmdbTransaction = lmdbTransaction;
        }
       

    }
}
