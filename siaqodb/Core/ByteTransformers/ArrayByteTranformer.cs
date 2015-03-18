using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using Sqo.Utilities;

#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Core
{
    class ArrayByteTranformer:IByteTransformer
    {
        FieldSqoInfo fi;
        SqoTypeInfo ti;
        ObjectSerializer serializer;
        RawdataSerializer rawSerializer;
         int parentOID;
        public ArrayByteTranformer(ObjectSerializer serializer, RawdataSerializer rawSerializer, SqoTypeInfo ti, FieldSqoInfo fi, int parentOID)
        {
            this.serializer = serializer;
            this.ti = ti;
            this.fi = fi;
            this.rawSerializer = rawSerializer;
            this.parentOID = parentOID;
        }
        #region IByteTransformer Members

        public byte[] GetBytes(object obj,LightningDB.LightningTransaction transaction)
        {

            ATuple<int, int> arrayMeta = null;
            if (parentOID > 0)//means already exists the rawOID
            {
                arrayMeta = serializer.GetArrayMetaOfField(ti, parentOID, fi);
            }
            if (fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.textID))
            {
                return rawSerializer.SerializeArray(obj, fi.AttributeType, fi.Header.Length, fi.Header.RealLength, ti.Header.version, arrayMeta, this.serializer, true,transaction);
            }
            else
            {
                return rawSerializer.SerializeArray(obj, fi.AttributeType, fi.Header.Length, fi.Header.RealLength, ti.Header.version, arrayMeta, this.serializer, false,transaction);
       
            }
            
        }

        public object GetObject(byte[] bytes, LightningDB.LightningTransaction transaction)
        {
            object fieldVal = null;
            if (fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.complexID) || fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.jaggedArrayID))// array of complexType
            {
                fieldVal = rawSerializer.DeserializeArray(fi.AttributeType, bytes, true, ti.Header.version, fi.IsText,false, this.serializer, ti.Type, fi.Name,transaction);
            }
            else if (fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.textID))
            {
                fieldVal = rawSerializer.DeserializeArray(fi.AttributeType, bytes, true, ti.Header.version, false,true,transaction);
            }
            else
            {

                fieldVal = rawSerializer.DeserializeArray(fi.AttributeType, bytes, true, ti.Header.version, fi.IsText, false, transaction);
            }
            return fieldVal;
        }


#if ASYNC
        public async Task<byte[]> GetBytesAsync(object obj)
        {
            ATuple<int, int> arrayMeta = null;
            if (parentOID > 0)//means already exists the rawOID
            {
                arrayMeta = await serializer.GetArrayMetaOfFieldAsync(ti, parentOID, fi).ConfigureAwait(false);
            }
            if (fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.textID))
            {
                return await rawSerializer.SerializeArrayAsync(obj, fi.AttributeType, fi.Header.Length, fi.Header.RealLength, ti.Header.version, arrayMeta, this.serializer, true).ConfigureAwait(false);
            }
            else
            {
                return await rawSerializer.SerializeArrayAsync(obj, fi.AttributeType, fi.Header.Length, fi.Header.RealLength, ti.Header.version, arrayMeta, this.serializer, false).ConfigureAwait(false);
            }
        }

        public async Task<object>  GetObjectAsync(byte[] bytes)
        {
            object fieldVal = null;
            if (fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.complexID) || fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.jaggedArrayID))// array of complexType
            {
                fieldVal = await rawSerializer.DeserializeArrayAsync(fi.AttributeType, bytes, true, ti.Header.version, fi.IsText,false, this.serializer, ti.Type, fi.Name).ConfigureAwait(false);
            }
            else if (fi.AttributeTypeId == (MetaExtractor.ArrayTypeIDExtra + MetaExtractor.textID))
            {
                fieldVal = await rawSerializer.DeserializeArrayAsync(fi.AttributeType, bytes, true, ti.Header.version, false, true).ConfigureAwait(false);
            }
            else
            {

                fieldVal = await rawSerializer.DeserializeArrayAsync(fi.AttributeType, bytes, true, ti.Header.version, fi.IsText, false).ConfigureAwait(false);
            }
            return fieldVal;
        }
        #endif

        #endregion

    }
}
