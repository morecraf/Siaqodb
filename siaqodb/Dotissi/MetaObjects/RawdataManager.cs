using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dotissi.Meta;
using Dotissi.Queries;
using Sqo.MetaObjects;
using Dotissi.Meta;
#if ASYNC_LMDB
using System.Threading.Tasks;
#endif
namespace Dotissi.MetaObjects
{
    class RawdataManager
    {
        StorageEngine storageEngine;
        public RawdataManager(StorageEngine storageEngine)
        {
            this.storageEngine = storageEngine;
        }
        public RawdataInfo GetRawdataInfo(int oid)
        {
            RawdataInfo info = storageEngine.LoadObjectByOID<RawdataInfo>(this.GetSqoTypeInfo(), oid,false);
            return info;
        }
#if ASYNC_LMDB
        public async Task<RawdataInfo> GetRawdataInfoAsync(int oid)
        {
            RawdataInfo info = await storageEngine.LoadObjectByOIDAsync<RawdataInfo>(this.GetSqoTypeInfo(), oid, false).ConfigureAwait(false);
            return info;
        }
#endif
        
        public RawdataInfo GetFreeRawdataInfo(int rawLength)
        {
            Where w = new Where("IsFree", OperationType.Equal, true);
            w.StorageEngine=this.storageEngine;
            w.ParentSqoTypeInfo = this.GetSqoTypeInfo();
            w.ParentType.Add(w.ParentSqoTypeInfo.Type);
            Where w1 = new Where("Length", OperationType.GreaterThanOrEqual, rawLength);
            w1.StorageEngine=storageEngine;
            w1.ParentSqoTypeInfo = this.GetSqoTypeInfo();
            w1.ParentType.Add(w1.ParentSqoTypeInfo.Type);
            And and = new And();
            and.Add(w,w1);

            List<int> oids = and.GetOIDs();
            if (oids.Count > 0)
            {
                return this.GetRawdataInfo(oids[0]);
            }

            return null;
        }
#if ASYNC_LMDB
        public async Task<RawdataInfo> GetFreeRawdataInfoAsync(int rawLength)
        {
            Where w = new Where("IsFree", OperationType.Equal, true);
            w.StorageEngine = this.storageEngine;
            w.ParentSqoTypeInfo = this.GetSqoTypeInfo();
            w.ParentType.Add(w.ParentSqoTypeInfo.Type);
            Where w1 = new Where("Length", OperationType.GreaterThanOrEqual, rawLength);
            w1.StorageEngine = storageEngine;
            w1.ParentSqoTypeInfo = this.GetSqoTypeInfo();
            w1.ParentType.Add(w1.ParentSqoTypeInfo.Type);
            And and = new And();
            and.Add(w, w1);

            List<int> oids = await and.GetOIDsAsync().ConfigureAwait(false);
            if (oids.Count > 0)
            {
                return await this.GetRawdataInfoAsync(oids[0]).ConfigureAwait(false);
            }

            return null;
        }
#endif
        public void SaveRawdataInfo(RawdataInfo rawdataInfo)
        {
            storageEngine.SaveObject(rawdataInfo, GetSqoTypeInfo());
        }
#if ASYNC_LMDB
        public async Task SaveRawdataInfoAsync(RawdataInfo rawdataInfo)
        {
            await storageEngine.SaveObjectAsync(rawdataInfo, GetSqoTypeInfo()).ConfigureAwait(false);
        }
#endif
        public int GetNextOID()
        {
            SqoTypeInfo ti = this.GetSqoTypeInfo();
            return ti.Header.numberOfRecords + 1;
        }
        private SqoTypeInfo GetSqoTypeInfo()
        {
            SqoTypeInfo ti = null;
            if (this.storageEngine.metaCache.Contains(typeof(RawdataInfo)))
            {
                ti = this.storageEngine.metaCache.GetSqoTypeInfo(typeof(RawdataInfo));
            }
            else
            {
                ti = Dotissi.Meta.MetaExtractor.GetSqoTypeInfo(typeof(RawdataInfo));
               
                storageEngine.SaveType(ti);
                this.storageEngine.metaCache.AddType(typeof(RawdataInfo), ti);
               

            }
            return ti;
        }
        internal void MarkRawInfoAsFree(int oid)
        {
            this.storageEngine.SaveValue(oid, "IsFree", this.GetSqoTypeInfo(), true);
        }
#if ASYNC_LMDB
        internal async Task MarkRawInfoAsFreeAsync(int oid)
        {
            await this.storageEngine.SaveValueAsync(oid, "IsFree", this.GetSqoTypeInfo(), true).ConfigureAwait(false);
        }
#endif
    }
}
