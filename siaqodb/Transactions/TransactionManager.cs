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
#if ASYNC
        private static readonly AsyncLock _locker = new AsyncLock();
#endif
        public static Transaction BeginTransaction(Siaqodb siaqodb)
        {
            Transaction trans = new Transaction();
            TransactionInternal trInt = new TransactionInternal(trans, siaqodb);

            transactions.Add(trans.ID, trInt);

            return trans;

        }
       
        internal static void CommitTransaction(Guid id)
        {
            lock (_SyncRoot)
            {
                
                
                TransactionInternal transactionInternal = transactions[id];
                
                TransactionObjectHeader lastHeader=null;
                TransactionsStorage transactStorage=transactionInternal.siaqodbInstance.GetTransactionLogStorage();
                foreach (SqoTypeInfo ti in transactionInternal.tiInvolvedInTransaction)
                {
                    transactionInternal.siaqodbInstance.PutIndexPersiststenceState(ti, false);
                }
                transactionInternal.siaqodbInstance.TransactionCommitStatus(true);
                try
                {
                    foreach (TransactionObject trObj in transactionInternal.transactionObjects)
                    {
                        if (!transactionInternal.nrRecordsBeforeCommit.ContainsKey(trObj.serializer))
                        {
                            transactionInternal.nrRecordsBeforeCommit.Add(trObj.serializer, new KeyValuePair<SqoTypeInfo, int>(trObj.objInfo.SqoTypeInfo, trObj.objInfo.SqoTypeInfo.Header.numberOfRecords));
                            TransactionTypeHeader tHeader = new TransactionTypeHeader();
                            tHeader.NumberOfRecords = trObj.objInfo.SqoTypeInfo.Header.numberOfRecords;
                            tHeader.TypeName = trObj.objInfo.SqoTypeInfo.TypeName;
                            transactionInternal.siaqodbInstance.StoreObject(tHeader);
                            transactionInternal.siaqodbInstance.Flush<TransactionTypeHeader>();
                        }


                        TransactionObjectHeader header = trObj.PreCommit();
                        if (header != null)
                        {
                            if (lastHeader != null)
                            {
                                header.Position = lastHeader.Position + lastHeader.BatchSize;
                            }
                            SaveObjectForCrashRollback(trObj.originalObject, trObj.objInfo.SqoTypeInfo, transactStorage, header, trObj.engine);
                            SaveHeader(header, transactionInternal.siaqodbInstance);
                            lastHeader = header;
                        }
                        transactionInternal.siaqodbInstance.circularRefCache.Add(trObj.currentObject);
                        
                        trObj.Commit();
                        
                    }
                }
                
                finally
                {
                    transactStorage.Close();
                    transactionInternal.siaqodbInstance.TransactionCommitStatus(false);
                }

                foreach (SqoTypeInfo ti in transactionInternal.tiInvolvedInTransaction)
                {
                    transactionInternal.siaqodbInstance.PutIndexPersiststenceState(ti, true);
                    transactionInternal.siaqodbInstance.PersistIndexDirtyNodes(ti);
                }

                transactions.Remove(id);
                transactionInternal.siaqodbInstance.DropType<TransactionObjectHeader>();
                transactionInternal.siaqodbInstance.DropType<TransactionTypeHeader>();
                transactionInternal.siaqodbInstance.DropTransactionLog();
                transactionInternal.transaction.status = TransactionStatus.Closed;
                transactionInternal.siaqodbInstance.Flush();
            }
        }
        
#if ASYNC
        internal static async Task CommitTransactionAsync(Guid id)
        {
            await _locker.LockAsync();
            try
            {


                TransactionInternal transactionInternal = transactions[id];

                TransactionObjectHeader lastHeader = null;
                TransactionsStorage transactStorage = transactionInternal.siaqodbInstance.GetTransactionLogStorage();
                foreach (SqoTypeInfo ti in transactionInternal.tiInvolvedInTransaction)
                {
                    transactionInternal.siaqodbInstance.PutIndexPersiststenceState(ti, false);
                }
                transactionInternal.siaqodbInstance.TransactionCommitStatus(true);
                try
                {
                    foreach (TransactionObject trObj in transactionInternal.transactionObjects)
                    {
                        if (!transactionInternal.nrRecordsBeforeCommit.ContainsKey(trObj.serializer))
                        {
                            transactionInternal.nrRecordsBeforeCommit.Add(trObj.serializer, new KeyValuePair<SqoTypeInfo, int>(trObj.objInfo.SqoTypeInfo, trObj.objInfo.SqoTypeInfo.Header.numberOfRecords));
                            TransactionTypeHeader tHeader = new TransactionTypeHeader();
                            tHeader.NumberOfRecords = trObj.objInfo.SqoTypeInfo.Header.numberOfRecords;
                            tHeader.TypeName = trObj.objInfo.SqoTypeInfo.TypeName;
                            await transactionInternal.siaqodbInstance.StoreObjectAsync(tHeader).ConfigureAwait(false);
                            await transactionInternal.siaqodbInstance.FlushAsync<TransactionTypeHeader>().ConfigureAwait(false);
                        }


                        TransactionObjectHeader header = await trObj.PreCommitAsync().ConfigureAwait(false);
                        if (header != null)
                        {
                            if (lastHeader != null)
                            {
                                header.Position = lastHeader.Position + lastHeader.BatchSize;
                            }
                            await SaveObjectForCrashRollbackAsync(trObj.originalObject, trObj.objInfo.SqoTypeInfo, transactStorage, header, trObj.engine).ConfigureAwait(false);
                            await SaveHeaderAsync(header, transactionInternal.siaqodbInstance).ConfigureAwait(false);
                            lastHeader = header;
                        }
                        transactionInternal.siaqodbInstance.circularRefCache.Add(trObj.currentObject);

                        await trObj.CommitAsync().ConfigureAwait(false);

                    }
                }

                finally
                {
                    transactStorage.Close();
                    transactionInternal.siaqodbInstance.TransactionCommitStatus(false);
                }

                foreach (SqoTypeInfo ti in transactionInternal.tiInvolvedInTransaction)
                {
                    transactionInternal.siaqodbInstance.PutIndexPersiststenceState(ti, true);
                    await transactionInternal.siaqodbInstance.PersistIndexDirtyNodesAsync(ti).ConfigureAwait(false);
                }

                transactions.Remove(id);
                await transactionInternal.siaqodbInstance.DropTypeAsync<TransactionObjectHeader>().ConfigureAwait(false);
                await transactionInternal.siaqodbInstance.DropTypeAsync<TransactionTypeHeader>().ConfigureAwait(false);
                transactionInternal.siaqodbInstance.DropTransactionLog();
                transactionInternal.transaction.status = TransactionStatus.Closed;
                await transactionInternal.siaqodbInstance.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                _locker.Release();
            }
        }
        
#endif
        private static void SaveObjectForCrashRollback(object obj, SqoTypeInfo ti, TransactionsStorage storage, TransactionObjectHeader header,StorageEngine engine)
        {

            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti, engine.metaCache);
            byte[] bytes=engine.GetObjectBytes(objInfo.Oid,ti);
            int batchSize = storage.SaveTransactionalObject(bytes, header.Position);
            storage.Flush();
            header.BatchSize = batchSize;
            header.TypeName = ti.TypeName;


        }
#if ASYNC
        private static async Task SaveObjectForCrashRollbackAsync(object obj, SqoTypeInfo ti, TransactionsStorage storage, TransactionObjectHeader header, StorageEngine engine)
        {

            ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti, engine.metaCache);
            byte[] bytes = await engine.GetObjectBytesAsync(objInfo.Oid, ti).ConfigureAwait(false);
            int batchSize = await storage.SaveTransactionalObjectAsync(bytes, header.Position).ConfigureAwait(false);
            await storage.FlushAsync().ConfigureAwait(false);
            header.BatchSize = batchSize;
            header.TypeName = ti.TypeName;


        }
#endif
        private static void SaveHeader(TransactionObjectHeader header, Siaqodb siaqodb)
        {
            siaqodb.StoreObject(header);
            siaqodb.Flush<TransactionObjectHeader>();
        }
#if ASYNC
        private static async Task SaveHeaderAsync(TransactionObjectHeader header, Siaqodb siaqodb)
        {
            await siaqodb.StoreObjectAsync(header).ConfigureAwait(false);
            await siaqodb.FlushAsync<TransactionObjectHeader>().ConfigureAwait(false);
        }
#endif
        internal static void RollbackTransaction(Guid id)
        {
            lock (_SyncRoot)
            {
                TransactionInternal transactionInternal = transactions[id];
                
                foreach (ObjectSerializer ser in transactionInternal.nrRecordsBeforeCommit.Keys)
                {
                    ser.SaveNrRecords(transactionInternal.nrRecordsBeforeCommit[ser].Key, transactionInternal.nrRecordsBeforeCommit[ser].Value);
                }
                foreach (TransactionObject trObj in transactionInternal.transactionObjects)
                {
                    trObj.Rollback();
                }
                transactions.Remove(id);
                transactionInternal.siaqodbInstance.DropType<TransactionObjectHeader>();
                transactionInternal.siaqodbInstance.DropType<TransactionTypeHeader>();
                transactionInternal.siaqodbInstance.DropTransactionLog();
                transactionInternal.transaction.status = TransactionStatus.Closed;
                transactionInternal.siaqodbInstance.Flush();
            }
        }
#if ASYNC
        internal static async Task RollbackTransactionAsync(Guid id)
        {
             await _locker.LockAsync();
             try
             {
                 TransactionInternal transactionInternal = transactions[id];

                 foreach (ObjectSerializer ser in transactionInternal.nrRecordsBeforeCommit.Keys)
                 {
                     await ser.SaveNrRecordsAsync(transactionInternal.nrRecordsBeforeCommit[ser].Key, transactionInternal.nrRecordsBeforeCommit[ser].Value).ConfigureAwait(false);
                 }
                 foreach (TransactionObject trObj in transactionInternal.transactionObjects)
                 {
                     await trObj.RollbackAsync().ConfigureAwait(false);
                 }
                 transactions.Remove(id);
                 await transactionInternal.siaqodbInstance.DropTypeAsync<TransactionObjectHeader>().ConfigureAwait(false);
                 await transactionInternal.siaqodbInstance.DropTypeAsync<TransactionTypeHeader>().ConfigureAwait(false);
                 transactionInternal.siaqodbInstance.DropTransactionLog();
                 transactionInternal.transaction.status = TransactionStatus.Closed;
                 await transactionInternal.siaqodbInstance.FlushAsync().ConfigureAwait(false);
             }

             finally
             {
                 _locker.Release();
             }
        }
#endif
    }
}
