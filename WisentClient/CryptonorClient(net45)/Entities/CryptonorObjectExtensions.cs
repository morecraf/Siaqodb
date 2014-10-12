using Cryptonor;
using Cryptonor.Exceptions;
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
                throw new CryptonorException("Encryption algorithm should be set,use CryptonorConfigurator.SetEncryptor(...) method");
            if (objValue == null)
                throw new ArgumentNullException("objValue");

            if (cryObj.Key == null && CryptonorConfigurator.KeyConventions.ContainsKey(objValue.GetType()))
            {
                cryObj.Key = CryptonorConfigurator.KeyConventions[objValue.GetType()](objValue);
            }
            if (cryObj.Version == null && CryptonorConfigurator.VersionSetConventions.ContainsKey(objValue.GetType()))
            {
                cryObj.Version = CryptonorConfigurator.VersionSetConventions[objValue.GetType()](objValue);
            }
            byte[] serializedObj = CryptonorConfigurator.DocumentSerializer.Serialize(objValue);

            CryptonorConfigurator.Cipher.EnsureLength(ref serializedObj);
            byte[] encBytes= CryptonorConfigurator.Cipher.Encrypt(serializedObj);
            cryObj.Document = encBytes;
            cryObj.IsDirty = true;
        }
        public static T GetValue<T>(this CryptonorObject crObj)
        {
            return (T)((object)crObj.GetValue(typeof(T)));
        }
        public static object GetValue(this CryptonorObject crObj, Type type)
        {
            if (crObj.Document == null)
                return null;
            if (CryptonorConfigurator.Cipher == null)
                throw new CryptonorException("Encryption algorithm should be set,use CryptonorConfigurator.SetEncryptor(...) method");
          
            byte[] documentVal = new byte[crObj.Document.Length];
            Array.Copy(crObj.Document, documentVal, crObj.Document.Length);
            byte[] decDoc = CryptonorConfigurator.Cipher.Decrypt(documentVal);
            object obj= CryptonorConfigurator.DocumentSerializer.Deserialize(type, decDoc);
            if (CryptonorConfigurator.VersionGetConventions.ContainsKey(type))
            {
                CryptonorConfigurator.VersionGetConventions[type](obj,crObj.Version);
            }
            return obj;
        }
        public static string GetValueAsJson(this CryptonorObject crObj)
        {
            if (crObj.Document == null)
                return null;
            if (CryptonorConfigurator.Cipher == null)
                throw new CryptonorException("Encryption algorithm should be set,use CryptonorConfigurator.SetEncryptor(...) method");
          
            byte[] documentVal = new byte[crObj.Document.Length];
            Array.Copy(crObj.Document, documentVal, crObj.Document.Length);
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
                throw new CryptonorException("Encryption algorithm should be set,use CryptonorConfigurator.SetEncryptor(...) method");
          
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException("json");
            byte[] serializedObj = Encoding.UTF8.GetBytes(json);

            CryptonorConfigurator.Cipher.EnsureLength(ref serializedObj);
            byte[] encBytes = CryptonorConfigurator.Cipher.Encrypt(serializedObj);
            cryObj.Document = encBytes;
            cryObj.IsDirty = true;
        }
       
        
    }
}
