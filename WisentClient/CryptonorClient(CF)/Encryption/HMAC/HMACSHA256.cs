using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;

namespace CryptonorClient.Encryption
{
    class HMACSHA256:IDisposable
    {
        byte[] key;
        public HMACSHA256(byte[] key)
        {
            this.key = key;
        }
        public byte[] ComputeHash(byte[] data)
        {

            KeyParameter paramKey = new KeyParameter(key);
            IMac mac = new HMac(new Sha256Digest());
            mac.Init(paramKey);
            mac.Reset();
            mac.BlockUpdate(data, 0, data.Length);
            byte[] bHash = new byte[mac.GetMacSize()];
            mac.DoFinal(bHash, 0);
            return bHash;
        }

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }
}
