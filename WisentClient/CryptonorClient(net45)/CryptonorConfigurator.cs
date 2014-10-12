using CryptonorClient.DocumentSerializer;
using CryptonorClient.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CryptonorClient
{
    public class CryptonorConfigurator
    {
        internal static CBCCipher Cipher;
        internal static IDocumentSerializer DocumentSerializer=new CryptoJsonSerializer();
        internal static Dictionary<Type, Func<object, string>> KeyConventions = new Dictionary<Type, Func<object, string>>();
        internal static Dictionary<Type, Func<object, string>> VersionSetConventions = new Dictionary<Type, Func<object, string>>();
        internal static Dictionary<Type, Action<object,string>> VersionGetConventions = new Dictionary<Type, Action<object,string>>();
        
        //internal static Dictionary<Type, Func<object, Dictionary<string, object>>> TagsConventions = new Dictionary<Type, Func<object, Dictionary<string, object>>>();
       
        public static void SetEncryptor(EncryptionAlgorithm algorithm, string encryptionKey)
        {
            if (algorithm == EncryptionAlgorithm.AES128)
            {
                AES128Encryptor encryptor = new AES128Encryptor();
                Cipher = new CBCCipher(encryptor);
                encryptor.SetKey(BuildKey(encryptionKey, 16));

            }
            if (algorithm == EncryptionAlgorithm.AES256)
            {
                AES256Encryptor encryptor = new AES256Encryptor();
                Cipher = new CBCCipher(encryptor);
                encryptor.SetKey(BuildKey(encryptionKey, 32));

            }
            if (algorithm == EncryptionAlgorithm.Camellia128)
            {
                CamelliaEngine encryptor = new CamelliaEngine();
                Cipher = new CBCCipher(encryptor);
                encryptor.SetKey(BuildKey(encryptionKey, 16));

            }
            if (algorithm == EncryptionAlgorithm.Camellia256)
            {
                CamelliaEngine encryptor = new CamelliaEngine();
                Cipher = new CBCCipher(encryptor);
              
                encryptor.SetKey(BuildKey(encryptionKey,32));

            }

        }
        private static byte[] BuildKey(string encryptionKey, int keyLength)
        {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);

            byte[] aesKey = new byte[keyLength];
            int length = key.Length;
            if (length > keyLength)
                length = keyLength;
            Array.Copy(key, aesKey, length);
            return aesKey;
        }
        public static void SetEncryptor(IEncryptor encryptor)
        {
            if (encryptor == null)
            {
                throw new ArgumentNullException("encryptor");
            }
            Cipher = new CBCCipher(encryptor);
        }
        public static void RegisterKeyConvention<T>(Func<T, string> func)
        {
            KeyConventions[typeof(T)] = a => func((T)a);
        }
        public static void RegisterSetVersionConvention<T>(Func<T, string> func)
        {
            VersionSetConventions[typeof(T)] = a => func((T)a);
        }

        public static void RegisterGetVersionConvention<T>(Action<T,string> action)
        {
            VersionGetConventions[typeof(T)] = (a,b) => action((T)a,b);
        }
        
    }
    public enum EncryptionAlgorithm { AES128, AES256, Camellia128,Camellia256}
}
