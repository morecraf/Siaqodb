using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.Encryption
{
    public interface IEncryptor
    {
        void Encrypt(byte[] bytes, int off, int len);
        void Decrypt(byte[] bytes, int off, int len);
        int GetBlockSize();

    }
}
