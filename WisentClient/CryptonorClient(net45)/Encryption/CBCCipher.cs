using CryptonorClient.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.Encryption
{
    public class CBCCipher
    {
        IEncryptor encryptor;
        readonly int BLOCK_SIZE;
        CryptonorRandom random = new CryptonorRandom();
        public CBCCipher(IEncryptor encryptor)
        {
            this.encryptor = encryptor;
            BLOCK_SIZE = encryptor.GetBlockSize() / 8;
        }
        public void EnsureLength(ref byte[] bytesIn)
        {
            int nrBytes = PaddingSize(bytesIn.Length);
            if (nrBytes != bytesIn.Length)
            {
                Array.Resize<byte>(ref bytesIn, nrBytes);
            }

        }
        public byte[] Encrypt(byte[] bytesIn)
        {
            if (bytesIn.Length % BLOCK_SIZE!=0)
                throw new ArgumentException("bytesIn size invalid");
            byte[] IV = new byte[BLOCK_SIZE];
            random.FillRandomBuffer(IV);

            byte[] bytesOut = new byte[bytesIn.Length + BLOCK_SIZE];//larger to keep IV
            Array.Copy(IV, 0, bytesOut, 0, IV.Length);//first block is IV
            byte[] cbc = new byte[BLOCK_SIZE];
            Array.Copy(IV, cbc, BLOCK_SIZE);

            for (int i = 0; i < bytesIn.Length; i += BLOCK_SIZE)
            {
                for (int j = i % BLOCK_SIZE; j < BLOCK_SIZE; j++)
                {
                    bytesIn[i+j] ^= cbc[j];
                }
                this.encryptor.Encrypt(bytesIn,i, bytesIn);
                Array.Copy(bytesIn, i, cbc, 0, BLOCK_SIZE);
            }
            Array.Copy(bytesIn, 0, bytesOut, BLOCK_SIZE, bytesIn.Length);
            return bytesOut;
        }
        public byte[] Decrypt(byte[] bytesIn)
        {
            if (bytesIn.Length % BLOCK_SIZE != 0)
                throw new ArgumentException("bytesIn size invalid");

            byte[] cbc = new byte[BLOCK_SIZE];
            byte[] cbcNext = new byte[BLOCK_SIZE];
            byte[] bytesOut = new byte[bytesIn.Length - BLOCK_SIZE];//remove IV
            Array.Copy(bytesIn, 0, cbc, 0, cbc.Length);//first block is IV
            for (int i = BLOCK_SIZE; i < bytesIn.Length; i += BLOCK_SIZE)
            {
                Array.Copy(bytesIn, i, cbcNext, 0, BLOCK_SIZE);

                this.encryptor.Decrypt(bytesIn,i, bytesIn);
                for (int j = i % BLOCK_SIZE; j < BLOCK_SIZE; j++)
                {
                    bytesIn[i + j] ^= cbc[j];
                }
                Array.Copy(cbcNext, cbc, BLOCK_SIZE);
                
            }
            Array.Copy(bytesIn, BLOCK_SIZE, bytesOut, 0, bytesOut.Length);
            return bytesOut;
        }
        private int PaddingSize(int length)
        {
            if (length % BLOCK_SIZE == 0)
                return length;
            else
                return length + (BLOCK_SIZE - (length % BLOCK_SIZE));
        }
       
    }
}
