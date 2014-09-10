using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WinRT
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif
namespace CryptonorClient.Encryption
{
    class CryptonorRandom
    {
#if !WinRT
         private RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
#endif
        public void FillRandomBuffer(byte[] buffer)
        {
            #if !WinRT
            rngCsp.GetBytes(buffer);
#else
            IBuffer randomBuffer = CryptographicBuffer.GenerateRandom((uint)buffer.Length);
            CryptographicBuffer.CopyToByteArray(randomBuffer, out buffer);
#endif
        }
    }
}
