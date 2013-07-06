using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace SiaqodbSyncProvider.Utilities
{
    class Decryptor
    {
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

    }
}
