using Sqo.Core;
using Sqo.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonor
{
    class TagsSerializer
    {
        static int dbVersion = -35;
        public static byte[] GetBytes(Dictionary<string, object> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
                return null;
            List<SerElement> elements = new List<SerElement>();
            foreach (string elem in dictionary.Keys)
            {
                elements.Add(GetValueElem(elem));
                elements.Add(GetValueElem(dictionary[elem]));
            }
            int totalLen = elements.Sum(a => a.Size);
           
            byte[] array = new byte[totalLen];
            int index = 0;
            foreach (var elem in elements)
            {
                byte[] elemeArr=elem.ToArray();
                Array.Copy(elemeArr, 0, array, index, elemeArr.Length);
                index += elemeArr.Length;
              
            }
            return array;

        }
       
        public static Dictionary<string, object> GetDictionary(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

             var dict= new Dictionary<string, object>();
            int index = 0;
            int i = 1;
            while (index < bytes.Length - 1)
            {
                object key = DeserializeElement(ref index, bytes);
                object value = DeserializeElement(ref index, bytes);
                dict.Add((string)key, value);
            }
            return dict;
        }
        private static object DeserializeElement(ref int index, byte[] bytes)
        {
            byte[] TypeIdBytes = new byte[4];
            Array.Copy(bytes, index, TypeIdBytes, 0, 4);
            int TypeId = (int)ByteConverter.DeserializeValueType(typeof(int), TypeIdBytes, dbVersion);
            index += TypeIdBytes.Length;

            byte[] nrElemeBytes = new byte[4];
            Array.Copy(bytes, index, nrElemeBytes, 0, 4);
            int nrElem = (int)ByteConverter.DeserializeValueType(typeof(int), nrElemeBytes, dbVersion);
            index += nrElemeBytes.Length;

            byte[] content = new byte[nrElem];
            Array.Copy(bytes, index, content, 0, nrElem);
            object contentObj = ByteConverter.DeserializeValueType(Sqo.Cache.Cache.GetTypebyID(TypeId), content, dbVersion);
            index += content.Length;

            return contentObj;

        }
        private static SerElement GetValueElem(object elem)
        {
            SerElement value = new SerElement();
            value.TypeId = MetaExtractor.GetAttributeType(elem.GetType());
            value.Value = ByteConverter.SerializeValueType(elem, elem.GetType(), dbVersion);

            return value;
        }
        private class SerElement
        {
            public int TypeId;
            public byte[] Value;
            public int Size
            {
                get 
                {
                    return 4 + 4 + Value.Length;    
                }
            }
            public byte[] ToArray()
            {
                byte[] allbytes = new byte[this.Size];
                byte[] TypeIdBytes = ByteConverter.SerializeValueType(TypeId, typeof(int), dbVersion);
                byte[] nrElementsBytes = ByteConverter.SerializeValueType(Value.Length, typeof(int), dbVersion);
                int index = 0;
                Array.Copy(TypeIdBytes, 0, allbytes, index, TypeIdBytes.Length);
                index += TypeIdBytes.Length;
                Array.Copy(nrElementsBytes, 0, allbytes, index, nrElementsBytes.Length);
                index += nrElementsBytes.Length;
                Array.Copy(Value, 0, allbytes, index, Value.Length);
                return allbytes;
            }
        }
    }
}
