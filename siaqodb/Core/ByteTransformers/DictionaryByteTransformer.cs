using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.MetaObjects;
using System.Collections;
using Sqo.Meta;

namespace Sqo.Core
{
    class DictionaryByteTransformer:IByteTransformer
    {
        private ObjectSerializer serializer;
        private RawdataSerializer rawSerializer;
        private Meta.SqoTypeInfo ti;
        private Meta.FieldSqoInfo fi;
        int parentOID;
        string dbName;
        public DictionaryByteTransformer(ObjectSerializer serializer, RawdataSerializer rawSerializer, Meta.SqoTypeInfo ti, Meta.FieldSqoInfo fi,int parentOID)
        {
            
            this.serializer = serializer;
            this.rawSerializer = rawSerializer;
            this.ti = ti;
            this.fi = fi;
            this.parentOID = parentOID;
            this.dbName =string.Format("raw.{0}" , ti.GetDBName());
        }
        #region IByteTransformer Members

        public byte[] GetBytes(object obj, LightningDB.LightningTransaction transaction)
        {
            DictionaryInfo dInfo = null;


            if (obj != null)
            {
                IDictionary actualDict = (IDictionary)obj;
                Type[] keyValueType = actualDict.GetType().GetGenericArguments();
                if (keyValueType.Length != 2)
                {
                    throw new Sqo.Exceptions.NotSupportedTypeException("Type:" + actualDict.GetType().ToString() + " is not supported");
                }
                int keyTypeId = MetaExtractor.GetAttributeType(keyValueType[0]);
                int valueTypeId = MetaExtractor.GetAttributeType(keyValueType[1]);
                dInfo = new DictionaryInfo();
                dInfo.KeyTypeId = keyTypeId;
                dInfo.ValueTypeId = valueTypeId;

            }
          
            return rawSerializer.SerializeDictionary(obj, fi.Header.Length, ti.Header.version, dInfo, serializer,dbName,fi.Name,parentOID, transaction);
        }

        public object GetObject(byte[] bytes, LightningDB.LightningTransaction transaction)
        {
            
            return rawSerializer.DeserializeDictionary(fi.AttributeType, bytes, ti.Header.version, serializer, ti.Type,dbName, fi.Name,parentOID, transaction);
        }

        #endregion


    }
}
