

namespace CryptonorClient.Encryption
{
    public interface IEncryptor
    {
        void Encrypt(byte[] bytesIn,int inOff, byte[] byteOut);
        void Decrypt(byte[] bytesIn,int inOff, byte[] byteOut);
        int GetBlockSize();

    }
}
