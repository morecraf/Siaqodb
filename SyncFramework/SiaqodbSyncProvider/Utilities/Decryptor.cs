using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !WinRT
using System.Security.Cryptography;
#else
using Windows.Storage.Streams;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
#endif
using System.IO;


namespace SiaqodbSyncProvider.Utilities
{
    class Decryptor
    {
#if WinRT
        public static string DecryptRJ128(string prm_key, string prm_iv, string prm_text_to_decrypt)
        {
            IBuffer encrypted;
            IBuffer buffer;
            IBuffer iv = null;
            byte[] keyBuff = System.Text.Encoding.UTF8.GetBytes(prm_key);
            byte[] IVBuff = System.Text.Encoding.UTF8.GetBytes(prm_iv);
            
            SymmetricKeyAlgorithmProvider algorithm = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7"); //This is the only one using two fixed keys and variable block size

            IBuffer keymaterial = CryptographicBuffer.CreateFromByteArray(keyBuff); // as said..I have fixed keys (see above)
            CryptographicKey key = algorithm.CreateSymmetricKey(keymaterial);
            
            byte[] sEncrypted = Convert.FromBase64String(prm_text_to_decrypt);
            
            iv = CryptographicBuffer.CreateFromByteArray(IVBuff); // again my IV is fixed
            buffer = CryptographicBuffer.CreateFromByteArray(sEncrypted);  //Directly converting GUID to byte array
            encrypted = Windows.Security.Cryptography.Core.CryptographicEngine.Decrypt(key, buffer, iv);

            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8,encrypted);
        }
#else
        public static string DecryptRJ128(string prm_key, string prm_iv, string prm_text_to_decrypt)
        {
            string sEncryptedString = prm_text_to_decrypt;
#if CF
            RijndaelManaged myRijndael = new RijndaelManaged();
#else
            AesManaged myRijndael = new AesManaged();
#endif
            myRijndael.KeySize = 128;
            myRijndael.BlockSize = 128;
            byte[] key = System.Text.Encoding.UTF8.GetBytes(prm_key);
            byte[] IV = System.Text.Encoding.UTF8.GetBytes(prm_iv);
            ICryptoTransform decryptor = myRijndael.CreateDecryptor(key, IV);
            byte[] sEncrypted = Convert.FromBase64String(sEncryptedString);
            byte[] fromEncrypt = new byte[sEncrypted.Length];
            MemoryStream msDecrypt = new MemoryStream(sEncrypted);
            CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

            return System.Text.Encoding.UTF8.GetString(fromEncrypt, 0, fromEncrypt.Length).TrimEnd('\0');
        }
        //http://stackoverflow.com/questions/224453/decrypt-php-encrypted-string-in-c

#endif
    }

}
