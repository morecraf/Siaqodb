using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#if ASYNC
using System.Threading.Tasks;
#endif


#if SILVERLIGHT
using System.IO.IsolatedStorage;
#endif

namespace Sqo.Core
{
    internal class SqoFile : ISqoFile
    {
        protected FileStream file;


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
                await file.FlushAsync();

            }

        }
#endif
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


        internal SqoFile(String filePath, bool readOnly)
        {

            file = new FileStream(filePath, FileMode.OpenOrCreate,
                                  readOnly ? FileAccess.Read : FileAccess.ReadWrite);


        }


    }
}
