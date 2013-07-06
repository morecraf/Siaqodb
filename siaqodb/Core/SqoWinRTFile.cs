using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
namespace Sqo.Core
{
    internal class SqoWinRTFile : ISqoFile
    {
        protected StorageFile file;
        string folderPath;
        string fileName;
        Stream streamReader;
        Stream streamWriter;
        FileRandomAccessStream fileStream;
        StorageFolder storageFolder;
        
        public virtual async Task WriteAsync(long pos, byte[] buf)
        {

            streamWriter.Seek(pos,SeekOrigin.Begin);
            await streamWriter.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            
        }
        public virtual async Task WriteAsync(byte[] buf)
        {

            await streamWriter.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            
        }
        public virtual async Task<int> ReadAsync(long pos, byte[] buf)
        {

            streamReader.Seek(pos,SeekOrigin.Begin);
            return await streamReader.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
            
        }
        public virtual void Write(long pos, byte[] buf)
        {

            streamWriter.Seek(pos, SeekOrigin.Begin);
            streamWriter.WriteAsync(buf, 0, buf.Length).Wait();

        }
        public virtual void Write(byte[] buf)
        {

            streamWriter.WriteAsync(buf, 0, buf.Length).Wait();

        }
        public virtual int Read(long pos, byte[] buf)
        {

            streamReader.Seek(pos, SeekOrigin.Begin);
            return streamReader.ReadAsync(buf, 0, buf.Length).Result;

        }
        public bool IsClosed
        {
            get { return this.isClosed; }
        }
        public void Flush()
        {
            try
            {
                if (fileStream != null)//possible some files are still pending in Serializer-> like TRansactionHeader, but are closed and not opened again
                {
                    streamWriter.FlushAsync().Wait();
                    fileStream.FlushAsync().AsTask().Wait();
                }
            }
            catch (FileNotFoundException ex)//did not found a better way to check if file exists
            {

            }
        }
        public async Task FlushAsync()
        {
            try
            {
                if (fileStream != null)//possible some files are still pending in Serializer-> like TRansactionHeader, but are closed and not opened again
                {
                    await streamWriter.FlushAsync().ConfigureAwait(false);
                    await fileStream.FlushAsync();
                }
            }
            catch (FileNotFoundException ex)//did not found a better way to check if file exists
            { 
                
            }
        }


        bool isClosed = false;
        public  void Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                fileStream.Dispose();
                fileStream = null;
            }
        }

        public long Length
        {
            get { return (long)fileStream.Size; }
            set { fileStream.Size = (ulong)value; }
        }

        internal SqoWinRTFile(String filePath, bool readOnly)
        {

            this.folderPath = filePath.Remove(filePath.LastIndexOf('\\'));
            this.fileName = filePath.Substring(filePath.LastIndexOf('\\') + 1);
            isClosed = false;
            storageFolder = StorageFolder.GetFolderFromPathAsync(folderPath).AsTask().Result;
            this.file = storageFolder.CreateFileAsync(this.fileName, CreationCollisionOption.OpenIfExists).AsTask().Result;
            this.fileStream = (FileRandomAccessStream)file.OpenAsync(FileAccessMode.ReadWrite).AsTask().Result;
            this.streamWriter = fileStream.AsStreamForWrite();
            this.streamReader = fileStream.AsStreamForRead();

        }
       
        

    }

    

}
