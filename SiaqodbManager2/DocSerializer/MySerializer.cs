using Newtonsoft.Json;
using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.DocSerializer
{
    public class MyJsonSerializer : IDocumentSerializer
    {
        #region IDocumentSerializer Members

#if !UNITY3D && !CF

        readonly JsonSerializer serializer = new JsonSerializer();
#endif
        public object Deserialize(Type type, byte[] objectBytes)
        {
#if SILVERLIGHT || CF || WinRT

            string jsonStr = Encoding.UTF8.GetString(objectBytes, 0, objectBytes.Length);

#else
            string jsonStr = Encoding.UTF8.GetString(objectBytes);

#endif
#if !UNITY3D && !CF
            return JsonConvert.DeserializeObject(jsonStr.TrimEnd('\0'), type);
#else
            LitJson.JsonReader reader = new LitJson.JsonReader(jsonStr.TrimEnd('\0'));

            return LitJson.JsonMapper.ReadValue(type, reader);
#endif

        }

        public byte[] Serialize(object obj)
        {
#if !UNITY3D && !CF
            string jsonStr = JsonConvert.SerializeObject(obj, Formatting.Indented);

#else
            string jsonStr = LitJson.JsonMapper.ToJson(obj);

#endif
            return Encoding.UTF8.GetBytes(jsonStr);
        }

        #endregion
    }
}
