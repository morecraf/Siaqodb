#if SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.IO.IsolatedStorage;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Core
{
    internal class SqoISFile2 : ISqoFile
    {
        protected IsolatedStorageFileStream file;
        
        private IsolatedStorageFile isf;
        private string filePath;


        public virtual void Write(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            file.Write(buf, 0, buf.Length);
        }
        public virtual void Write(byte[] buf)
        {
            file.Write(buf, 0, buf.Length);
        }
        public virtual int Read(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            return file.Read(buf, 0, buf.Length);
        }
        public bool IsClosed
        {
            get { return this.isClosed; }
        }
        public virtual void Flush()
        {
            if (!this.IsClosed)
            {
                file.Flush();
                
            }

        }
#if ASYNC
        public async Task WriteAsync(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            await file.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }
        public async Task WriteAsync(byte[] buf)
        {
            await file.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }
        public async Task<int> ReadAsync(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            return await file.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }
        public async Task FlushAsync()
        {
            if (!this.IsClosed)
            {
                await file.FlushAsync().ConfigureAwait(false);

            }

        }
#endif

        bool isClosed = false;
        public virtual void Close()
        {
           isClosed = true;
            file.Close();
            

           
        }
        public long Length
        {
            get { return file.Length; }
            set { file.SetLength(value); }
        }


        internal SqoISFile2(String filePath, bool readOnly)
        {

            isf = IsolatedStorageFile.GetUserStoreForApplication();

            file = new IsolatedStorageFileStream(filePath, FileMode.OpenOrCreate,
                                  readOnly ? FileAccess.Read : FileAccess.ReadWrite, isf);
            this.filePath = filePath;
        }



    }
}
#endif