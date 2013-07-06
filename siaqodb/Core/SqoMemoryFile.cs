
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Sqo.Core
{
    internal class SqoMemoryFile:ISqoFile
    {
        protected FileStream phisicalFile;
        private MemoryStream file;
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
                byte[] bytes = file.GetBuffer();
                phisicalFile = new FileStream(filePath, FileMode.OpenOrCreate,FileAccess.ReadWrite);


                phisicalFile.Seek(0, SeekOrigin.Begin);
                phisicalFile.Write(bytes, 0, bytes.Length);
                phisicalFile.Close();
            }

        }


        bool isClosed = false;
        public virtual void Close()
        {
            isClosed = true;
            file.Close();
            

            phisicalFile.Close();
        }
        public long Length
        {
            get { return file.Length; }
            set { file.SetLength(value); }
        }


        internal SqoMemoryFile(String filePath, bool readOnly)
        {


            phisicalFile = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            file = new MemoryStream();
            byte[] fullFile = new byte[phisicalFile.Length];

            phisicalFile.Read(fullFile, 0, fullFile.Length);
            file.Write(fullFile, 0, fullFile.Length);
            phisicalFile.Close();
            this.filePath = filePath;
        }



    }
}
