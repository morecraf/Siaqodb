#if MONODROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Java.IO;

namespace Sqo.Core
{
    class SqoMDFile : ISqoFile
    {

        protected RandomAccessFile file;


        public virtual void Write(long pos, byte[] buf)
        {
            file.Seek(pos);
            file.Write(buf, 0, buf.Length);
        }
        public virtual void Write(byte[] buf)
        {
            file.Write(buf, 0, buf.Length);
        }
        public virtual int Read(long pos, byte[] buf)
        {
            file.Seek(pos);
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
            get { return file.Length(); }
            set { file.SetLength(value); }
        }


        internal SqoMDFile(String filePath, bool readOnly)
        {

            file = new RandomAccessFile(filePath, "rw");


        }

    }
}
#endif