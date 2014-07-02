using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;

namespace Sqo.Cache
{
    class MetaCache
    {
        private  Dictionary<Type, SqoTypeInfo> cacheOfTypes = new Dictionary<Type, SqoTypeInfo>();
        private CacheOIDs cacheOIDs;
        private CacheDocuments cacheDocs;
        public MetaCache()
        {
            cacheOIDs = new CacheOIDs();
            cacheDocs = new CacheDocuments();
        }
        public  void AddType(Type type, SqoTypeInfo ti)
        {
            if (Sqo.Utilities.SqoLicense.isStarterEdition)
            {
                if (type != typeof(Sqo.MetaObjects.RawdataInfo) && type != typeof(Sqo.Indexes.IndexInfo2) && ti.Header.numberOfRecords > 100)
                {
                    throw new Sqo.Exceptions.InvalidLicenseException("Siaqodb Starter edition may store maximum 100 objects per type!");
                }

            }
            cacheOfTypes[type] = ti;
            this.SetMaxTID(ti.Header.TID);
            if (!Sqo.Utilities.MetaHelper.TypeHasOID(type))
                cacheOIDs.AddTypeInfo(ti);
        }
        public  bool Contains(Type type)
        {
            return cacheOfTypes.ContainsKey(type);
        }
        public  void Remove(Type type)
        {
            if (cacheOfTypes.ContainsKey(type))
            {
                cacheOfTypes.Remove(type);
            }
        }
        public SqoTypeInfo GetSqoTypeInfo(Type t)
        {
            return cacheOfTypes[t];
        }
        public List<SqoTypeInfo> DumpAllTypes()
        {
            List<SqoTypeInfo> types = new List<SqoTypeInfo>();
            foreach (SqoTypeInfo ti in cacheOfTypes.Values)
            {
                types.Add(ti);
            }
            return types;
        }
        public SqoTypeInfo GetSqoTypeInfoByTID(int tid)
        {
            return cacheOfTypes.Values.First<SqoTypeInfo>(tii => tii.Header.TID == tid);
                  
        }
        private int nextTID;
        public int GetNextTID()
        {
            ++nextTID;

            return nextTID;
        }
        public void SetMaxTID(int tid)
        {
            if (nextTID < tid)
            {
                nextTID = tid;
            }
        }
        public void SetOIDToObject(object obj, int oid, SqoTypeInfo ti)
        {
            cacheOIDs.SetOIDToObject(obj, oid, ti);
        }
        public int GetOIDOfObject(object obj, SqoTypeInfo ti)
        {
            return cacheOIDs.GetOIDOfObject(obj, ti);
        }
        public void AddDocument(SqoTypeInfo ti, object parentObjOfDocument, string fieldName, int docinfoOid)
        {
            cacheDocs.AddDocument(ti, parentObjOfDocument, fieldName, docinfoOid);
        }
        
        public int GetDocumentInfoOID(SqoTypeInfo ti, object parentObjOfDocument, string fieldName)
        {
            return cacheDocs.GetDocumentInfoOID(ti, parentObjOfDocument, fieldName);
        }
    }
}
