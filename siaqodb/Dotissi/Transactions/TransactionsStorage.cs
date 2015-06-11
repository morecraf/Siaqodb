using System;
using Dotissi.Core;
using Dotissi.Meta;
#if ASYNC_LMDB
using System.Threading.Tasks;
#endif
namespace Dotissi.Transactions
{
    internal class TransactionsStorage
    {
        ISqoFile file;
        public TransactionsStorage(string filePath,bool useElevatedTrust)
        {
            file = FileFactory.Create(filePath, false, useElevatedTrust);
        }
        
        public int SaveTransactionalObject(byte[] objBytes, long pos)
        {

            file.Write(pos, objBytes);
            return objBytes.Length;
            
        }
#if ASYNC_LMDB
        public async Task<int> SaveTransactionalObjectAsync(byte[] objBytes, long pos)
        {

            await file.WriteAsync(pos, objBytes).ConfigureAwait(false);
            return objBytes.Length;

        }
#endif
        public void Write(long pos, byte[] buffer)
        {
            file.Write(pos, buffer);
        }
#if ASYNC_LMDB
        public async Task WriteAsync(long pos, byte[] buffer)
        {
            await file.WriteAsync(pos, buffer).ConfigureAwait(false);
        }
#endif
        public void Read(long pos, byte[] buffer)
        {
            file.Read(pos, buffer);
        }
#if ASYNC_LMDB
        public async Task ReadAsync(long pos, byte[] buffer)
        {
            await file.ReadAsync(pos, buffer).ConfigureAwait(false);
        }
#endif
        public void Flush()
        {
            file.Flush();
        }
#if ASYNC_LMDB
        public async Task FlushAsync()
        {
            await file.FlushAsync().ConfigureAwait(false);
        }
#endif
        public void Close()
        {
            file.Flush();
            file.Close();
        }
#if ASYNC_LMDB
        public async Task CloseAsync()
        {
            await file.FlushAsync().ConfigureAwait(false);
            file.Close();
        }
#endif
        
    }
}
