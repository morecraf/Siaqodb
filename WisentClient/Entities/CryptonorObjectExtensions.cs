using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            byte[] serializedObj = CryptonorConfigurator.DocumentSerializer.Serialize(objValue);
            int nrBytes = PaddingSize(serializedObj.Length);
            byte[] encBytes = new byte[nrBytes];
            Array.Copy(serializedObj, 0, encBytes, 0, serializedObj.Length);
            CryptonorConfigurator.DefaultEncryptor.Encrypt(encBytes, 0, encBytes.Length);
            cryObj.Document = encBytes;
            cryObj.IsDirty = true;
        }
        public static T GetValue<T>(this CryptonorObject cryObj)
        {
            return (T)((object)cryObj.GetValue(typeof(T)));
        }
        public static object GetValue(this CryptonorObject cryObj, Type type)
        {
            byte[] encryptedDoc = new byte[cryObj.Document.Length];
            Array.Copy(cryObj.Document, 0, encryptedDoc, 0, encryptedDoc.Length);

            CryptonorConfigurator.DefaultEncryptor.Decrypt(encryptedDoc, 0, encryptedDoc.Length);
            return CryptonorConfigurator.DocumentSerializer.Deserialize(type, encryptedDoc);
        }
        private static int PaddingSize(int length)
        {
            int blockSize = CryptonorConfigurator.DefaultEncryptor.GetBlockSize() / 8;
            if (length % blockSize == 0) 
                return length;
            else
                return length + (blockSize - (length % blockSize));
        }
    }
}
