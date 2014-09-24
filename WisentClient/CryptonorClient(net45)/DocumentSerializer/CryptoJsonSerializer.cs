using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CryptonorClient.DocumentSerializer
{
    public class CryptoJsonSerializer : IDocumentSerializer
    {
        #region IDocumentSerializer Members
        readonly JsonSerializer serializer = new JsonSerializer();
        public object Deserialize(Type type, byte[] objectBytes)
        {
#if SILVERLIGHT || CF || WinRT

            string jsonStr = Encoding.UTF8.GetString(objectBytes, 0, objectBytes.Length);

#else
            string jsonStr = Encoding.UTF8.GetString(objectBytes);

#endif

            return JsonConvert.DeserializeObject(jsonStr.TrimEnd('\0'), type);
        }

        public byte[] Serialize(object obj)
        {
           
            string jsonStr = JsonConvert.SerializeObject(obj,Formatting.Indented);
            return Encoding.UTF8.GetBytes(jsonStr);
        }

        #endregion
    }
}
