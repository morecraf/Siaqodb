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
    internal class SqoISFile:ISqoFile
    {
        protected IsolatedStorageFileStream phisicalFile;
        private MemoryStream file;
        private IsolatedStorageFile isf;
        private string filePath;
        bool isModified;

        public virtual void Write(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            file.Write(buf, 0, buf.Length);
            isModified = true;
        }
        public virtual void Write(byte[] buf)
        {
            file.Write(buf, 0, buf.Length);
            isModified = true;
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
            if (!this.IsClosed && isModified)
            {
                file.Flush();
                byte[] bytes = file.GetBuffer();
                phisicalFile = new IsolatedStorageFileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, isf);

                phisicalFile.Seek(0, SeekOrigin.Begin);
                phisicalFile.Write(bytes, 0, bytes.Length);
                phisicalFile.Close();
                isModified = false;
            }

        }
#if ASYNC
        public async Task WriteAsync(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            await file.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            isModified = true;
        }
        public async Task WriteAsync(byte[] buf)
        {
            await file.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            isModified = true;
        }
        public async Task<int> ReadAsync(long pos, byte[] buf)
        {
            file.Seek(pos, SeekOrigin.Begin);
            return await file.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }
        public async Task FlushAsync()
        {
            if (!this.IsClosed && isModified)
            {
                await file.FlushAsync().ConfigureAwait(false);
                byte[] bytes = file.GetBuffer();
                phisicalFile = new IsolatedStorageFileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, isf);

                phisicalFile.Seek(0, SeekOrigin.Begin);
                await phisicalFile.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                phisicalFile.Close();
                isModified = false ;
            }

        }
#endif

        bool isClosed = false;
        public virtual void Close()
        {
            isClosed = true;
            file.Close();
            

            phisicalFile.Close();
            isModified = false;
        }
        public long Length
        {
            get { return file.Length; }
            set { file.SetLength(value); }
        }


        internal SqoISFile(String filePath, bool readOnly)
        {

            isf = IsolatedStorageFile.GetUserStoreForApplication();

            phisicalFile = new IsolatedStorageFileStream(filePath, FileMode.OpenOrCreate,
                                  readOnly ? FileAccess.Read : FileAccess.ReadWrite, isf);
            file = new MemoryStream();
            byte[] fullFile = new byte[phisicalFile.Length];

            phisicalFile.Read(fullFile, 0, fullFile.Length);
            file.Write(fullFile, 0, fullFile.Length);
            phisicalFile.Close();
            this.filePath = filePath;
        }



    }
}
#endif