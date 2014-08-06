using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.Encryption
{
    public interface IEncryptor
    {
        void Encrypt(byte[] bytesIn,int inOff, byte[] byteOut);
        void Decrypt(byte[] bytesIn,int inOff, byte[] byteOut);
        int GetBlockSize();

    }
}
