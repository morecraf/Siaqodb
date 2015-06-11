using System;
using System.Collections.Generic;
using Dotissi.Core;
using Dotissi.Meta;

namespace Dotissi.Transactions
{
    class TransactionInternal
    {
        internal Transaction transaction;
        internal Dictionary<ObjectSerializer, KeyValuePair<SqoTypeInfo, int>> nrRecordsBeforeCommit = new Dictionary<ObjectSerializer, KeyValuePair<SqoTypeInfo, int>>();
        internal Siaqodb siaqodbInstance;
        internal List<TransactionObject> transactionObjects = new List<TransactionObject>();
        internal List<SqoTypeInfo> tiInvolvedInTransaction = new List<SqoTypeInfo>();
        public TransactionInternal(Transaction tr,Siaqodb siaqodb)
        {
            transaction = tr;
            siaqodbInstance = siaqodb;
        }
        public void AddTransactionObject(TransactionObject trObj)
        {
            transactionObjects.Add(trObj);
            if (!tiInvolvedInTransaction.Contains(trObj.objInfo.SqoTypeInfo))
            {
                tiInvolvedInTransaction.Add(trObj.objInfo.SqoTypeInfo);
            }
        }

    }
}
