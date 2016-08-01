using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;

namespace Sqo.Core
{
    class PrimitiveByteTransformer:IByteTransformer
    {
        FieldSqoInfo fi;
        SqoTypeInfo ti;
        public PrimitiveByteTransformer(FieldSqoInfo fi,SqoTypeInfo ti)
        {
            this.fi=fi;
            this.ti = ti;
        }

        #region IByteTransformer Members

        public byte[] GetBytes(object obj, LightningDB.LightningTransaction transaction)
        {
            return ByteConverter.SerializeValueType(obj, fi.AttributeType, fi.Header.Length, fi.Header.RealLength, ti.Header.version);
        }

        public object GetObject(byte[] bytes, LightningDB.LightningTransaction transaction)
        {
            return ByteConverter.DeserializeValueType(fi.AttributeType, bytes, true, ti.Header.version);
        }

        

        #endregion
    }
}
