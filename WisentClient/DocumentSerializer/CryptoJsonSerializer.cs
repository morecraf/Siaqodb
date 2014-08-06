using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.DocumentSerializer
{
    public class CryptoJsonSerializer : IDocumentSerializer
    {
        #region IDocumentSerializer Members
        readonly JsonSerializer serializer = new JsonSerializer();
        public object Deserialize(Type type, byte[] objectBytes)
        {
            string jsonStr = Encoding.UTF8.GetString(objectBytes);
            return JsonConvert.DeserializeObject(jsonStr.TrimEnd('\0'), type);
        }

        public byte[] Serialize(object obj)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();

            string jsonStr = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(jsonStr);
        }

        #endregion
    }
}
