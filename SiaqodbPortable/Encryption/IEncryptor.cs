using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Encryption
{
    public interface IEncryptor
    {
        void Encrypt(byte[] bytes, int off, int len);
        void Decrypt(byte[] bytes, int off, int len);
        int GetBlockSize();
        
    }
}
