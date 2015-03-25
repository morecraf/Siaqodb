using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using System.Collections;
using Sqo.Utilities;
using Sqo;
namespace Sqo.Core
{
    class ByteTransformerFactory
    {
        public static IByteTransformer GetByteTransformer(ObjectSerializer serializer, RawdataSerializer rawSerializer, FieldSqoInfo fi, SqoTypeInfo ti, int parentOID)
        {
            if (fi.AttributeTypeId == MetaExtractor.complexID || fi.AttributeTypeId == MetaExtractor.documentID)
            {
                return new ComplexTypeTransformer(serializer, ti, fi);
            }
            else if (typeof(IList).IsAssignableFrom(fi.AttributeType) || fi.IsText)//array
            {
                return new ArrayByteTranformer(serializer, rawSerializer, ti, fi, parentOID);
            }
            else if (fi.AttributeTypeId == MetaExtractor.dictionaryID)
            {
                return new DictionaryByteTransformer(serializer, rawSerializer, ti, fi,parentOID);
            }
            else
            {
                return new PrimitiveByteTransformer(fi, ti);
            }
        }
        
    }
}
