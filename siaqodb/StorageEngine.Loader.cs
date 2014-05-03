using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Sqo.Meta;
using System.Diagnostics;
using Sqo.Core;
using System.IO;
using Sqo.Queries;
using System.Reflection;
using Sqo.Exceptions;
using Sqo.Utilities;
using Sqo.Indexes;
using Sqo.Transactions;
using Sqo.Cache;

#if ASYNC
using System.Threading.Tasks;
#endif

#if SILVERLIGHT
	using System.IO.IsolatedStorage;
#endif

namespace Sqo
{
    partial class StorageEngine
    {

        internal ObjectList<T> LoadAll<T>(SqoTypeInfo ti)
        {
            ObjectList<T> ol = new ObjectList<T>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
            int nrRecords = ti.Header.numberOfRecords;
            int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
            if (rangeSize < 1) rangeSize = 1;
           
            for (int i = 0; i < nrRecords; i++)
            {


                int oid = i + 1;
                if (i % rangeSize == 0)
                {
                    int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                    serializer.PreLoadBytes(oid, oidEnd, ti);
                }
                if (serializer.IsObjectDeleted(oid, ti))
                {
                    continue;
                }
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                    this.OnLoadingObject(args);
                    if (args.Cancel)
                    {
                        continue;
                    }
                    else if (args.Replace != null)
                    {
                        ol.Add((T)args.Replace);
                        continue;
                    }
                }
                T currentObj = default(T);
                currentObj = Activator.CreateInstance<T>();
                circularRefCache.Clear();
                circularRefCache.Add(oid, ti, currentObj);
                try
                {
                    serializer.ReadObject<T>(currentObj, ti, oid, this.rawSerializer);
                }
                catch (ArgumentException ex)
                {
                    SiaqodbConfigurator.LogMessage("Object with OID:"+oid.ToString()+" seems to be corrupted!", VerboseLevel.Error);
                 
                    if (SiaqodbUtil.IsRepairMode)
                    {
                        SiaqodbConfigurator.LogMessage("Object with OID:" + oid.ToString() + " is deleted", VerboseLevel.Warn);
                        this.DeleteObjectByOID(oid, ti);
                        continue;

                    }
                    else throw ex;
                }
                metaCache.SetOIDToObject(currentObj, oid, ti);
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    this.OnLoadedObject(oid, currentObj);
                }
                ol.Add(currentObj);
            }
            serializer.ResetPreload();
            return ol;
        }
#if ASYNC
        internal async Task<ObjectList<T>> LoadAllAsync<T>(SqoTypeInfo ti)
        {
            ObjectList<T> ol = new ObjectList<T>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedReadComplexObjectAsync);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
          
            int nrRecords = ti.Header.numberOfRecords;
            int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
            if (rangeSize < 1) rangeSize = 1;
           
            for (int i = 0; i < nrRecords; i++)
            {


                int oid = i + 1;
                if (i % rangeSize == 0)
                {
                    int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                    await serializer.PreLoadBytesAsync(oid, oidEnd, ti).ConfigureAwait(false);
                }
                if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                {
                    continue;
                }
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                    this.OnLoadingObject(args);
                    if (args.Cancel)
                    {
                        continue;
                    }
                    else if (args.Replace != null)
                    {
                        ol.Add((T)args.Replace);
                        continue;
                    }
                }
                T currentObj = default(T);
                currentObj = Activator.CreateInstance<T>();
                circularRefCache.Clear();
                circularRefCache.Add(oid, ti, currentObj);
                bool exTh = false;
                try
                {
                    await serializer.ReadObjectAsync<T>(currentObj, ti, oid, this.rawSerializer).ConfigureAwait(false);
                }
                catch (ArgumentException ex)
                {
                    SiaqodbConfigurator.LogMessage("Object with OID:" + oid.ToString() + " seems to be corrupted!", VerboseLevel.Error);

                    if (SiaqodbUtil.IsRepairMode)
                    {
                        SiaqodbConfigurator.LogMessage("Object with OID:" + oid.ToString() + " is deleted", VerboseLevel.Warn);
                        exTh = true;
                    }
                    else throw ex;
                }
                if (exTh)
                {
                    await this.DeleteObjectByOIDAsync(oid, ti).ConfigureAwait(false);
                    continue;

                }
                metaCache.SetOIDToObject(currentObj, oid, ti);
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    this.OnLoadedObject(oid, currentObj);
                }
                ol.Add(currentObj);
            }
            serializer.ResetPreload();
            return ol;
        }
#endif
        internal ObjectTable LoadAll(SqoTypeInfo ti)
        {
            ObjectTable obTable = new ObjectTable();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);

            int nrRecords = ti.Header.numberOfRecords;
            int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
            if (rangeSize < 1) rangeSize = 1;

            obTable.Columns.Add("OID", 0);
            int j = 1;
            foreach (FieldSqoInfo fi in ti.Fields)
            {
                obTable.Columns.Add(fi.Name, j);
                j++;
            }

            for (int i = 0; i < nrRecords; i++)
            {

                int oid = i + 1;
                if (i % rangeSize == 0)
                {
                    int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                    serializer.PreLoadBytes(oid, oidEnd, ti);
                }
                if (serializer.IsObjectDeleted(oid, ti))
                {
                    ObjectRow row = obTable.NewRow();
                    row["OID"] = -oid;
                    obTable.Rows.Add(row);
                }
                else
                {
                    ObjectRow row = obTable.NewRow();
                    row["OID"] = oid;
                    serializer.ReadObjectRow(row, ti, oid, rawSerializer);

                    obTable.Rows.Add(row);
                }
            }
            serializer.ResetPreload();
            return obTable;
        }
#if ASYNC
        internal async Task<ObjectTable> LoadAllAsync(SqoTypeInfo ti)
        {
            ObjectTable obTable = new ObjectTable();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedReadComplexObjectAsync);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
          
            int nrRecords = ti.Header.numberOfRecords;
            int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
            if (rangeSize < 1) rangeSize = 1;
            
            obTable.Columns.Add("OID", 0);
            int j = 1;
            foreach (FieldSqoInfo fi in ti.Fields)
            {
                obTable.Columns.Add(fi.Name, j);
                j++;
            }

            for (int i = 0; i < nrRecords; i++)
            {

                int oid = i + 1;
                if (i % rangeSize == 0)
                {
                    int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                    await serializer.PreLoadBytesAsync(oid, oidEnd, ti).ConfigureAwait(false);
                }
                if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                {
                    ObjectRow row = obTable.NewRow();
                    row["OID"] = -oid;
                    obTable.Rows.Add(row);
                }
                else
                {
                    ObjectRow row = obTable.NewRow();
                    row["OID"] = oid;
                    await serializer.ReadObjectRowAsync(row, ti, oid, rawSerializer).ConfigureAwait(false);

                    obTable.Rows.Add(row);
                }
            }
            serializer.ResetPreload();
            return obTable;
        }
       
#endif
        internal List<int> LoadFilteredOids(Where where)
        {
            List<int> oids = null;


            //fix Types problem when a field is declared in a base class and used in a derived class
            Type type = where.ParentSqoTypeInfo.Type;

            for (int j = (where.AttributeName.Count - 1); j >= 0; j--)
            {
                string fieldName = where.AttributeName[j];
                FieldInfo finfo = MetaExtractor.FindField(type, fieldName);
                if (finfo != null )
                {
                    where.ParentType[j] = type;
                    type = finfo.FieldType;
                }
                else if (fieldName == "OID")
                {
                    where.ParentType[j] = type;
                }
               
            }
            int i = 0;
            foreach (string attName in where.AttributeName)
            {
                if (i == 0)//the deepest property
                {
                    SqoTypeInfo ti = this.GetSqoTypeInfoSoft(where.ParentType[i]);
                    oids = this.LoadFilteredOids(where, ti);
                }
                else
                {
                    SqoTypeInfo ti = this.GetSqoTypeInfoSoft(where.ParentType[i]);
                    ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
                    List<int> oidsComplextObj = this.GetOIDsOfComplexObj(ti, where.AttributeName[i], oids);
                    oids = oidsComplextObj;
                }

                i++;
            }
            return oids;
        }

#if ASYNC
        internal async Task<List<int>> LoadFilteredOidsAsync(Where where)
        {
            List<int> oids = null;


            //fix Types problem when a field is declared in a base class and used in a derived class
            Type type = where.ParentSqoTypeInfo.Type;

            for (int j = (where.AttributeName.Count - 1); j >= 0; j--)
            {
                string fieldName = where.AttributeName[j];
                FieldInfo finfo = MetaExtractor.FindField(type, fieldName);
                if (finfo != null)
                {
                    where.ParentType[j] = type;
                    type = finfo.FieldType;
                }
                else if (fieldName == "OID")
                {
                    where.ParentType[j] = type;
                }

            }
            int i = 0;
            foreach (string attName in where.AttributeName)
            {
                if (i == 0)//the deepest property
                {
                    SqoTypeInfo ti = this.GetSqoTypeInfoSoft(where.ParentType[i]);
                    oids = await this.LoadFilteredOidsAsync(where, ti).ConfigureAwait(false);
                }
                else
                {
                    SqoTypeInfo ti = this.GetSqoTypeInfoSoft(where.ParentType[i]);
                    ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
                    List<int> oidsComplextObj = await this.GetOIDsOfComplexObjAsync(ti, where.AttributeName[i], oids).ConfigureAwait(false);
                    oids = oidsComplextObj;
                }

                i++;
            }
            return oids;
        }

#endif
        private List<int> GetOIDsOfComplexObj(SqoTypeInfo ti, string fieldName, List<int> insideOids)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            insideOids.Sort();
            int nrRecords = ti.Header.numberOfRecords;
            List<int> oids = new List<int>();
            if (insideOids.Count == 0)
                return oids;

            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (serializer.IsObjectDeleted(oid, ti))
                {
                    continue;
                }

                int oidOfComplex = serializer.ReadOidOfComplex(ti, oid, fieldName, this.rawSerializer);

                int index = insideOids.BinarySearch(oidOfComplex);//intersection
                if (index >= 0)
                {
                    oids.Add(oid);
                }


            }
            return oids;
        }
#if ASYNC
        private async Task<List<int>> GetOIDsOfComplexObjAsync(SqoTypeInfo ti, string fieldName, List<int> insideOids)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            insideOids.Sort();
            int nrRecords = ti.Header.numberOfRecords;
            List<int> oids = new List<int>();
            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                {
                    continue;
                }

                int oidOfComplex = await serializer.ReadOidOfComplexAsync(ti, oid, fieldName, this.rawSerializer).ConfigureAwait(false);

                int index = insideOids.BinarySearch(oidOfComplex);//intersection
                if (index >= 0)
                {
                    oids.Add(oid);
                }


            }
            return oids;
        }
#endif
        internal List<int> LoadFilteredOids(Where where, SqoTypeInfo ti)
        {
            List<int> oids = new List<int>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);

            int nrRecords = ti.Header.numberOfRecords;
            bool isOIDField = where.AttributeName[0] == "OID";
            if (!indexManager.LoadOidsByIndex(ti, where.AttributeName[0], where, oids))
            {
                if (tagsIndexManager != null && ti.Type == typeof(DotissiObject) && (where.AttributeName[0] == "intTags" || where.AttributeName[0] == "strTags") && tagsIndexManager.ExistsIndex(where.Value2.ToString()))
                {
                    tagsIndexManager.LoadOidsByIndex(where, oids);
                }
                else if (isOIDField)
                {
                    this.FillOidsIndexed(oids, where, ti, serializer);

                }
                else //full scann
                {
                    int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
                    if (rangeSize < 1) rangeSize = 1;

                    for (int i = 0; i < nrRecords; i++)
                    {
                        int oid = i + 1;
                        if (i % rangeSize == 0)
                        {
                            int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                            serializer.PreLoadBytes(oid, oidEnd, ti);
                        }
                        if (serializer.IsObjectDeleted(oid, ti))
                        {
                            continue;
                        }

                        object val = serializer.ReadFieldValue(ti, oid, where.AttributeName[0], this.rawSerializer);
                        if (Match(where, val))
                        {
                            oids.Add(oid);
                        }

                    }
                    serializer.ResetPreload();
                }
            }
            return oids;
        }

#if ASYNC
        internal async Task<List<int>> LoadFilteredOidsAsync(Where where, SqoTypeInfo ti)
        {
            List<int> oids = new List<int>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedReadComplexObjectAsync);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
          
            int nrRecords = ti.Header.numberOfRecords;
            bool isOIDField = where.AttributeName[0] == "OID";
            if (!(await indexManager.LoadOidsByIndexAsync(ti, where.AttributeName[0], where, oids).ConfigureAwait(false)))
            {

                if (isOIDField)
                {
                    await this.FillOidsIndexedAsync(oids, where, ti, serializer).ConfigureAwait(false);

                }
                else //full scann
                {
                    int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
                    if (rangeSize < 1) rangeSize = 1;
           
                    for (int i = 0; i < nrRecords; i++)
                    {
                        int oid = i + 1;
                        if (i % rangeSize == 0)
                        {
                            int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                            await serializer.PreLoadBytesAsync(oid, oidEnd, ti).ConfigureAwait(false);
                        }
                        if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                        {
                            continue;
                        }

                        object val = await serializer.ReadFieldValueAsync(ti, oid, where.AttributeName[0], this.rawSerializer).ConfigureAwait(false);
                        if (Match(where, val))
                        {
                            oids.Add(oid);
                        }

                    }
                    serializer.ResetPreload();
                }
            }
            return oids;
        }
#endif
       

        private void FillOidsIndexed(List<int> oids, Where where, SqoTypeInfo ti, ObjectSerializer serializer)
        {
            int oid = (int)where.Value;
            int nrRecords = ti.Header.numberOfRecords;
            if (where.OperationType == OperationType.Equal)
            {

                if (!serializer.IsObjectDeleted(oid, ti))
                {
                    oids.Add(oid);
                    return;
                }
            }
            else if (where.OperationType == OperationType.NotEqual)
            {
                for (int i = 0; i < nrRecords; i++)
                {
                    int localOid = i + 1;
                    if (serializer.IsObjectDeleted(localOid, ti) || oid == localOid)
                    {
                        continue;
                    }
                    oids.Add(localOid);


                }
                return;
            }
            else if (where.OperationType == OperationType.LessThan)
            {
                for (int i = 0; i < oid - 1; i++)
                {
                    int localOid = i + 1;
                    if (serializer.IsObjectDeleted(localOid, ti))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
            else if (where.OperationType == OperationType.LessThanOrEqual)
            {
                for (int i = 0; i < oid; i++)
                {
                    int localOid = i + 1;
                    if (serializer.IsObjectDeleted(localOid, ti))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
            else if (where.OperationType == OperationType.GreaterThan)
            {
                for (int i = oid; i < nrRecords; i++)
                {
                    int localOid = i + 1;
                    if (serializer.IsObjectDeleted(localOid, ti))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
            else if (where.OperationType == OperationType.GreaterThanOrEqual)
            {
                for (int i = oid - 1; i < nrRecords; i++)
                {
                    int localOid = i + 1;
                    if (serializer.IsObjectDeleted(localOid, ti))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
        }
#if ASYNC
        private async Task FillOidsIndexedAsync(List<int> oids, Where where, SqoTypeInfo ti, ObjectSerializer serializer)
        {
            int oid = (int)where.Value;
            int nrRecords = ti.Header.numberOfRecords;
            if (where.OperationType == OperationType.Equal)
            {

                if (!(await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false)))
                {
                    oids.Add(oid);
                    return;
                }
            }
            else if (where.OperationType == OperationType.NotEqual)
            {
                for (int i = 0; i < nrRecords; i++)
                {
                    int localOid = i + 1;
                    if ((await serializer.IsObjectDeletedAsync(localOid, ti).ConfigureAwait(false)) || oid == localOid)
                    {
                        continue;
                    }
                    oids.Add(localOid);


                }
                return;
            }
            else if (where.OperationType == OperationType.LessThan)
            {
                for (int i = 0; i < oid - 1; i++)
                {
                    int localOid = i + 1;
                    if (await serializer.IsObjectDeletedAsync(localOid, ti).ConfigureAwait(false))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
            else if (where.OperationType == OperationType.LessThanOrEqual)
            {
                for (int i = 0; i < oid; i++)
                {
                    int localOid = i + 1;
                    if (await serializer.IsObjectDeletedAsync(localOid, ti).ConfigureAwait(false))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
            else if (where.OperationType == OperationType.GreaterThan)
            {
                for (int i = oid; i < nrRecords; i++)
                {
                    int localOid = i + 1;
                    if (await serializer.IsObjectDeletedAsync(localOid, ti).ConfigureAwait(false))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
            else if (where.OperationType == OperationType.GreaterThanOrEqual)
            {
                for (int i = oid - 1; i < nrRecords; i++)
                {
                    int localOid = i + 1;
                    if (await serializer.IsObjectDeletedAsync(localOid, ti).ConfigureAwait(false))
                    {
                        continue;
                    }
                    oids.Add(localOid);

                }
                return;

            }
        }
#endif
        private bool Match(Where w, object val)
        {
            if (val == null || w.Value == null)
            {
                if (w.OperationType == OperationType.Equal)
                {
                    return val == w.Value;
                }
                else if (w.OperationType == OperationType.NotEqual)
                {
                    return val != w.Value;
                }

            }
            else
            {
                #region IList
                if (val is IList)
                {
                    if (w.OperationType == OperationType.Contains)
                    {
                        IList valObj = val as IList;
                        IList valWhere = w.Value as IList;
                        IComparable valComp = w.Value as IComparable;

                        if (valComp == null && val.GetType().IsClass() && valWhere == null)//complex type
                        {
                            if (parentsComparison == null)
                            {
                                parentsComparison = new List<object>();
                            }
                            foreach (object listObj in valObj)
                            {

                                try
                                {
                                    parentsComparison.Add(listObj);
                                    parentsComparison.Add(w.Value);

                                    if (ComplexObjectsAreEqual(listObj, w.Value))
                                    {
                                        return true;
                                    }
                                }
                                finally
                                {
                                    parentsComparison.Clear();
                                }
                            }
                            return false;
                        }
                        else if (valWhere != null)//jagged list
                        {
                            foreach (object listObj in valObj)
                            {
                                if (ListsAreEqual((IList)listObj, valWhere))
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            return valObj.Contains(w.Value);
                        }
                    }
                    return false;
                }
                #endregion

                #region dictionary
                else if (val is IDictionary)
                {
                    IDictionary dictionary = val as IDictionary;

                    IComparable valComp = w.Value as IComparable;

                    if (valComp == null && valComp.GetType().IsClass())//complex type
                    {
                        if (parentsComparison == null)
                        {
                            parentsComparison = new List<object>();
                        }
                        foreach (object key in dictionary.Keys)
                        {

                            try
                            {
                                parentsComparison.Add(w.Value);
                                object keyOrVal = w.OperationType == OperationType.ContainsKey ? key : dictionary[key];

                                parentsComparison.Add(keyOrVal);

                                if (ComplexObjectsAreEqual(keyOrVal, w.Value))
                                {
                                    return true;
                                }
                            }
                            finally
                            {
                                parentsComparison.Clear();
                            }
                        }
                        return false;
                    }
                    else
                    {
                        foreach (object key in dictionary.Keys)
                        {
                            if (w.OperationType == OperationType.ContainsKey)
                            {
                                
                                int compareResultKey = valComp.CompareTo((IComparable)key);
                                if (compareResultKey == 0)
                                {
                                    return true;
                                }
                            }
                            else if (w.OperationType == OperationType.ContainsValue)
                            {

                                int compareResultVal = valComp.CompareTo((IComparable)dictionary[key]);
                                if (compareResultVal == 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;

                }
                #endregion

                if (val.GetType() != w.Value.GetType())
                {

                    w.Value = Convertor.ChangeType(w.Value, val.GetType());

                }
                IComparable valComparable = val as IComparable;
                if (valComparable == null && val.GetType().IsClass())//complex type
                {
                    try
                    {
                        if (parentsComparison == null)
                        {
                            parentsComparison = new List<object>();
                        }
                        parentsComparison.Add(val);
                        parentsComparison.Add(w.Value);

                        if (w.OperationType == OperationType.Equal)
                        {
                            return this.ComplexObjectsAreEqual(val, w.Value);
                        }
                        else if (w.OperationType == OperationType.NotEqual)
                        {
                            return !this.ComplexObjectsAreEqual(val, w.Value);
                        }
                    }
                    finally
                    {
                        parentsComparison.Clear();
                    }
                }
                int compareResult = valComparable.CompareTo((IComparable)w.Value);
                if (w.OperationType == OperationType.Equal)
                {
                    return compareResult == 0;
                }
                else if (w.OperationType == OperationType.NotEqual)
                {
                    return !(compareResult == 0);
                }
                else if (w.OperationType == OperationType.LessThan)
                {
                    return compareResult < 0;
                }
                else if (w.OperationType == OperationType.LessThanOrEqual)
                {
                    return compareResult <= 0;
                }
                else if (w.OperationType == OperationType.GreaterThan)
                {
                    return compareResult > 0;
                }
                else if (w.OperationType == OperationType.GreaterThanOrEqual)
                {
                    return compareResult >= 0;
                }
                else if (w.OperationType == OperationType.Contains && val is string)
                {
                    string wVal = w.Value as string;
                    string valObj = val as string;
                    if (w.Value2 != null && w.Value2 is StringComparison)
                    {
                        return valObj.IndexOf(wVal, (StringComparison)w.Value2) != -1;
                    }
                    else
                    {
                        return valObj.Contains(wVal);
                    }
                }
                else if (w.OperationType == OperationType.StartWith)
                {
                    string wVal = w.Value as string;
                    string valObj = val as string;
                    if (w.Value2 != null && w.Value2 is StringComparison)
                    {
                        return valObj.StartsWith(wVal, (StringComparison)w.Value2);
                    }
                    else
                    {
                        return valObj.StartsWith(wVal);
                    }
                }
                else if (w.OperationType == OperationType.EndWith)
                {
                    string wVal = w.Value as string;
                    string valObj = val as string;
                    if (w.Value2 != null && w.Value2 is StringComparison)
                    {
                        return valObj.EndsWith(wVal, (StringComparison)w.Value2);
                    }
                    else
                    {
                        return valObj.EndsWith(wVal);
                    }
                }

            }
            return false;
        }

        private bool ListsAreEqual(IList iList, IList valWhere)
        {
            if (iList.Count != valWhere.Count)
            {
                return false;
            }
            int i=0;
            foreach (object elem in iList)
            {
                IList elemList = elem as IList;
                if (elemList != null)
                {
                    return ListsAreEqual(elemList, (IList)valWhere[i]);
                }
                else
                {
                    IComparable valComp = valWhere[i] as IComparable;

                    if (valComp == null && valWhere[i].GetType().IsClass())//complex type
                    {
                        if (!ComplexObjectsAreEqual(elem, valWhere[i]))
                            return false;
                    }
                    else
                    {

                        if (elem.GetType() != valWhere[i].GetType())
                        {
                            valWhere[i] = Convertor.ChangeType(valWhere[i], elem.GetType());

                        }
                        int compareResult = valComp.CompareTo((IComparable)elem);
                        if (compareResult != 0)
                        {
                            return false;
                        }
                    }
                }
                i++;
            }
            return true;
        }
        private bool ComplexObjectsAreEqual(object obj1, object obj2)
        {
            SqoTypeInfo ti = this.metaCache.GetSqoTypeInfo(obj1.GetType());
            foreach (FieldSqoInfo fi in ti.Fields)
            {
#if SILVERLIGHT
				object objVal1=null;
                object objVal2=null;
                try
				{
					objVal1=MetaHelper.CallGetValue(fi.FInfo,obj1,ti.Type);
					objVal2=MetaHelper.CallGetValue(fi.FInfo,obj2,ti.Type);
				}
				catch (Exception ex)
				{
					throw new SiaqodbException("Override GetValue and SetValue methods of SqoDataObject-Silverlight limitation to private fields");
				}
#else
                object objVal1 = fi.FInfo.GetValue(obj1);
                object objVal2 = fi.FInfo.GetValue(obj2);
#endif
                if (fi.FInfo.FieldType == typeof(string))
                {
                    if (objVal1 == null)
                    {
                        objVal1 = string.Empty;
                    }
                    if (objVal2 == null)
                    {
                        objVal2 = string.Empty;
                    }
                }

                if (objVal1 == null || objVal2 == null)
                {
                    if (objVal1 != objVal2)
                        return false;
                }
                else
                {
                    IComparable valComparable = objVal1 as IComparable;
                    if (valComparable != null)
                    {
                        if (valComparable.CompareTo((IComparable)objVal2) != 0)
                        {
                            return false;
                        }
                    }
                    else if (objVal1 is IList)
                    {
                        return ListsAreEqual((IList)objVal1,(IList) objVal2);
                    }
                    else if (objVal1.GetType().IsClass())//complex type
                    {
                        if (parentsComparison.Contains(objVal1) || parentsComparison.Contains(objVal2))
                        {
                            continue;
                        }
                        parentsComparison.Add(objVal1);
                        parentsComparison.Add(objVal2);

                        if (!ComplexObjectsAreEqual(objVal1, objVal2))
                        {
                            return false;
                        }
                    }
                }


            }
            return true;
        }

        internal List<int> LoadFilteredDeletedOids(Where where, SqoTypeInfo ti)
        {
            List<int> oids = new List<int>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            int nrRecords = ti.Header.numberOfRecords;

            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (serializer.IsObjectDeleted(oid, ti))
                {
                    object val = serializer.ReadFieldValue(ti, oid, where.AttributeName[0]);
                    if (Match(where, val))
                    {
                        oids.Add(oid);
                    }
                }
                else
                {
                    continue;
                }



            }

            return oids;
        }
#if ASYNC
        internal async Task<List<int>> LoadFilteredDeletedOidsAsync(Where where, SqoTypeInfo ti)
        {
            List<int> oids = new List<int>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            int nrRecords = ti.Header.numberOfRecords;

            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                {
                    object val = await serializer.ReadFieldValueAsync(ti, oid, where.AttributeName[0]).ConfigureAwait(false);
                    if (Match(where, val))
                    {
                        oids.Add(oid);
                    }
                }
                else
                {
                    continue;
                }
            }

            return oids;
        }
#endif
        internal IObjectList<T> LoadByOIDs<T>(List<int> oids, SqoTypeInfo ti)
        {
            ObjectList<T> ol = new ObjectList<T>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs> (serializer_NeedCacheDocument);
            //int nrRecords = ti.Header.numberOfRecords;
            foreach (int oid in oids)
            {
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                    this.OnLoadingObject(args);
                    if (args.Cancel)
                    {
                        continue;
                    }
                    else if (args.Replace != null)
                    {
                        ol.Add((T)args.Replace);
                        continue;
                    }
                }
                T currentObj = default(T);
                currentObj = Activator.CreateInstance<T>();
                circularRefCache.Clear();
                circularRefCache.Add(oid, ti, currentObj);
                serializer.ReadObject<T>(currentObj, ti, oid, rawSerializer);

                metaCache.SetOIDToObject(currentObj, oid, ti);
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    this.OnLoadedObject(oid, currentObj);
                }
                ol.Add(currentObj);
            }
            return ol;
        }

        void serializer_NeedCacheDocument(object sender, DocumentEventArgs e)
        {
            metaCache.AddDocument(e.TypeInfo, e.ParentObject, e.FieldName, e.DocumentInfoOID);
        }
#if ASYNC
        internal async Task<IObjectList<T>> LoadByOIDsAsync<T>(List<int> oids, SqoTypeInfo ti)
        {
            ObjectList<T> ol = new ObjectList<T>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedReadComplexObjectAsync);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
          
            //int nrRecords = ti.Header.numberOfRecords;
            foreach (int oid in oids)
            {
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                    this.OnLoadingObject(args);
                    if (args.Cancel)
                    {
                        continue;
                    }
                    else if (args.Replace != null)
                    {
                        ol.Add((T)args.Replace);
                        continue;
                    }
                }
                T currentObj = default(T);
                currentObj = Activator.CreateInstance<T>();
                circularRefCache.Clear();
                circularRefCache.Add(oid, ti, currentObj);
                await serializer.ReadObjectAsync<T>(currentObj, ti, oid, rawSerializer).ConfigureAwait(false);

                metaCache.SetOIDToObject(currentObj, oid, ti);
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    this.OnLoadedObject(oid, currentObj);
                }
                ol.Add(currentObj);
            }
            return ol;
        }
    
#endif
        void serializer_NeedReadComplexObject(object sender, ComplexObjectEventArgs e)
        {
            
            if (e.TID == 0 && e.SavedOID == 0)//means null
            {
                e.ComplexObject = null;
                return;
            }
            else if (e.ParentType != null && SiaqodbConfigurator.LazyLoaded != null && SiaqodbConfigurator.LazyLoaded.ContainsKey(e.ParentType))
            {
                if (SiaqodbConfigurator.LazyLoaded[e.ParentType])
                {
                    if (!this.ExistsInIncludesCache(e.ParentType, e.FieldName))
                    {
                        e.ComplexObject = null;
                        return;
                    }
                }
            }
            SqoTypeInfo ti = this.metaCache.GetSqoTypeInfoByTID(e.TID);
            object cacheObj = this.circularRefCache.GetObject(e.SavedOID, ti);
            if (cacheObj != null)
            {
                e.ComplexObject = cacheObj;
            }
            else
            {
                ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
                //if there is a Nested object of same type we have to reset
                serializer.ResetPreload();

                if (!this.IsObjectDeleted(e.SavedOID, ti))
                {
                    e.ComplexObject = this.LoadObjectByOID(ti, e.SavedOID, false);
                }

            }    
            
        }
#if ASYNC
        async Task serializer_NeedReadComplexObjectAsync(object sender, ComplexObjectEventArgs e)
        {
            
            if (e.TID == 0 && e.SavedOID == 0)//means null
            {
                e.ComplexObject = null;
                return;
            }
            else if (e.ParentType != null && SiaqodbConfigurator.LazyLoaded != null && SiaqodbConfigurator.LazyLoaded.ContainsKey(e.ParentType))
            {
                if (SiaqodbConfigurator.LazyLoaded[e.ParentType])
                {
                    if (!this.ExistsInIncludesCache(e.ParentType, e.FieldName))
                    {
                        e.ComplexObject = null;
                        return;
                    }
                }
            }
            SqoTypeInfo ti = this.metaCache.GetSqoTypeInfoByTID(e.TID);
            object cacheObj = this.circularRefCache.GetObject(e.SavedOID, ti);
            if (cacheObj != null)
            {
                e.ComplexObject = cacheObj;
            }
            else
            {
                ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
                //if there is a Nested object of same type we have to reset
                serializer.ResetPreload();
                if (!(await this.IsObjectDeletedAsync(e.SavedOID, ti).ConfigureAwait(false)))
                {
                    e.ComplexObject = await this.LoadObjectByOIDAsync(ti, e.SavedOID, false).ConfigureAwait(false);
                }

            }    
            
        }
#endif
        internal List<object> LoadByOIDs(List<int> oids, SqoTypeInfo ti)
        {
            List<object> ol = new List<object>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);

            foreach (int oid in oids)
            {
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                    this.OnLoadingObject(args);
                    if (args.Cancel)
                    {
                        continue;
                    }
                    else if (args.Replace != null)
                    {
                        ol.Add(args.Replace);
                        continue;
                    }
                }
                object currentObj = Activator.CreateInstance(ti.Type);
                circularRefCache.Clear();
                circularRefCache.Add(oid, ti, currentObj);
                serializer.ReadObject(currentObj, ti, oid, rawSerializer);

                metaCache.SetOIDToObject(currentObj, oid, ti);
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    this.OnLoadedObject(oid, currentObj);
                }
                ol.Add(currentObj);
            }
            return ol;
        }
        
#if ASYNC
        internal async Task<List<object>> LoadByOIDsAsync(List<int> oids, SqoTypeInfo ti)
        {
            List<object> ol = new List<object>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedReadComplexObjectAsync);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
          

            foreach (int oid in oids)
            {
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                    this.OnLoadingObject(args);
                    if (args.Cancel)
                    {
                        continue;
                    }
                    else if (args.Replace != null)
                    {
                        ol.Add(args.Replace);
                        continue;
                    }
                }
                object currentObj = Activator.CreateInstance(ti.Type);
                circularRefCache.Clear();
                circularRefCache.Add(oid, ti, currentObj);
                await serializer.ReadObjectAsync(currentObj, ti, oid, rawSerializer).ConfigureAwait(false);

                metaCache.SetOIDToObject(currentObj, oid, ti);
                if (SiaqodbConfigurator.RaiseLoadEvents)
                {
                    this.OnLoadedObject(oid, currentObj);
                }
                ol.Add(currentObj);
            }
            return ol;
        }
        
#endif
        
        internal List<KeyValuePair<int, int>> LoadJoin(SqoTypeInfo tiOuter, string criteriaOuter, List<int> oidOuter, SqoTypeInfo tiInner, string criteriaInner, List<int> oidInner)
        {


            List<KeyValuePair<int, int>> oids = new List<KeyValuePair<int, int>>();
            ObjectSerializer serializerOuter = SerializerFactory.GetSerializer(this.path, GetFileByType(tiOuter), useElevatedTrust);
            ObjectSerializer serializerInner = SerializerFactory.GetSerializer(this.path, GetFileByType(tiInner), useElevatedTrust);
            bool outCheckForDeleted = false;
            bool innCheckForDeleted = false;

            int nrRecordsOuter = 0;
            if (oidOuter == null)
            {
                nrRecordsOuter = tiOuter.Header.numberOfRecords;
                outCheckForDeleted = true;
            }
            else
            {
                nrRecordsOuter = oidOuter.Count;
            }
            int nrRecordsInner = 0;
            if (oidInner == null)
            {
                nrRecordsInner = tiInner.Header.numberOfRecords;
                innCheckForDeleted = true;
            }
            else
            {
                nrRecordsInner = oidInner.Count;
            }
#if UNITY3D
            Dictionary<int, object> outerDict = new Dictionary<int, object>(new Sqo.Utilities.EqualityComparer<int>());
			Dictionary<int, object> innerDict = new Dictionary<int, object>(new Sqo.Utilities.EqualityComparer<int>());
#else
            Dictionary<int, object> outerDict = new Dictionary<int, object>();
            Dictionary<int, object> innerDict = new Dictionary<int, object>();
#endif
            for (int i = 0; i < nrRecordsOuter; i++)
            {
                int oidOut = oidOuter == null ? (i + 1) : oidOuter[i];
                if (outCheckForDeleted)
                {
                    if (serializerOuter.IsObjectDeleted(oidOut, tiOuter))
                    {
                        continue;
                    }
                }
                if (string.Compare(criteriaOuter, "OID") == 0)
                {
                    outerDict.Add(oidOut, oidOut);
                }
                else
                {
                    object val = serializerOuter.ReadFieldValue(tiOuter, oidOut, criteriaOuter);
                    if (val != null)//added when nullable types was added
                    {
                        outerDict.Add(oidOut, val);
                    }
                }
            }
            for (int j = 0; j < nrRecordsInner; j++)
            {
                int oidInn = oidInner == null ? (j + 1) : oidInner[j];
                if (innCheckForDeleted)
                {
                    if (serializerInner.IsObjectDeleted(oidInn, tiInner))
                    {
                        continue;
                    }
                }
                if (string.Compare(criteriaInner, "OID") == 0)
                {
                    innerDict.Add(oidInn, oidInn);
                }
                else
                {
                    object valInner = serializerInner.ReadFieldValue(tiInner, oidInn, criteriaInner);
                    if (valInner != null)//added when nullable types was added
                    {
                        innerDict.Add(oidInn, valInner);
                    }
                }
            }
            foreach (int outerOid in outerDict.Keys)
            {
                object val = outerDict[outerOid];
                foreach (int innerOid in innerDict.Keys)
                {
                    object valInner = innerDict[innerOid];
                    if (val.Equals(valInner))
                    {
                        KeyValuePair<int, int> kv = new KeyValuePair<int, int>(outerOid, innerOid);

                        oids.Add(kv);
                    }
                }
            }


            return oids;
        }
#if ASYNC
        internal async Task<List<KeyValuePair<int, int>>> LoadJoinAsync(SqoTypeInfo tiOuter, string criteriaOuter, List<int> oidOuter, SqoTypeInfo tiInner, string criteriaInner, List<int> oidInner)
        {


            List<KeyValuePair<int, int>> oids = new List<KeyValuePair<int, int>>();
            ObjectSerializer serializerOuter = SerializerFactory.GetSerializer(this.path, GetFileByType(tiOuter), useElevatedTrust);
            ObjectSerializer serializerInner = SerializerFactory.GetSerializer(this.path, GetFileByType(tiInner), useElevatedTrust);
            bool outCheckForDeleted = false;
            bool innCheckForDeleted = false;

            int nrRecordsOuter = 0;
            if (oidOuter == null)
            {
                nrRecordsOuter = tiOuter.Header.numberOfRecords;
                outCheckForDeleted = true;
            }
            else
            {
                nrRecordsOuter = oidOuter.Count;
            }
            int nrRecordsInner = 0;
            if (oidInner == null)
            {
                nrRecordsInner = tiInner.Header.numberOfRecords;
                innCheckForDeleted = true;
            }
            else
            {
                nrRecordsInner = oidInner.Count;
            }
#if UNITY3D
            Dictionary<int, object> outerDict = new Dictionary<int, object>(new Sqo.Utilities.EqualityComparer<int>());
			Dictionary<int, object> innerDict = new Dictionary<int, object>(new Sqo.Utilities.EqualityComparer<int>());
#else
            Dictionary<int, object> outerDict = new Dictionary<int, object>();
            Dictionary<int, object> innerDict = new Dictionary<int, object>();
#endif
            for (int i = 0; i < nrRecordsOuter; i++)
            {
                int oidOut = oidOuter == null ? (i + 1) : oidOuter[i];
                if (outCheckForDeleted)
                {
                    if (await serializerOuter.IsObjectDeletedAsync(oidOut, tiOuter).ConfigureAwait(false))
                    {
                        continue;
                    }
                }
                if (string.Compare(criteriaOuter, "OID") == 0)
                {
                    outerDict.Add(oidOut, oidOut);
                }
                else
                {
                    object val = await serializerOuter.ReadFieldValueAsync(tiOuter, oidOut, criteriaOuter).ConfigureAwait(false);
                    if (val != null)//added when nullable types was added
                    {
                        outerDict.Add(oidOut, val);
                    }
                }
            }
            for (int j = 0; j < nrRecordsInner; j++)
            {
                int oidInn = oidInner == null ? (j + 1) : oidInner[j];
                if (innCheckForDeleted)
                {
                    if (await serializerInner.IsObjectDeletedAsync(oidInn, tiInner).ConfigureAwait(false))
                    {
                        continue;
                    }
                }
                if (string.Compare(criteriaInner, "OID") == 0)
                {
                    innerDict.Add(oidInn, oidInn);
                }
                else
                {
                    object valInner = await serializerInner.ReadFieldValueAsync(tiInner, oidInn, criteriaInner).ConfigureAwait(false);
                    if (valInner != null)//added when nullable types was added
                    {
                        innerDict.Add(oidInn, valInner);
                    }
                }
            }
            foreach (int outerOid in outerDict.Keys)
            {
                object val = outerDict[outerOid];
                foreach (int innerOid in innerDict.Keys)
                {
                    object valInner = innerDict[innerOid];
                    if (val.Equals(valInner))
                    {
                        KeyValuePair<int, int> kv = new KeyValuePair<int, int>(outerOid, innerOid);

                        oids.Add(kv);
                    }
                }
            }


            return oids;
        }

#endif
        internal object LoadValue(int oid, string fieldName, SqoTypeInfo ti)
        {
            if (fieldName == "OID")
            {
                return oid;
            }
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
            return serializer.ReadFieldValue(ti, oid, fieldName, this.rawSerializer);
        }
#if ASYNC
        internal async Task<object> LoadValueAsync(int oid, string fieldName, SqoTypeInfo ti)
        {
            if (fieldName == "OID")
            {
                return oid;
            }
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedReadComplexObjectAsync);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
          
            return await serializer.ReadFieldValueAsync(ti, oid, fieldName, this.rawSerializer).ConfigureAwait(false);
        }
#endif
        internal List<int> LoadAllOIDs(SqoTypeInfo ti)
        {
            List<int> oids = new List<int>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            int nrRecords = ti.Header.numberOfRecords;
            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (serializer.IsObjectDeleted(oid, ti))
                {
                    continue;
                }
                oids.Add(oid);
            }
            return oids;

        }
        #if ASYNC
        internal async Task<List<int>> LoadAllOIDsAsync(SqoTypeInfo ti)
        {
            List<int> oids = new List<int>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            int nrRecords = ti.Header.numberOfRecords;
            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                {
                    continue;
                }
                oids.Add(oid);
            }
            return oids;

        }
#endif
        internal List<int> LoadAllOIDs(string typeName)
        {
            SqoTypeInfo ti = GetSqoTypeInfo(typeName);
            if (ti == null)
            {
                return null;
            }
            return LoadAllOIDs(ti);
        }
#if ASYNC
        internal async Task<List<int>> LoadAllOIDsAsync(string typeName)
        {
            SqoTypeInfo ti = await GetSqoTypeInfoAsync(typeName).ConfigureAwait(false);
            if (ti == null)
            {
                return null;
            }
            return await LoadAllOIDsAsync(ti).ConfigureAwait(false);
        }
#endif
        internal object LoadObjectByOID(SqoTypeInfo ti, int oid, List<string> includes)
        {

            if (this.includePropertiesCache == null)
            {
                this.includePropertiesCache = new List<ATuple<Type, string>>();
            }
            foreach (string path in includes)
            {
                string[] arrayPath = path.Split('.');

                PropertyInfo property;
                Type type = ti.Type;
                foreach (var include in arrayPath)
                {
                    if ((property = type.GetProperty(include)) == null)
                    {
                        if (typeof(IList).IsAssignableFrom(type))
                        {
                            Type elementType = type.GetElementType();
                            if (elementType == null)
                            {
                                elementType = type.GetProperty("Item").PropertyType;
                            }
                            type = elementType;
                            if ((property = type.GetProperty(include)) == null)
                            {
                                throw new Sqo.Exceptions.SiaqodbException("Property:" + include + " does not belong to Type:" + type.FullName);
                            }

                        }
                        else
                        {
                            throw new Sqo.Exceptions.SiaqodbException("Property:" + include + " does not belong to Type:" + type.FullName);
                        }
                    }
                    string backingField = ExternalMetaHelper.GetBackingField(property);
                    if (!ExistsInIncludesCache(type, backingField))
                    {
                        includePropertiesCache.Add(new ATuple<Type, string>(type, backingField));
                    }
                    type = property.PropertyType;
                }

            }
            object obj = this.LoadObjectByOID(ti, oid);
            this.includePropertiesCache.Clear();

            return obj;

        }
#if ASYNC
        internal async Task<object> LoadObjectByOIDAsync(SqoTypeInfo ti, int oid, List<string> includes)
        {

            if (this.includePropertiesCache == null)
            {
                this.includePropertiesCache = new List<ATuple<Type, string>>();
            }
            foreach (string path in includes)
            {
                string[] arrayPath = path.Split('.');

                PropertyInfo property;
                Type type = ti.Type;
                foreach (var include in arrayPath)
                {
                    if ((property = type.GetProperty(include)) == null)
                    {
                        if (typeof(IList).IsAssignableFrom(type))
                        {
                            Type elementType = type.GetElementType();
                            if (elementType == null)
                            {
                                elementType = type.GetProperty("Item").PropertyType;
                            }
                            type = elementType;
                            if ((property = type.GetProperty(include)) == null)
                            {
                                throw new Sqo.Exceptions.SiaqodbException("Property:" + include + " does not belong to Type:" + type.FullName);
                            }

                        }
                        else
                        {
                            throw new Sqo.Exceptions.SiaqodbException("Property:" + include + " does not belong to Type:" + type.FullName);
                        }
                    }
                    string backingField = ExternalMetaHelper.GetBackingField(property);
                    if (!ExistsInIncludesCache(type, backingField))
                    {
                        includePropertiesCache.Add(new ATuple<Type, string>(type, backingField));
                    }
                    type = property.PropertyType;
                }

            }
            object obj = await this.LoadObjectByOIDAsync(ti, oid).ConfigureAwait(false);
            this.includePropertiesCache.Clear();

            return obj;

        }
#endif
        internal object LoadObjectByOID(SqoTypeInfo ti, int oid)
        {
            return this.LoadObjectByOID(ti, oid, true);
        }
#if ASYNC
        internal async Task<object> LoadObjectByOIDAsync(SqoTypeInfo ti, int oid)
        {
            return await this.LoadObjectByOIDAsync(ti, oid, true).ConfigureAwait(false);
        }
#endif

        internal object LoadObjectByOID(SqoTypeInfo ti, int oid, bool clearCache)
        {
            object currentObj = null;

            if (SiaqodbConfigurator.RaiseLoadEvents)
            {
                LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                this.OnLoadingObject(args);
                if (args.Cancel)
                {
                    return null;
                }
                else if (args.Replace != null)
                {
                    return args.Replace;

                }
            }

            currentObj = Activator.CreateInstance(ti.Type);
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);

            if (clearCache)
            {
                circularRefCache.Clear();
            }
            circularRefCache.Add(oid, ti, currentObj);
            try
            {
                serializer.ReadObject(currentObj, ti, oid, rawSerializer);
            }
            catch (ArgumentException ex)
            {
                SiaqodbConfigurator.LogMessage("Object with OID:" + oid.ToString() + " seems to be corrupted!", VerboseLevel.Error);
                 
                if (SiaqodbUtil.IsRepairMode)
                {
                    SiaqodbConfigurator.LogMessage("Object with OID:" + oid.ToString() + " is deleted", VerboseLevel.Warn);
                       
                    this.DeleteObjectByOID(oid, ti);
                    return null;

                }
                else throw ex;
            }
            metaCache.SetOIDToObject(currentObj, oid, ti);

            if (SiaqodbConfigurator.RaiseLoadEvents)
            {
                this.OnLoadedObject(oid, currentObj);
            }
            return currentObj;
        }
#if ASYNC
        internal async Task<object> LoadObjectByOIDAsync(SqoTypeInfo ti, int oid, bool clearCache)
        {
            object currentObj = null;

            if (SiaqodbConfigurator.RaiseLoadEvents)
            {
                LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                this.OnLoadingObject(args);
                if (args.Cancel)
                {
                    return null;
                }
                else if (args.Replace != null)
                {
                    return args.Replace;

                }
            }

            currentObj = Activator.CreateInstance(ti.Type);
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObjectAsync += new ComplexObjectEventHandler(serializer_NeedReadComplexObjectAsync);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
          
            if (clearCache)
            {
                circularRefCache.Clear();
            }
            circularRefCache.Add(oid, ti, currentObj);
            bool exTh = false;
            try
            {
                await serializer.ReadObjectAsync(currentObj, ti, oid, rawSerializer).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                SiaqodbConfigurator.LogMessage("Object with OID:" + oid.ToString() + " seems to be corrupted!", VerboseLevel.Error);

                if (SiaqodbUtil.IsRepairMode)
                {
                    SiaqodbConfigurator.LogMessage("Object with OID:" + oid.ToString() + " is deleted", VerboseLevel.Warn);

                    exTh = true;
                }
                else throw ex;
            }
            if (exTh)
            {
                await this.DeleteObjectByOIDAsync(oid, ti).ConfigureAwait(false);
                return null;

            }
            metaCache.SetOIDToObject(currentObj, oid, ti);

            if (SiaqodbConfigurator.RaiseLoadEvents)
            {
                this.OnLoadedObject(oid, currentObj);
            }
            return currentObj;
        }
        
#endif
        internal T LoadObjectByOID<T>(SqoTypeInfo ti, int oid)
        {

            return this.LoadObjectByOID<T>(ti, oid, true);

        }
#if ASYNC
        internal async Task<T> LoadObjectByOIDAsync<T>(SqoTypeInfo ti, int oid)
        {

            return await this.LoadObjectByOIDAsync<T>(ti, oid, true).ConfigureAwait(false);

        }
#endif
        internal T LoadObjectByOID<T>(SqoTypeInfo ti, int oid, bool clearCache)
        {


            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            if (oid > 0 && oid <= ti.Header.numberOfRecords && !serializer.IsObjectDeleted(oid, ti))
            {
                return (T)this.LoadObjectByOID(ti, oid, clearCache);
            }
            else
            {
                return default(T);
            }



        }
#if ASYNC
        internal async Task<T> LoadObjectByOIDAsync<T>(SqoTypeInfo ti, int oid, bool clearCache)
        {


            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            if (oid > 0 && oid <= ti.Header.numberOfRecords && !(await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false)))
            {
                return (T)(await this.LoadObjectByOIDAsync(ti, oid, clearCache).ConfigureAwait(false));
            }
            else
            {
                return default(T);
            }



        }
#endif
        internal int Count(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            int nrRecords = ti.Header.numberOfRecords;
            int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
            if (rangeSize < 1) rangeSize = 1;
           
            int count = 0;
            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (i % rangeSize == 0)
                {
                    int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                    serializer.PreLoadBytes(oid, oidEnd, ti);
                }
                if (serializer.IsObjectDeleted(oid, ti))
                {
                    continue;
                }
                count++;
            }
            serializer.ResetPreload();
            return count;
        }
#if ASYNC
        internal async Task<int> CountAsync(SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            int nrRecords = ti.Header.numberOfRecords;
            int rangeSize = Convert.ToInt32((SiaqodbConfigurator.BufferingChunkPercent * nrRecords / 100));
            if (rangeSize < 1) rangeSize = 1;
           
            int count = 0;
            for (int i = 0; i < nrRecords; i++)
            {
                int oid = i + 1;
                if (i % rangeSize == 0)
                {
                    int oidEnd = i + rangeSize <= nrRecords ? (i + rangeSize) : nrRecords;
                    await serializer.PreLoadBytesAsync(oid, oidEnd, ti).ConfigureAwait(false);
                }
                if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                {
                    continue;
                }
                count++;
            }
            serializer.ResetPreload();
            return count;
        }
#endif
        internal ATuple<int, int> GetArrayMetaOfField(SqoTypeInfo ti, int oid, FieldSqoInfo fi)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return serializer.GetArrayMetaOfField(ti, oid, fi);
        }
#if ASYNC
        internal async Task<ATuple<int, int>> GetArrayMetaOfFieldAsync(SqoTypeInfo ti, int oid, FieldSqoInfo fi)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return await serializer.GetArrayMetaOfFieldAsync(ti, oid, fi).ConfigureAwait(false);
        }
#endif
        private bool ExistsInIncludesCache(Type type, string fieldName)
        {
            if (this.includePropertiesCache == null)
            {
                return false;
            }
            foreach (ATuple<Type, string> tuple in this.includePropertiesCache)
            {
                if (tuple.Name == type && tuple.Value == fieldName)
                {
                    return true;
                }
            }
            return false;
        }

        internal KeyValuePair<int, int> LoadOIDAndTID(int oid, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return serializer.ReadOIDAndTID(ti, oid, fi);
        }
#if ASYNC
        internal async Task<KeyValuePair<int, int>> LoadOIDAndTIDAsync(int oid, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return await serializer.ReadOIDAndTIDAsync(ti, oid, fi).ConfigureAwait(false);
        }
#endif
        internal List<KeyValuePair<int, int>> LoadComplexArray(int oid, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return serializer.ReadComplexArrayOids(oid, fi, ti, this.rawSerializer);
        }
#if ASYNC
        internal async Task<List<KeyValuePair<int, int>>> LoadComplexArrayAsync(int oid, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return await serializer.ReadComplexArrayOidsAsync(oid, fi, ti, this.rawSerializer).ConfigureAwait(false);
        }
#endif
        internal int LoadComplexArrayTID(int oid, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return serializer.ReadFirstTID(oid, fi, ti, this.rawSerializer);
        }
#if ASYNC
        internal async Task<int> LoadComplexArrayTIDAsync(int oid, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return await serializer.ReadFirstTIDAsync(oid, fi, ti, this.rawSerializer).ConfigureAwait(false);
        }
#endif
        internal bool IsObjectDeleted(int oid, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            if (oid > 0 && oid <= ti.Header.numberOfRecords && !serializer.IsObjectDeleted(oid, ti))
            {
                return false;
            }
            return true;
        }
#if ASYNC
        internal async Task<bool> IsObjectDeletedAsync(int oid, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            if (oid > 0 && oid <= ti.Header.numberOfRecords && !(await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false)))
            {
                return false;
            }
            return true;
        }
#endif

         internal List<int> GetUsedRawdataInfoOIDs(SqoTypeInfo ti)
         {
             List<int> existingRawdataInfoOIDs = new List<int>();
             ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
             int nrRecords = ti.Header.numberOfRecords;
             List<FieldSqoInfo> existingDynamicFields = new List<FieldSqoInfo>();
             foreach (FieldSqoInfo ai in ti.Fields)
             {
                 IByteTransformer byteTrans = ByteTransformerFactory.GetByteTransformer(null, null, ai, ti);
                 if (byteTrans is ArrayByteTranformer || byteTrans is DictionaryByteTransformer)
                 {
                     existingDynamicFields.Add(ai);
                 }
             }
             if (existingDynamicFields.Count > 0)
             {
                 for (int i = 0; i < nrRecords; i++)
                 {

                     int oid = i + 1;
                     if (serializer.IsObjectDeleted(oid, ti))
                     {
                         continue;
                     }
                     foreach (FieldSqoInfo ai in existingDynamicFields)
                     {
                         ATuple<int, int> arrayInfo = this.GetArrayMetaOfField(ti, oid, ai);
                         if (arrayInfo.Name > 0)
                         {
                             existingRawdataInfoOIDs.Add(arrayInfo.Name);
                         }
                     }
                 }
             }
             return existingRawdataInfoOIDs;
         }
#if ASYNC
         internal async Task<List<int>> GetUsedRawdataInfoOIDsAsync(SqoTypeInfo ti)
         {
             List<int> existingRawdataInfoOIDs = new List<int>();
             ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
             int nrRecords = ti.Header.numberOfRecords;
             List<FieldSqoInfo> existingDynamicFields = new List<FieldSqoInfo>();
             foreach (FieldSqoInfo ai in ti.Fields)
             {
                 IByteTransformer byteTrans = ByteTransformerFactory.GetByteTransformer(null, null, ai, ti);
                 if (byteTrans is ArrayByteTranformer || byteTrans is DictionaryByteTransformer)
                 {
                     existingDynamicFields.Add(ai);
                 }
             }
             if (existingDynamicFields.Count > 0)
             {
                 for (int i = 0; i < nrRecords; i++)
                 {

                     int oid = i + 1;
                     if (await serializer.IsObjectDeletedAsync(oid, ti).ConfigureAwait(false))
                     {
                         continue;
                     }
                     foreach (FieldSqoInfo ai in existingDynamicFields)
                     {
                         ATuple<int, int> arrayInfo = await this.GetArrayMetaOfFieldAsync(ti, oid, ai).ConfigureAwait(false);
                         if (arrayInfo.Name > 0)
                         {
                             existingRawdataInfoOIDs.Add(arrayInfo.Name);
                         }
                     }
                 }
             }
             return existingRawdataInfoOIDs;
         }
#endif
         internal List<int> LoadOidsByField(SqoTypeInfo ti, string fieldName, object obj)
         {
             ObjectInfo objInfo = MetaExtractor.GetObjectInfo(obj, ti, metaCache);
             FieldSqoInfo fi = MetaHelper.FindField(ti.Fields, fieldName);
             if (fi == null)
             {
                 throw new SiaqodbException("Field:" + fieldName + " was not found as member of Type:" + ti.TypeName);
             }
             Where w = new Where(fi.Name, OperationType.Equal, objInfo.AtInfo[fi]);

             return this.LoadFilteredOids(w, ti);
         }
    }
}
