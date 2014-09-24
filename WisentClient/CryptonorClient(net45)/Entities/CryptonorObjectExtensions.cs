using Cryptonor;
using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace CryptonorClient
{
    public static class CryptonorObjectExtensions
    {
        public static void SetValue<T>(this CryptonorObject cryObj, T objValue)
        {
            SetValue(cryObj, (object)objValue);
        }
        public static void SetValue(this CryptonorObject cryObj, object objValue)
        {
            if (CryptonorConfigurator.Cipher == null)
                throw new Exception("Encryption algorithm should be set");
            byte[] serializedObj = CryptonorConfigurator.DocumentSerializer.Serialize(objValue);

            CryptonorConfigurator.Cipher.EnsureLength(ref serializedObj);
            byte[] encBytes= CryptonorConfigurator.Cipher.Encrypt(serializedObj);
            cryObj.Document = encBytes;
            cryObj.IsDirty = true;
        }
        public static T GetValue<T>(this CryptonorObject cryObj)
        {
            return (T)((object)cryObj.GetValue(typeof(T)));
        }
        public static object GetValue(this CryptonorObject cryObj, Type type)
        {
            byte[] documentVal = new byte[cryObj.Document.Length];
            Array.Copy(cryObj.Document, documentVal, cryObj.Document.Length);
            byte[] decDoc = CryptonorConfigurator.Cipher.Decrypt(documentVal);
            return CryptonorConfigurator.DocumentSerializer.Deserialize(type, decDoc);
        }
        public static string GetValueAsJson(this CryptonorObject cryObj)
        {
            byte[] documentVal = new byte[cryObj.Document.Length];
            Array.Copy(cryObj.Document, documentVal, cryObj.Document.Length);
            byte[] decDoc = CryptonorConfigurator.Cipher.Decrypt(documentVal);
#if SILVERLIGHT || CF || WinRT

            string jsonStr = Encoding.UTF8.GetString(decDoc, 0, decDoc.Length);

#else
            string jsonStr = Encoding.UTF8.GetString(decDoc);

#endif
            return jsonStr;
        }
        public static void SetValueAsJson(this CryptonorObject cryObj, string json)
        {
            if (CryptonorConfigurator.Cipher == null)
                throw new Exception("Encryption algorithm should be set");
            byte[] serializedObj = Encoding.UTF8.GetBytes(json);

            CryptonorConfigurator.Cipher.EnsureLength(ref serializedObj);
            byte[] encBytes = CryptonorConfigurator.Cipher.Encrypt(serializedObj);
            cryObj.Document = encBytes;
            cryObj.IsDirty = true;
        }
       
        
    }
}
