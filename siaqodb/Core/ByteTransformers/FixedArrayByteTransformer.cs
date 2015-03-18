using Sqo.Meta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Indexes;

namespace Sqo.Core
{
    class FixedArrayByteTransformer:IByteTransformer
    {
        FieldSqoInfo fi;
        SqoTypeInfo ti;
        ObjectSerializer serializer;
        public FixedArrayByteTransformer(ObjectSerializer serializer,SqoTypeInfo ti, FieldSqoInfo fi)
        {
            this.serializer = serializer;
            this.ti = ti;
            this.fi = fi;
        }
        public byte[] GetBytes(object obj, LightningDB.LightningTransaction transaction)
        {
            Type elementType = fi.AttributeType.GetElementType();
            int elementTypeId = MetaExtractor.GetAttributeType(elementType);
            int elementSize = MetaExtractor.GetSizeOfField(elementTypeId);
            int rawLength = ((IList)obj).Count * elementSize;
            byte[] rawArray = new byte[rawLength];
            //build array for elements
            int currentIndex = 0;
            foreach (object elem in ((IList)obj))
            {
                byte[] elemArray = null;
                if (elementTypeId == MetaExtractor.complexID)
                {
                    elemArray = serializer.GetComplexObjectBytes(elem,true,transaction);
                }
                else
                {
                    object elemObj = elem;
                    if (elem == null)
                    {
                        if (elementType == typeof(string))
                        {
                            elemObj = string.Empty;
                        }
                    }
                     elemArray = ByteConverter.SerializeValueType(elemObj, elemObj.GetType(), elementSize, MetaExtractor.GetAbsoluteSizeOfField(elementTypeId), ti.Header.version);
                }
                Array.Copy(elemArray, 0, rawArray, currentIndex, elemArray.Length);

                currentIndex += elemArray.Length;
            }
            return rawArray;
        }

        public object GetObject(byte[] arrayData, LightningDB.LightningTransaction transaction)
        {
            bool isArray = fi.AttributeType.IsArray;
            Type elementType = fi.AttributeType.GetElementType();

            int elementTypeId = MetaExtractor.GetAttributeType(elementType);
            int elementSize = MetaExtractor.GetSizeOfField(elementTypeId);
            int nrElem = 0;
            if (fi.Name == "Keys" || fi.Name == "Values")
            {
                nrElem=BTreeNode<int>.KEYS_PER_NODE;
            }
            else//_childrenOIDs
            {
               nrElem= BTreeNode<int>.CHILDREN_PER_NODE;
            }

            Array ar = null;
            
            if (isArray)
            {
                ar = Array.CreateInstance(elementType, nrElem);
            }
          
            int currentIndex = 0;
            byte[] elemBytes = new byte[elementSize];
            for (int i = 0; i < nrElem; i++)
            {
                Array.Copy(arrayData, currentIndex, elemBytes, 0, elementSize);
                currentIndex += elementSize;
                object obj = null;
                if (elementTypeId == MetaExtractor.complexID)
                {
                    obj = serializer.ReadComplexObject(elemBytes, ti.Type, fi.Name,transaction);
                }
                else
                {
                    obj = ByteConverter.DeserializeValueType(elementType, elemBytes, true, ti.Header.version);
                }
                if (isArray)
                {
                    ar.SetValue(obj, i);
                }
               


            }

            return ar;



        }

#if ASYNC
        public async System.Threading.Tasks.Task<byte[]> GetBytesAsync(object obj)
        {
            Type elementType = fi.AttributeType.GetElementType();
            int elementTypeId = MetaExtractor.GetAttributeType(elementType);
            int elementSize = MetaExtractor.GetSizeOfField(elementTypeId);
            int rawLength = ((IList)obj).Count * elementSize;
            byte[] rawArray = new byte[rawLength];
            //build array for elements
            int currentIndex = 0;
            foreach (object elem in ((IList)obj))
            {
                byte[] elemArray = null;
                if (elementTypeId == MetaExtractor.complexID)
                {
                    elemArray = await serializer.GetComplexObjectBytesAsync(elem, true).ConfigureAwait(false);
                }
                else
                {
                    object elemObj = elem;
                    if (elem == null)
                    {
                        if (elementType == typeof(string))
                        {
                            elemObj = string.Empty;
                        }
                    }
                    elemArray = ByteConverter.SerializeValueType(elemObj, elemObj.GetType(), elementSize, MetaExtractor.GetAbsoluteSizeOfField(elementTypeId), ti.Header.version);
                }
                Array.Copy(elemArray, 0, rawArray, currentIndex, elemArray.Length);

                currentIndex += elemArray.Length;
            }
            return rawArray;
        }

        public async System.Threading.Tasks.Task<object> GetObjectAsync(byte[] arrayData)
        {
            bool isArray = fi.AttributeType.IsArray;
            Type elementType = fi.AttributeType.GetElementType();

            int elementTypeId = MetaExtractor.GetAttributeType(elementType);
            int elementSize = MetaExtractor.GetSizeOfField(elementTypeId);
            int nrElem = 0;
            if (fi.Name == "Keys" || fi.Name == "Values")
            {
                nrElem = BTreeNode<int>.KEYS_PER_NODE;
            }
            else//_childrenOIDs
            {
                nrElem = BTreeNode<int>.CHILDREN_PER_NODE;
            }

            Array ar = null;

            if (isArray)
            {
                ar = Array.CreateInstance(elementType, nrElem);
            }

            int currentIndex = 0;
            byte[] elemBytes = new byte[elementSize];
            for (int i = 0; i < nrElem; i++)
            {
                Array.Copy(arrayData, currentIndex, elemBytes, 0, elementSize);
                currentIndex += elementSize;
                object obj = null;
                if (elementTypeId == MetaExtractor.complexID)
                {
                    obj = await serializer.ReadComplexObjectAsync(elemBytes, ti.Type, fi.Name).ConfigureAwait(false);
                }
                else
                {
                    obj = ByteConverter.DeserializeValueType(elementType, elemBytes, true, ti.Header.version);
                }
                if (isArray)
                {
                    ar.SetValue(obj, i);
                }



            }

            return ar;

        }
#endif
    }
}
