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
using LightningDB;


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
                if (fieldVal != null && ai.FInfo.FieldType.IsNullableEnum())
                {
                    Type enumType = Nullable.GetUnderlyingType(ai.FInfo.FieldType);
                    fieldVal = Enum.ToObject(enumType, fieldVal);

                }
                ai.FInfo.SetValue(obj, fieldVal);
#endif
            }




        }

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

        public void ReadObject<T>(T obj, byte[] objBytes, SqoTypeInfo ti, int oid, RawdataSerializer rawSerializer,LightningDB.LightningTransaction transaction)
        {
            this.ReadObject((object)obj, objBytes, ti, oid, rawSerializer,transaction);
        }

        public object ReadFieldValue(SqoTypeInfo ti, int oid,byte[] objBytes, string fieldName,LightningDB.LightningTransaction transaction)
        {
            return ReadFieldValue(ti, oid, objBytes, fieldName, null, transaction);
        }

        public object ReadFieldValue(SqoTypeInfo ti, int oid, byte[] objBytes, FieldSqoInfo fi, LightningDB.LightningTransaction transaction)
        {
            return this.ReadFieldValue(ti, oid, objBytes, fi, null, transaction);
        }

        public object ReadFieldValue(SqoTypeInfo ti, int oid,byte[] objBytes, string fieldName, RawdataSerializer rawSerializer,LightningDB.LightningTransaction transaction)
        {

            FieldSqoInfo fi = FindField(ti.Fields, fieldName);
            if (fi == null)
            {
                throw new SiaqodbException("Field:" + fieldName + " not exists in the Type Definition, if you use a Property you have to use UseVariable Attribute");
            }
            return this.ReadFieldValue(ti, oid,objBytes, fi, rawSerializer,transaction);

        }
       
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

        public KeyValuePair<int, int> ReadOIDAndTID(SqoTypeInfo ti, int oid,byte[] objBytes, FieldSqoInfo fi)
        {

            byte[] b = new byte[fi.Header.Length];
            Array.Copy(objBytes, fi.Header.PositionInRecord, b, 0, b.Length);

            return ReadOIDAndTID(b);

        }

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


        internal void ReadObjectRow(Sqo.Utilities.ObjectRow row, SqoTypeInfo ti,byte[] objBytes, RawdataSerializer rawSerializer)
        {
            //TODO LMDB
            //
           
            int fieldPosition = 0;
            byte[] oidBuff = GetFieldBytes(objBytes, fieldPosition, 4);
            int oidFromFile = ByteConverter.ByteArrayToInt(oidBuff);

            foreach (FieldSqoInfo ai in ti.Fields)
            {
                byte[] field = GetFieldBytes(objBytes, ai.Header.PositionInRecord, ai.Header.Length);
                if (typeof(IList).IsAssignableFrom(ai.AttributeType) || ai.IsText || ai.AttributeTypeId == MetaExtractor.complexID || ai.AttributeTypeId == MetaExtractor.dictionaryID || ai.AttributeTypeId == MetaExtractor.documentID)
                {
                    row[ai.Name] = field;
                }
                else
                {
                    try
                    {
                        if(ai.AttributeType != null){
                            row[ai.Name] = ByteConverter.DeserializeValueType(ai.AttributeType, field, true, ti.Header.version);
                        }else{
                            row[ai.Name] = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        SiaqodbConfigurator.LogMessage("Field's" + ai.Name + " value of Type " + ti.TypeName + "cannot be loaded,will be set to default.", VerboseLevel.Info);
                        row[ai.Name] = MetaHelper.GetDefault(ai.AttributeType);
                    }
                }

            }
           
        }

       
       
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

        private byte[] GetFieldBytes(byte[] b, int fieldPosition, int fieldSize)
        {
            byte[] field = new byte[fieldSize];
            Array.Copy(b, fieldPosition, field, 0, fieldSize);
            return field;
        }
    }
}
