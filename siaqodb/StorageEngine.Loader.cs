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
using LightningDB;

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
            string dbName=GetFileByType(ti);
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, dbName, useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);
            int nrRecords = ti.Header.numberOfRecords;
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(dbName, DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    using (var cursor = transaction.CreateCursor(db))
                    {
                        var current = cursor.MoveNext();

                        while (current.HasValue)
                        {
                            byte[] crObjBytes = current.Value.Value;
                            byte[] oidBytes = current.Value.Key;
                            int oid = ByteConverter.ByteArrayToInt(oidBytes);
                            if (crObjBytes != null)
                            {
                               
                                if (SiaqodbConfigurator.RaiseLoadEvents)
                                {
                                    LoadingObjectEventArgs args = new LoadingObjectEventArgs(oid, ti.Type);
                                    this.OnLoadingObject(args);
                                    if (args.Cancel)
                                    {
                                        current = cursor.MoveNext();
                                        continue;
                                    }
                                    else if (args.Replace != null)
                                    {
                                        ol.Add((T)args.Replace);
                                        current = cursor.MoveNext();
                                        continue;
                                    }
                                }
                                T currentObj = default(T);
                                currentObj = Activator.CreateInstance<T>();
                                circularRefCache.Clear();
                                circularRefCache.Add(oid, ti, currentObj);

                                serializer.ReadObject<T>(currentObj, crObjBytes, ti, oid, this.rawSerializer,transaction);

                                metaCache.SetOIDToObject(currentObj, oid, ti);
                                if (SiaqodbConfigurator.RaiseLoadEvents)
                                {
                                    this.OnLoadedObject(oid, currentObj);
                                }
                                ol.Add(currentObj);
                            }
                            current = cursor.MoveNext();
                        }
                    }
                    
                }
               
            }
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


                ObjectRow row = obTable.NewRow();
                row["OID"] = oid;
                serializer.ReadObjectRow(row, ti, oid, rawSerializer);

                obTable.Rows.Add(row);

            }
           
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
            string dbName=GetFileByType(ti);
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, dbName, useElevatedTrust);
            insideOids.Sort();
            int nrRecords = ti.Header.numberOfRecords;
            
            List<int> oids = new List<int>();
            if (insideOids.Count == 0)
                return oids;
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(dbName, DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    using (var cursor = transaction.CreateCursor(db))
                    {
                        var current = cursor.MoveNext();

                        while (current.HasValue)
                        {
                            byte[] crObjBytes = current.Value.Value;
                            byte[] oidBytes = current.Value.Key;
                            int oid = ByteConverter.ByteArrayToInt(oidBytes);
                            if (crObjBytes != null)
                            {
                                
                                int oidOfComplex = serializer.ReadOidOfComplex(ti, oid, crObjBytes,fieldName, this.rawSerializer);

                                int index = insideOids.BinarySearch(oidOfComplex);//intersection
                                if (index >= 0)
                                {
                                    oids.Add(oid);
                                }

                            }
                            current = cursor.MoveNext();
                        }
                    }
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
            var transaction=transactionManager.GetActiveTransaction();
            {
                if (!indexManager.LoadOidsByIndex(ti, where.AttributeName[0], where, oids, transaction))
                {
                    var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                    {
                        using (var cursor = transaction.CreateCursor(db))
                        {
                            var current = cursor.MoveNext();

                            while (current.HasValue)
                            {
                                byte[] crObjBytes = current.Value.Value;
                                byte[] oidBytes = current.Value.Key;
                                int oid = ByteConverter.ByteArrayToInt(oidBytes);
                                if (crObjBytes != null)
                                {
                                   
                                    object val = isOIDField ? oid : serializer.ReadFieldValue(ti, oid, crObjBytes, where.AttributeName[0], this.rawSerializer, transaction);

                                    if (Match(where, val))
                                    {
                                        oids.Add(oid);
                                    }

                                }
                                current = cursor.MoveNext();
                            }

                        }
                    }

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

        internal IObjectList<T> LoadByOIDs<T>(List<int> oids, SqoTypeInfo ti)
        {
            ObjectList<T> ol = new ObjectList<T>();
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs> (serializer_NeedCacheDocument);
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
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

                        byte[] key = ByteConverter.IntToByteArray(oid);

                        byte[] objBytes = transaction.Get(db, key);
                        serializer.ReadObject<T>(currentObj, objBytes, ti, oid, rawSerializer, transaction);

                        metaCache.SetOIDToObject(currentObj, oid, ti);
                        if (SiaqodbConfigurator.RaiseLoadEvents)
                        {
                            this.OnLoadedObject(oid, currentObj);
                        }
                        ol.Add(currentObj);
                    }
                }
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

                e.ComplexObject = this.LoadObjectByOID(ti, e.SavedOID, false,e.Transaction);


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
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
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

                        byte[] key = ByteConverter.IntToByteArray(oid);

                        byte[] objBytes = transaction.Get(db, key);
                        serializer.ReadObject(currentObj, objBytes, ti, oid, rawSerializer, transaction);

                        metaCache.SetOIDToObject(currentObj, oid, ti);
                        if (SiaqodbConfigurator.RaiseLoadEvents)
                        {
                            this.OnLoadedObject(oid, currentObj);
                        }
                        ol.Add(currentObj);
                    }
                }
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
        internal object LoadValue(int oid, string fieldName, SqoTypeInfo ti)
        {
            return this.LoadValue(oid, fieldName, ti, null);
        }
        internal object LoadValue(int oid, string fieldName, SqoTypeInfo ti, LightningTransaction transaction)
        {
            if (fieldName == "OID")
            {
                return oid;
            }
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            serializer.NeedReadComplexObject += new EventHandler<ComplexObjectEventArgs>(serializer_NeedReadComplexObject);
            serializer.NeedCacheDocument += new EventHandler<DocumentEventArgs>(serializer_NeedCacheDocument);

            var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);

            byte[] key = ByteConverter.IntToByteArray(oid);

            byte[] objBytes = transaction.Get(db, key);
            var fieldVal = serializer.ReadFieldValue(ti, oid, objBytes, fieldName, this.rawSerializer, transaction);
            
            return fieldVal;




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


            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    using (var cursor = transaction.CreateCursor(db))
                    {
                        var current = cursor.MoveNext();

                        while (current.HasValue)
                        {
                            byte[] crObjBytes = current.Value.Value;
                            byte[] oidBytes = current.Value.Key;
                            int oid = ByteConverter.ByteArrayToInt(oidBytes);
                            if (crObjBytes != null)
                            {
                               
                                oids.Add(oid);
                            }
                            current = cursor.MoveNext();
                        }
                    }
                }
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
            return this.LoadObjectByOID(ti, oid, true,transactionManager.GetActiveTransaction());
        }
#if ASYNC
        internal async Task<object> LoadObjectByOIDAsync(SqoTypeInfo ti, int oid)
        {
            return await this.LoadObjectByOIDAsync(ti, oid, true).ConfigureAwait(false);
        }
#endif

        internal object LoadObjectByOID(SqoTypeInfo ti, int oid, bool clearCache, LightningDB.LightningTransaction transaction)
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

            var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
            {
                byte[] key = ByteConverter.IntToByteArray(oid);

                byte[] objBytes = transaction.Get(db, key);
                if (objBytes == null )
                    return null;
                serializer.ReadObject(currentObj, objBytes, ti, oid, rawSerializer, transaction);
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
            return this.LoadObjectByOID<T>(ti, oid, clearCache, transactionManager.GetActiveTransaction());
        }
        internal T LoadObjectByOID<T>(SqoTypeInfo ti, int oid, bool clearCache,LightningTransaction transaction)
        {
            

            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);

            if (oid > 0 && oid <= ti.Header.numberOfRecords )
            {
                return (T)this.LoadObjectByOID(ti, oid, clearCache,transaction);
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


            int count = 0;
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    using (var cursor = transaction.CreateCursor(db))
                    {
                        var current = cursor.MoveNext();

                        while (current.HasValue)
                        {
                            byte[] crObjBytes = current.Value.Value;
                           
                            if (crObjBytes != null)
                            {
                                
                                count++;
                            }
                            current = cursor.MoveNext();
                        }
                    }
                }
            }


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
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    byte[] key = ByteConverter.IntToByteArray(oid);

                    byte[] objBytes = transaction.Get(db, key);
                    return serializer.ReadOIDAndTID(ti, oid,objBytes, fi);
                }
            }
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
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    byte[] key = ByteConverter.IntToByteArray(oid);

                    byte[] objBytes = transaction.Get(db, key);
                    return serializer.ReadComplexArrayOids(oid,objBytes, fi, ti, this.rawSerializer,transaction);
                }
            }
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
            var transaction=transactionManager.GetActiveTransaction();
            {
                var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);
                {
                    byte[] key = ByteConverter.IntToByteArray(oid);

                    byte[] objBytes = transaction.Get(db, key);
                    return serializer.ReadFirstTID(oid, objBytes, fi, ti, this.rawSerializer, transaction);
                }
            }
        }
#if ASYNC
        internal async Task<int> LoadComplexArrayTIDAsync(int oid, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
            return await serializer.ReadFirstTIDAsync(oid, fi, ti, this.rawSerializer).ConfigureAwait(false);
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
         internal List<ATuple<int, object>> GetAllValues(SqoTypeInfo ti, FieldSqoInfo fi, LightningDB.LightningTransaction transaction)
         {
             ObjectSerializer serializer = SerializerFactory.GetSerializer(this.path, GetFileByType(ti), useElevatedTrust);
             List<ATuple<int, object>> all = new List<ATuple<int, object>>();
             var db = transaction.OpenDatabase(GetFileByType(ti), DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey);

             using (var cursor = transaction.CreateCursor(db))
             {
                 var current = cursor.MoveNext();

                 while (current.HasValue)
                 {
                     byte[] crObjBytes = current.Value.Value;
                     byte[] oidBytes = current.Value.Key;
                     int oid = ByteConverter.ByteArrayToInt(oidBytes);
                     if (crObjBytes != null)
                     {
                        
                        object value= serializer.ReadFieldValue(ti, oid, crObjBytes, fi.Name, this.rawSerializer, transaction);
                        ATuple<int, object> retVal = new ATuple<int, object>(oid, value);
                        all.Add(retVal);
                     }
                     current = cursor.MoveNext();
                 }
             }
             return all;
         }
   
       
    }
}
