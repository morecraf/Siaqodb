using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using Sqo.Queries;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo.MetaObjects
{
    class RawdataManager
    {
        StorageEngine storageEngine;
        public RawdataManager(StorageEngine storageEngine)
        {
            this.storageEngine = storageEngine;
        }
        public RawdataInfo GetRawdataInfo(int oid,LightningDB.LightningTransaction transaction)
        {
            RawdataInfo info = storageEngine.LoadObjectByOID<RawdataInfo>(this.GetSqoTypeInfo(transaction), oid,false,transaction);
            return info;
        }
#if ASYNC
        public async Task<RawdataInfo> GetRawdataInfoAsync(int oid)
        {
            RawdataInfo info = await storageEngine.LoadObjectByOIDAsync<RawdataInfo>(this.GetSqoTypeInfo(), oid, false).ConfigureAwait(false);
            return info;
        }
#endif
        
        public void SaveRawdataInfo(RawdataInfo rawdataInfo,LightningDB.LightningTransaction transaction)
        {
            storageEngine.SaveObject(rawdataInfo, GetSqoTypeInfo(transaction),transaction);
        }
#if ASYNC
        public async Task SaveRawdataInfoAsync(RawdataInfo rawdataInfo)
        {
            await storageEngine.SaveObjectAsync(rawdataInfo, GetSqoTypeInfo()).ConfigureAwait(false);
        }
#endif
        public int GetNextOID(LightningDB.LightningTransaction transaction)
        {
            SqoTypeInfo ti = this.GetSqoTypeInfo(transaction);
            return ti.Header.numberOfRecords + 1;
        }
        private SqoTypeInfo GetSqoTypeInfo(LightningDB.LightningTransaction transaction)
        {
            SqoTypeInfo ti = null;
            if (this.storageEngine.metaCache.Contains(typeof(RawdataInfo)))
            {
                ti = this.storageEngine.metaCache.GetSqoTypeInfo(typeof(RawdataInfo));
            }
            else
            {
                ti = MetaExtractor.GetSqoTypeInfo(typeof(RawdataInfo));
               
                storageEngine.SaveType(ti,transaction);
                this.storageEngine.metaCache.AddType(typeof(RawdataInfo), ti);
               

            }
            return ti;
        }
        internal void MarkRawInfoAsFree(int oid,LightningDB.LightningTransaction transaction)
        {
            this.storageEngine.SaveValue(oid, "IsFree", this.GetSqoTypeInfo(transaction), true, transaction);
        }
#if ASYNC
        internal async Task MarkRawInfoAsFreeAsync(int oid)
        {
            await this.storageEngine.SaveValueAsync(oid, "IsFree", this.GetSqoTypeInfo(), true).ConfigureAwait(false);
        }
#endif
    }
}
