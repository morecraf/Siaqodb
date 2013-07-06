using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Core;
using Sqo.Meta;
using Sqo.Attributes;
using Sqo.Utilities;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo.Transactions
{
    class TransactionObject
    {

        
        public TransactionObject(StorageEngine storageEngine)
        {
            this.engine = storageEngine;
            
        }
        public ObjectInfo objInfo;
        public object originalObject;//for rollback
        public object currentObject;
        public StorageEngine engine;
        public ObjectSerializer serializer;
        public OperationType Operation;

        public TransactionObjectHeader PreCommit()
        {
            if (objInfo.Oid > 0 && objInfo.Oid <= objInfo.SqoTypeInfo.Header.numberOfRecords && !serializer.IsObjectDeleted(objInfo.Oid, objInfo.SqoTypeInfo))
            {
                originalObject = engine.LoadObjectByOID(objInfo.SqoTypeInfo, objInfo.Oid);
                TransactionObjectHeader header = new TransactionObjectHeader();
                header.Operation = this.Operation;
                header.OIDofObject = engine.metaCache.GetOIDOfObject(originalObject, objInfo.SqoTypeInfo);
                return header;

            }
            return null;            
        }
#if ASYNC
        public async Task<TransactionObjectHeader> PreCommitAsync()
        {
            if (objInfo.Oid > 0 && objInfo.Oid <= objInfo.SqoTypeInfo.Header.numberOfRecords && !await serializer.IsObjectDeletedAsync(objInfo.Oid, objInfo.SqoTypeInfo).ConfigureAwait(false))
            {
                originalObject = await engine.LoadObjectByOIDAsync(objInfo.SqoTypeInfo, objInfo.Oid).ConfigureAwait(false);
                TransactionObjectHeader header = new TransactionObjectHeader();
                header.Operation = this.Operation;
                header.OIDofObject = engine.metaCache.GetOIDOfObject(originalObject, objInfo.SqoTypeInfo);
                return header;

            }
            return null;
        }
#endif
        public  void Commit()
        {
            
            if (this.Operation == OperationType.InsertOrUpdate)
            {
                engine.SaveObject(currentObject, objInfo.SqoTypeInfo, objInfo);
            }
            else
            {
                engine.DeleteObject(currentObject, objInfo.SqoTypeInfo);
            }
            
        }
#if ASYNC
        public async Task CommitAsync()
        {

            if (this.Operation == OperationType.InsertOrUpdate)
            {
                await engine.SaveObjectAsync(currentObject, objInfo.SqoTypeInfo, objInfo).ConfigureAwait(false);
            }
            else
            {
                await engine.DeleteObjectAsync(currentObject, objInfo.SqoTypeInfo).ConfigureAwait(false);
            }

        }
#endif
        public void Rollback()
        {
            if (originalObject != null)
            {
                if (this.Operation == OperationType.InsertOrUpdate)
                {
                    engine.RollbackObject(originalObject, objInfo.SqoTypeInfo);
                }
                else//delete
                {
                    engine.RollbackDeletedObject(originalObject, objInfo.SqoTypeInfo);
                }
            }
           
        }
#if ASYNC
        public async Task RollbackAsync()
        {
            if (originalObject != null)
            {
                if (this.Operation == OperationType.InsertOrUpdate)
                {
                    await engine.RollbackObjectAsync(originalObject, objInfo.SqoTypeInfo).ConfigureAwait(false);
                }
                else//delete
                {
                    await engine.RollbackDeletedObjectAsync(originalObject, objInfo.SqoTypeInfo).ConfigureAwait(false);
                }
            }

        }
#endif
        public enum OperationType { InsertOrUpdate, Delete };    
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    internal class TransactionObjectHeader
    {
        public int OID { get; set; }
        public long Position;
        public int BatchSize;
        public int OIDofObject;
        [MaxLength(500)]
        public string TypeName;
        public Sqo.Transactions.TransactionObject.OperationType Operation;
        
        #if SILVERLIGHT
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
        #endif
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    internal class TransactionTypeHeader
    {
        public int OID { get; set; }
        [MaxLength(500)]
        public string TypeName;
        public int NumberOfRecords;
#if SILVERLIGHT
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
#endif
    }
}
