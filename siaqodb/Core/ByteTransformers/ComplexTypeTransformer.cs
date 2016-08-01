using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;


namespace Sqo.Core
{
    class ComplexTypeTransformer:IByteTransformer
    {
        ObjectSerializer serializer;
        SqoTypeInfo ti;
        FieldSqoInfo fi;
        public ComplexTypeTransformer(ObjectSerializer serializer,SqoTypeInfo ti,FieldSqoInfo fi)
        {
            this.serializer = serializer;
            this.fi = fi;
            this.ti = ti;
        }


        #region IByteTransformer Members

        public byte[] GetBytes(object obj,LightningDB.LightningTransaction transaction)
        {
            return this.serializer.GetComplexObjectBytes(obj, transaction);
        }

        public object GetObject(byte[] bytes, LightningDB.LightningTransaction transaction)
        {
            return this.serializer.ReadComplexObject(bytes, ti.Type, fi.Name, transaction);
        }

        #endregion
    }
}
