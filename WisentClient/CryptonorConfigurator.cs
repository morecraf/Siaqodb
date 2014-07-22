using CryptonorClient.DocumentSerializer;
using CryptonorClient.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public class CryptonorConfigurator
    {
        internal static IEncryptor DefaultEncryptor=new AESEncryptor();
        internal static IDocumentSerializer DocumentSerializer=new CryptoJsonSerializer();
      
        public static void SetEncryptionKey(string encryptionKey)
        {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            AESEncryptor encAes = DefaultEncryptor as AESEncryptor;
            if (encAes != null)
            {
                byte[] aesKey = new byte[16];
                int length = key.Length;
                if (length > 16)
                    length = 16;
                Array.Copy(key, aesKey, length);
                encAes.SetKey(aesKey);
            }

        }
        public static void SetEncryptor(IEncryptor encryptor)
        {
            if (encryptor == null)
            {
                throw new ArgumentNullException("encryptor");
            }
            DefaultEncryptor = encryptor;
        }
        public static void SetDocumentSerializer(IDocumentSerializer documentSerializer)
        {
            if (documentSerializer == null)
            {
                throw new ArgumentNullException("documentSerializer");
            }
            DocumentSerializer = documentSerializer;
        }
       
    }
}
