using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sqo;
using Sqo.Core;
using Sqo.Meta;
using Sqo.Exceptions;
using Sqo.Utilities;
using System.Collections;
using System.Reflection;
using Sqo.MetaObjects;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Core
{
    partial class ObjectSerializer
    {
        public void ReadObject(object obj, byte[] objBytes, SqoTypeInfo ti, int oid, RawdataSerializer rawSerializer, LightningDB.LightningTransaction transaction)
        {


            int fieldPosition = 0;
            byte[] oidBuff = GetFieldBytes(objBytes, fieldPosition, 4);
            int oidFromFile = ByteConverter.ByteArrayToInt(oidBuff);
            //eventual make comparison

            foreach (FieldSqoInfo ai in ti.Fields)
            {
                byte[] field = GetFieldBytes(objBytes, ai.Header.PositionInRecord, ai.Header.Length);

                IByteTransformer byteTransformer = ByteTransformerFactory.GetByteTransformer(this, rawSerializer, ai, ti, oid);

                object fieldVal = null;

                fieldVal = byteTransformer.GetObject(field,transaction);

                if (ai.AttributeTypeId == MetaExtractor.documentID)
                {
                    DocumentInfo dinfo = fieldVal as DocumentInfo;
                    if (dinfo != null)
                    {
                        if (SiaqodbConfigurator.DocumentSerializer == null)
                        {
                            throw new SiaqodbException("Document serializer is not set, use SiaqodbConfigurator.SetDocumentSerializer method to set it");
                        }
                        fieldVal = SiaqodbConfigurator.DocumentSerializer.Deserialize(ai.AttributeType, dinfo.Document);
                        //put in weak cache to be able to update the document
                        DocumentEventArgs args = new DocumentEventArgs();
                        args.ParentObject = obj;
                        args.DocumentInfoOID = dinfo.OID;
                        args.FieldName = ai.Name;
                        args.TypeInfo = ti;
                        this.OnNeedCacheDocument(args);
                    }
                }

#if SILVERLIGHT

                    try
                    {
                        //dobj.SetValue(ai.FInfo, ByteConverter.DeserializeValueType(ai.FInfo.FieldType, field));
                        MetaHelper.CallSetValue(ai.FInfo,fieldVal, obj, ti.Type);
                    }
                    catch (Exception ex)
                    {
                        throw new SiaqodbException("Override GetValue and SetValue methods of SqoDataObject-Silverlight limitation to private fields");
                    }


#else
                ai.FInfo.SetValue(obj, fieldVal);
#endif
            }




        }
#if ASYNC
        public async Task ReadObjectAsync(object obj, SqoTypeInfo ti, int oid, RawdataSerializer rawSerializer)
        {

            //long position = (long)ti.Header.headerSize + (long)((long)(oid - 1) * (long)ti.Header.lengthOfRecord);
            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            byte[] b = new byte[recordLength];
            if (oidEnd == 0 && oidStart == 0)
            {
                await file.ReadAsync(position, b).ConfigureAwait(false);
            }
            else
            {
                int recordPosition = (oid - oidStart) * recordLength;
                Array.Copy(preloadedBytes, recordPosition, b, 0, b.Length);
            }
            int fieldPosition = 0;
            byte[] oidBuff = GetFieldBytes(b, fieldPosition, 4);
            int oidFromFile = ByteConverter.ByteArrayToInt(oidBuff);
            //eventual make comparison

            foreach (FieldSqoInfo ai in ti.Fields)
            {
                byte[] field = GetFieldBytes(b, ai.Header.PositionInRecord, ai.Header.Length);

                IByteTransformer byteTransformer = ByteTransformerFactory.GetByteTransformer(this, rawSerializer, ai, ti);
                object fieldVal = null;
                try
                {
                    fieldVal =  await byteTransformer.GetObjectAsync(field).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ti.Type != null && ti.Type.IsGenericType() && ti.Type.GetGenericTypeDefinition() == typeof(Indexes.BTreeNode<>))
                    {
                        throw new IndexCorruptedException();
                    }
                    SiaqodbConfigurator.LogMessage("Field's" + ai.Name + " value of Type " + ti.TypeName + "cannot be loaded, will be set to default.", VerboseLevel.Info);
                    fieldVal = MetaHelper.GetDefault(ai.AttributeType);
                }
                if (ai.AttributeTypeId == MetaExtractor.documentID)
                {
                    DocumentInfo dinfo = fieldVal as DocumentInfo;
                    if (dinfo != null)
                    {
                        if (SiaqodbConfigurator.DocumentSerializer == null)
                        {
                            throw new SiaqodbException("Document serializer is not set, use SiaqodbConfigurator.SetDocumentSerializer method to set it");
                 
                        }
                        fieldVal = SiaqodbConfigurator.DocumentSerializer.Deserialize(ai.AttributeType, dinfo.Document);
                    
                        //put in weak cache to be able to update the document
                        DocumentEventArgs args = new DocumentEventArgs();
                        args.ParentObject = obj;
                        args.DocumentInfoOID = dinfo.OID;
                        args.FieldName = ai.Name;
                        args.TypeInfo = ti;
                        this.OnNeedCacheDocument(args);
                    }
                }

#if SILVERLIGHT

                    try
                    {
                        //dobj.SetValue(ai.FInfo, ByteConverter.DeserializeValueType(ai.FInfo.FieldType, field));
                        MetaHelper.CallSetValue(ai.FInfo,fieldVal, obj, ti.Type);
                    }
                    catch (Exception ex)
                    {
                        throw new SiaqodbException("Override GetValue and SetValue methods of SqoDataObject-Silverlight limitation to private fields");
                    }


#else
                ai.FInfo.SetValue(obj, fieldVal);
#endif
            }




        }
#endif
        public object ReadComplexObject(byte[] field, Type parentType, string fieldName, LightningDB.LightningTransaction transaction)
        {
            byte[] oidOfComplexObjBuff = GetFieldBytes(field, 0, 4);
            int oidOfComplexObj = ByteConverter.ByteArrayToInt(oidOfComplexObjBuff);
            byte[] tidOfComplexObjBuff = GetFieldBytes(field, 4, 4);
            int tidOfComplexObj = ByteConverter.ByteArrayToInt(tidOfComplexObjBuff);
            ComplexObjectEventArgs args = new ComplexObjectEventArgs(oidOfComplexObj, tidOfComplexObj);
            args.ParentType = parentType;
            args.FieldName = fieldName;
            args.Transaction = transaction;
            this.OnNeedReadComplexObject(args);
            return args.ComplexObject;
        }
#if ASYNC
        public async Task<object> ReadComplexObjectAsync(byte[] field, Type parentType, string fieldName)
        {
            byte[] oidOfComplexObjBuff = GetFieldBytes(field, 0, 4);
            int oidOfComplexObj = ByteConverter.ByteArrayToInt(oidOfComplexObjBuff);
            byte[] tidOfComplexObjBuff = GetFieldBytes(field, 4, 4);
            int tidOfComplexObj = ByteConverter.ByteArrayToInt(tidOfComplexObjBuff);
            ComplexObjectEventArgs args = new ComplexObjectEventArgs(oidOfComplexObj, tidOfComplexObj);
            args.ParentType = parentType;
            args.FieldName = fieldName;
            await this.OnNeedReadComplexObjectAsync(args).ConfigureAwait(false);
            return args.ComplexObject;
        }
#endif
        public void ReadObject<T>(T obj, byte[] objBytes, SqoTypeInfo ti, int oid, RawdataSerializer rawSerializer,LightningDB.LightningTransaction transaction)
        {
            this.ReadObject((object)obj, objBytes, ti, oid, rawSerializer,transaction);
        }
        #if ASYNC
        public async Task ReadObjectAsync<T>(T obj, SqoTypeInfo ti, int oid, RawdataSerializer rawSerializer)
        {
            await this.ReadObjectAsync((object)obj, ti, oid, rawSerializer).ConfigureAwait(false);
        }
#endif
        public object ReadFieldValue(SqoTypeInfo ti, int oid,byte[] objBytes, string fieldName,LightningDB.LightningTransaction transaction)
        {
            return ReadFieldValue(ti, oid, objBytes, fieldName, null, transaction);
        }
        #if ASYNC
        public async Task<object> ReadFieldValueAsync(SqoTypeInfo ti, int oid, string fieldName)
        {
            return await ReadFieldValueAsync(ti, oid, fieldName, null).ConfigureAwait(false);
        }
#endif
        public object ReadFieldValue(SqoTypeInfo ti, int oid, byte[] objBytes, FieldSqoInfo fi, LightningDB.LightningTransaction transaction)
        {
            return this.ReadFieldValue(ti, oid, objBytes, fi, null, transaction);
        }
        #if ASYNC
        public async Task<object> ReadFieldValueAsync(SqoTypeInfo ti, int oid, FieldSqoInfo fi)
        {
            return await this.ReadFieldValueAsync(ti, oid, fi, null).ConfigureAwait(false);
        }
#endif
        public object ReadFieldValue(SqoTypeInfo ti, int oid,byte[] objBytes, string fieldName, RawdataSerializer rawSerializer,LightningDB.LightningTransaction transaction)
        {

            FieldSqoInfo fi = FindField(ti.Fields, fieldName);
            if (fi == null)
            {
                throw new SiaqodbException("Field:" + fieldName + " not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            return this.ReadFieldValue(ti, oid,objBytes, fi, rawSerializer,transaction);

        }
        #if ASYNC
        public async Task<object> ReadFieldValueAsync(SqoTypeInfo ti, int oid, string fieldName, RawdataSerializer rawSerializer)
        {

            FieldSqoInfo fi = FindField(ti.Fields, fieldName);
            if (fi == null)
            {
                throw new SiaqodbException("Field:" + fieldName + " not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            return await this.ReadFieldValueAsync(ti, oid, fi, rawSerializer).ConfigureAwait(false);

        }
#endif
        public object ReadFieldValue(SqoTypeInfo ti, int oid, byte[] objBytes, FieldSqoInfo fi, RawdataSerializer rawSerializer, LightningDB.LightningTransaction transaction)
        {

            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];

            Array.Copy(objBytes, fi.Header.PositionInRecord, b, 0, b.Length);

            IByteTransformer byteTransformer = ByteTransformerFactory.GetByteTransformer(this, rawSerializer, fi, ti, oid);

            return byteTransformer.GetObject(b,transaction);



        }
#if ASYNC
        public async Task<object> ReadFieldValueAsync(SqoTypeInfo ti, int oid, FieldSqoInfo fi, RawdataSerializer rawSerializer)
        {

            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];
            if (oidEnd == 0 && oidStart == 0)
            {
                await file.ReadAsync((long)(position + (long)fi.Header.PositionInRecord), b).ConfigureAwait(false);
            }
            else
            {
                int fieldPosition = (oid - oidStart) * recordLength + fi.Header.PositionInRecord;
                Array.Copy(preloadedBytes, fieldPosition, b, 0, b.Length);
            }
            IByteTransformer byteTransformer = ByteTransformerFactory.GetByteTransformer(this, rawSerializer, fi, ti);
            try
            {
                return await byteTransformer.GetObjectAsync(b).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ti.Type != null && ti.Type.IsGenericType() && ti.Type.GetGenericTypeDefinition() == typeof(Indexes.BTreeNode<>))
                {
                    throw new IndexCorruptedException();
                }
                SiaqodbConfigurator.LogMessage("Field's" + fi.Name + " value of Type " + ti.TypeName + "cannot be loaded,will be set to default.", VerboseLevel.Info);
                return MetaHelper.GetDefault(fi.AttributeType);
            }

        }
#endif
        public KeyValuePair<int, int> ReadOIDAndTID(SqoTypeInfo ti, int oid,byte[] objBytes, FieldSqoInfo fi)
        {

            byte[] b = new byte[fi.Header.Length];
            Array.Copy(objBytes, fi.Header.PositionInRecord, b, 0, b.Length);

            return ReadOIDAndTID(b);

        }
#if ASYNC
        public async Task<KeyValuePair<int, int>> ReadOIDAndTIDAsync(SqoTypeInfo ti, int oid, FieldSqoInfo fi)
        {

            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];

            await file.ReadAsync((long)(position + (long)fi.Header.PositionInRecord), b).ConfigureAwait(false);

            return ReadOIDAndTID(b);

        }
#endif
        public KeyValuePair<int, int> ReadOIDAndTID(byte[] b)
        {
            byte[] oidOfComplexObjBuff = GetFieldBytes(b, 0, 4);
            int oidOfComplexObj = ByteConverter.ByteArrayToInt(oidOfComplexObjBuff);
            byte[] tidOfComplexObjBuff = GetFieldBytes(b, 4, 4);
            int tidOfComplexObj = ByteConverter.ByteArrayToInt(tidOfComplexObjBuff);
            return new KeyValuePair<int, int>(oidOfComplexObj, tidOfComplexObj);
        }
        public int ReadOidOfComplex(SqoTypeInfo ti, int oid,byte[] objBytes, string fieldName, RawdataSerializer rawSerializer)
        {
            FieldSqoInfo fi = FindField(ti.Fields, fieldName);
            if (fi == null)
            {
                throw new SiaqodbException("Field:" + fieldName + " not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            return this.ReadOidOfComplex(ti, oid,objBytes, fi, rawSerializer);
        }
#if ASYNC
        public async Task<int> ReadOidOfComplexAsync(SqoTypeInfo ti, int oid, string fieldName, RawdataSerializer rawSerializer)
        {
            FieldSqoInfo fi = FindField(ti.Fields, fieldName);
            if (fi == null)
            {
                throw new SiaqodbException("Field:" + fieldName + " not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            return await this.ReadOidOfComplexAsync(ti, oid, fi, rawSerializer).ConfigureAwait(false);
        }
#endif
        public int ReadOidOfComplex(SqoTypeInfo ti, int oid,byte[] objBytes, FieldSqoInfo fi, RawdataSerializer rawSerializer)
        {
            
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
           
            byte[] oidOfComplexObjBuff = GetFieldBytes(objBytes, fi.Header.PositionInRecord, 4);
            int oidOfComplexObj = ByteConverter.ByteArrayToInt(oidOfComplexObjBuff);
            return oidOfComplexObj;
        }
#if ASYNC
        public async Task<int> ReadOidOfComplexAsync(SqoTypeInfo ti, int oid, FieldSqoInfo fi, RawdataSerializer rawSerializer)
        {
            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];

            await file.ReadAsync((long)(position + (long)fi.Header.PositionInRecord), b).ConfigureAwait(false);

            byte[] oidOfComplexObjBuff = GetFieldBytes(b, 0, 4);
            int oidOfComplexObj = ByteConverter.ByteArrayToInt(oidOfComplexObjBuff);
            return oidOfComplexObj;
        }
#endif
       
        internal void ReadObjectRow(Sqo.Utilities.ObjectRow row, SqoTypeInfo ti, int oid, RawdataSerializer rawSerializer)
        {
            //TODO LMDB
            /*

            lock (file)
            {
                // long position = (long)ti.Header.headerSize + (long)((long)(oid - 1) * (long)ti.Header.lengthOfRecord);
                long position = MetaHelper.GetSeekPosition(ti, oid);
                int recordLength = ti.Header.lengthOfRecord;
                byte[] b = new byte[recordLength];
                if (oidStart == 0 && oidEnd == 0)
                {
                    file.Read(position, b);
                }
                else
                {
                    int recordPosition = (oid - oidStart) * recordLength;
                    Array.Copy(preloadedBytes, recordPosition, b, 0, b.Length);
                }
                int fieldPosition = 0;
                byte[] oidBuff = GetFieldBytes(b, fieldPosition, 4);
                int oidFromFile = ByteConverter.ByteArrayToInt(oidBuff);

                foreach (FieldSqoInfo ai in ti.Fields)
                {
                    byte[] field = GetFieldBytes(b, ai.Header.PositionInRecord, ai.Header.Length);
                    if (typeof(IList).IsAssignableFrom(ai.AttributeType) || ai.IsText || ai.AttributeTypeId == MetaExtractor.complexID || ai.AttributeTypeId==MetaExtractor.dictionaryID || ai.AttributeTypeId==MetaExtractor.documentID)
                    {
                        row[ai.Name] = field;
                    }
                    else
                    {
                        try
                        {
                            row[ai.Name] = ByteConverter.DeserializeValueType(ai.AttributeType, field, true, ti.Header.version);
                        }
                        catch (Exception ex)
                        {

                            SiaqodbConfigurator.LogMessage("Field's" + ai.Name + " value of Type " + ti.TypeName + "cannot be loaded,will be set to default.", VerboseLevel.Info);
                            row[ai.Name] = MetaHelper.GetDefault(ai.AttributeType);
                        }
                    }

                }
            }

            */
        }
#if ASYNC
        internal async Task ReadObjectRowAsync(Sqo.Utilities.ObjectRow row, SqoTypeInfo ti, int oid, RawdataSerializer rawSerializer)
        {

            // long position = (long)ti.Header.headerSize + (long)((long)(oid - 1) * (long)ti.Header.lengthOfRecord);
            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            byte[] b = new byte[recordLength];
            if (oidStart == 0 && oidEnd == 0)
            {
                await file.ReadAsync(position, b).ConfigureAwait(false);
            }
            else
            {
                int recordPosition = (oid - oidStart) * recordLength;
                Array.Copy(preloadedBytes, recordPosition, b, 0, b.Length);
            }
            int fieldPosition = 0;
            byte[] oidBuff = GetFieldBytes(b, fieldPosition, 4);
            int oidFromFile = ByteConverter.ByteArrayToInt(oidBuff);

            foreach (FieldSqoInfo ai in ti.Fields)
            {
                byte[] field = GetFieldBytes(b, ai.Header.PositionInRecord, ai.Header.Length);
                if (typeof(IList).IsAssignableFrom(ai.AttributeType) || ai.IsText || ai.AttributeTypeId == MetaExtractor.complexID || ai.AttributeTypeId == MetaExtractor.dictionaryID || ai.AttributeTypeId == MetaExtractor.documentID)
                {
                    row[ai.Name] = field;
                }
                else
                {
                    try
                    {
                        row[ai.Name] = ByteConverter.DeserializeValueType(ai.AttributeType, field, true, ti.Header.version);
                    }
                    catch (Exception ex)
                    {

                        SiaqodbConfigurator.LogMessage("Field's" + ai.Name + " value of Type " + ti.TypeName + "cannot be loaded,will be set to default.", VerboseLevel.Info);
                        row[ai.Name] = MetaHelper.GetDefault(ai.AttributeType);
                    }
                }

            }



        }
#endif
        internal byte[] ReadObjectBytes(int oid, SqoTypeInfo ti)
        {
            return null;
            //TODO LMDB
            /*long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            byte[] b = new byte[recordLength];
            file.Read(position, b);
            return b;*/
        }
#if ASYNC 
        internal async Task<byte[]> ReadObjectBytesAsync(int oid, SqoTypeInfo ti)
        {
            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            byte[] b = new byte[recordLength];
            await file.ReadAsync(position, b).ConfigureAwait(false);
            return b;
        }
#endif
       
        [System.Reflection.Obfuscation(Exclude = true)]
        private EventHandler<ComplexObjectEventArgs> needReadComplexObject;
        [System.Reflection.Obfuscation(Exclude = true)]
        public event EventHandler<ComplexObjectEventArgs> NeedReadComplexObject
        {
            add
            {
                lock (_syncRoot)
                {
                    if (needReadComplexObject == null)
                    {
                        needReadComplexObject += value;
                    }
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    needReadComplexObject -= value;
                }
            }
        }
        protected void OnNeedReadComplexObject(ComplexObjectEventArgs args)
        {
            EventHandler<ComplexObjectEventArgs> handler;
            lock (_syncRoot)
            {
                handler = needReadComplexObject;
            }
            if (handler != null)
            {
                handler(this, args);
            }
        }
        [System.Reflection.Obfuscation(Exclude = true)]
        private EventHandler<DocumentEventArgs> needCacheDocument;
        [System.Reflection.Obfuscation(Exclude = true)]
        public event EventHandler<DocumentEventArgs> NeedCacheDocument
        {
            add
            {
                lock (_syncRoot)
                {
                    if (needCacheDocument == null)
                    {
                        needCacheDocument += value;
                    }
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    needCacheDocument -= value;
                }
            }
        }
        protected void OnNeedCacheDocument(DocumentEventArgs args)
        {
            EventHandler<DocumentEventArgs> handler;
            lock (_syncRoot)
            {
                handler = needCacheDocument;
            }
            if (handler != null)
            {
                handler(this, args);
            }
        }
#if ASYNC
        [System.Reflection.Obfuscation(Exclude = true)]
        private ComplexObjectEventHandler needReadComplexObjectAsync;
        [System.Reflection.Obfuscation(Exclude = true)]
        public event ComplexObjectEventHandler NeedReadComplexObjectAsync
        {
            add
            {
                lock (_syncRoot)
                {
                    if (needReadComplexObjectAsync == null)
                    {
                        needReadComplexObjectAsync += value;
                    }
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    needReadComplexObjectAsync -= value;
                }
            }
        }
        protected async Task OnNeedReadComplexObjectAsync(ComplexObjectEventArgs args)
        {
            ComplexObjectEventHandler handler;
            lock (_syncRoot)
            {
                handler = needReadComplexObjectAsync;
            }
            if (handler != null)
            {
                await handler(this, args).ConfigureAwait(false);
            }
        }

#endif
        internal List<KeyValuePair<int, int>> ReadComplexArrayOids(int oid,byte[] objBytes, FieldSqoInfo fi, SqoTypeInfo ti, RawdataSerializer rawdataSerializer,LightningDB.LightningTransaction transaction)
        {
            
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];

            Array.Copy(objBytes, fi.Header.PositionInRecord, b, 0, b.Length);

            return rawdataSerializer.ReadComplexArrayOids(b, ti.Header.version, this,transaction);
        }
#if ASYNC
        internal async Task<List<KeyValuePair<int, int>>> ReadComplexArrayOidsAsync(int oid, FieldSqoInfo fi, SqoTypeInfo ti, RawdataSerializer rawdataSerializer)
        {
            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];

            await file.ReadAsync((long)(position + (long)fi.Header.PositionInRecord), b).ConfigureAwait(false);
            return rawdataSerializer.ReadComplexArrayOids(b, ti.Header.version, this);
        }
#endif
        internal int ReadFirstTID(int oid, byte[] objBytes, FieldSqoInfo fi, SqoTypeInfo ti, RawdataSerializer rawdataSerializer,LightningDB.LightningTransaction transaction)
        {
           
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];
            Array.Copy(objBytes, fi.Header.PositionInRecord, b, 0, b.Length);
            
            return rawdataSerializer.ReadComplexArrayFirstTID(b, ti.Header.version, this,transaction);
        }
#if ASYNC
        internal async Task<int> ReadFirstTIDAsync(int oid, FieldSqoInfo fi, SqoTypeInfo ti, RawdataSerializer rawdataSerializer)
        {
            long position = MetaHelper.GetSeekPosition(ti, oid);
            int recordLength = ti.Header.lengthOfRecord;
            if (fi == null)
            {
                throw new SiaqodbException("Field not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            byte[] b = new byte[fi.Header.Length];

            await file.ReadAsync((long)(position + (long)fi.Header.PositionInRecord), b).ConfigureAwait(false);
            return await rawdataSerializer.ReadComplexArrayFirstTIDAsync(b, ti.Header.version, this).ConfigureAwait(false);
        }
#endif
        private byte[] GetFieldBytes(byte[] b, int fieldPosition, int fieldSize)
        {
            byte[] field = new byte[fieldSize];
            Array.Copy(b, fieldPosition, field, 0, fieldSize);
            return field;
        }
       

    }
}
