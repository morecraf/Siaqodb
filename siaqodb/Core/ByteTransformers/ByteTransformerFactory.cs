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
            if (fi.AttributeTypeId == MetaExtractor.complexID)
            {
                return new ComplexTypeTransformer(serializer, ti, fi);
            }
            else if (typeof(IList).IsAssignableFrom(fi.AttributeType) || fi.IsText)//array
            {
                if (ti.Type != null) //is null when loaded by SiaqodbManager
                {

                    if (ti.Type.IsGenericType())
                    {
                        if (ti.Type.GetGenericTypeDefinition() == typeof(Indexes.BTreeNode<>) && (fi.Name == "Keys" || fi.Name == "_childrenOIDs"))
                        {
                            return new FixedArrayByteTransformer(serializer, ti, fi);
                        }
                        else
                        {
                            return new ArrayByteTranformer(serializer, rawSerializer, ti, fi, parentOID);
                        }
                    }
                    else
                    {
                        return new ArrayByteTranformer(serializer, rawSerializer, ti, fi, parentOID);
                    }
                }
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
        public static IByteTransformer GetByteTransformer(ObjectSerializer serializer, RawdataSerializer rawSerializer, FieldSqoInfo fi, SqoTypeInfo ti)
        {
            return GetByteTransformer(serializer, rawSerializer, fi, ti, -1);
        }
    }
}
