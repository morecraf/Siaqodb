using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
#if ASYNC
using System.Threading.Tasks;
#endif

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

        public byte[] GetBytes(object obj)
        {
            return this.serializer.GetComplexObjectBytes(obj);
        }

        public object GetObject(byte[] bytes)
        {
            return this.serializer.ReadComplexObject(bytes, ti.Type, fi.Name);
        }
#if ASYNC
        public async Task<byte[]> GetBytesAsync(object obj)
        {
            return await this.serializer.GetComplexObjectBytesAsync(obj).ConfigureAwait(false);
        }

        public async Task<object> GetObjectAsync(byte[] bytes)
        {
            return await this.serializer.ReadComplexObjectAsync(bytes, ti.Type, fi.Name).ConfigureAwait(false);
        }
#endif
        #endregion
    }
}
