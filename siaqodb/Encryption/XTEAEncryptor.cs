using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Encryption
{
    class XTEAEncryptor : IEncryptor
    {
        private static readonly int DELTA = unchecked((int)(0x9E3779B9));
        private int k0, k1, k2, k3, k4, k5, k6, k7, k8, k9, k10, k11, k12, k13, k14, k15;
        private int k16, k17, k18, k19, k20, k21, k22, k23, k24, k25, k26, k27, k28, k29, k30, k31;
        public XTEAEncryptor()
        {
            byte[] kap = new byte[32];
            kap[0] = 54; kap[1] = 34; kap[2] = 12; kap[3] = 67; kap[4] = 87; kap[5] = 33; kap[6] = 89; kap[7] = 43; kap[8] = 45; kap[9] = 111;
            kap[10] = 54; kap[11] = 34; kap[12] = 12; kap[13] = 67; kap[14] = 87; kap[15] = 33; kap[16] = 89; kap[17] = 43; kap[18] = 45; kap[19] = 111;
            kap[20] = 54; kap[21] = 34; kap[22] = 12; kap[23] = 67; kap[24] = 87; kap[25] = 33; kap[26] = 89; kap[27] = 43; kap[28] = 45; kap[29] = 111;
            kap[30] = 22; kap[31] = 66;
            SetKey(kap);
        }
        public void SetKey(byte[] b)
        {
            int[] key = new int[4];
            for (int i = 0; i < 16; )
            {
                key[i / 4] = (b[i++] << 24) + ((b[i++] & 255) << 16) + ((b[i++] & 255) << 8) + (b[i++] & 255);
            }
            int[] r = new int[32];
            for (int i = 0, sum = 0; i < 32; )
            {
                r[i++] = sum + key[sum & 3];
                sum += DELTA;
                r[i++] = sum + key[((int)((uint)sum >> 11)) & 3];
            }
            k0 = r[0]; k1 = r[1]; k2 = r[2]; k3 = r[3]; k4 = r[4]; k5 = r[5]; k6 = r[6]; k7 = r[7];
            k8 = r[8]; k9 = r[9]; k10 = r[10]; k11 = r[11]; k12 = r[12]; k13 = r[13]; k14 = r[14]; k15 = r[15];
            k16 = r[16]; k17 = r[17]; k18 = r[18]; k19 = r[19]; k20 = r[20]; k21 = r[21]; k22 = r[22]; k23 = r[23];
            k24 = r[24]; k25 = r[25]; k26 = r[26]; k27 = r[27]; k28 = r[28]; k29 = r[29]; k30 = r[30]; k31 = r[31];
        }
        public void Encrypt(byte[] bytes, int off, int len)
        {

            for (int i = off; i < off + len; i += 8)
            {
                encryptBlock(bytes, bytes, i);
            }
        }

        public void Decrypt(byte[] bytes, int off, int len)
        {

            for (int i = off; i < off + len; i += 8)
            {
                decryptBlock(bytes, bytes, i);
            }
        }

        public void encryptBlock(byte[] inb, byte[] outb, int off)
        {
            int y = (inb[off] << 24) | ((inb[off + 1] & 255) << 16) | ((inb[off + 2] & 255) << 8) | (inb[off + 3] & 255);
            int z = (inb[off + 4] << 24) | ((inb[off + 5] & 255) << 16) | ((inb[off + 6] & 255) << 8) | (inb[off + 7] & 255);
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k0;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k1;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k2;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k3;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k4;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k5;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k6;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k7;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k8;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k9;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k10;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k11;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k12;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k13;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k14;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k15;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k16;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k17;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k18;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k19;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k20;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k21;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k22;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k23;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k24;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k25;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k26;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k27;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k28;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k29;
            y += (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k30;
            z += ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k31;
            outb[off] = (byte)(y >> 24); outb[off + 1] = (byte)(y >> 16); outb[off + 2] = (byte)(y >> 8); outb[off + 3] = (byte)y;
            outb[off + 4] = (byte)(z >> 24); outb[off + 5] = (byte)(z >> 16); outb[off + 6] = (byte)(z >> 8); outb[off + 7] = (byte)z;
        }

        public void decryptBlock(byte[] inb, byte[] outb, int off)
        {
            int y = (inb[off] << 24) | ((inb[off + 1] & 255) << 16) | ((inb[off + 2] & 255) << 8) | (inb[off + 3] & 255);
            int z = (inb[off + 4] << 24) | ((inb[off + 5] & 255) << 16) | ((inb[off + 6] & 255) << 8) | (inb[off + 7] & 255);
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k31;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k30;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k29;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k28;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k27;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k26;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k25;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k24;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k23;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k22;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k21;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k20;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k19;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k18;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k17;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k16;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k15;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k14;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k13;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k12;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k11;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k10;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k9;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k8;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k7;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k6;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k5;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k4;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k3;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k2;
            z -= ((((int)((uint)y >> 5)) ^ (y << 4)) + y) ^ k1;
            y -= (((z << 4) ^ ((int)((uint)z >> 5))) + z) ^ k0;
            outb[off] = (byte)(y >> 24); outb[off + 1] = (byte)(y >> 16); outb[off + 2] = (byte)(y >> 8); outb[off + 3] = (byte)y;
            outb[off + 4] = (byte)(z >> 24); outb[off + 5] = (byte)(z >> 16); outb[off + 6] = (byte)(z >> 8); outb[off + 7] = (byte)z;
        }




       

        public int GetBlockSize()
        {
            return 64;
        }

       
    }
}
