using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dotissi.Meta;
using System.Collections;
using Dotissi.Utilities;
using Dotissi;
namespace Dotissi.Core
{
    class ByteTransformerFactory
    {
        public static IByteTransformer GetByteTransformer(ObjectSerializer serializer, RawdataSerializer rawSerializer, FieldSqoInfo fi, SqoTypeInfo ti, int parentOID)
        {
            if (fi.AttributeTypeId == Dotissi.Meta.MetaExtractor.complexID || fi.AttributeTypeId == Dotissi.Meta.MetaExtractor.documentID)
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
            else if (fi.AttributeTypeId == Dotissi.Meta.MetaExtractor.dictionaryID)
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
