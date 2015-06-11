using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if WinRT
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
#endif
using System.IO;
namespace Dotissi.Core
{
#if WinRT 
    internal class SqoWinRTFile : ISqoFile
    {
        protected StorageFile file;
        string folderPath;
        string fileName;
        Stream stream;
        //Stream streamWriter;
        FileRandomAccessStream fileStream;
        StorageFolder storageFolder;

        public virtual async Task WriteAsync(long pos, byte[] buf)
        {

            stream.Seek(pos, SeekOrigin.Begin);
            await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            /*fileStream.Seek((ulong)pos);
            if (buf.Length > 0)
            {
                await fileStream.WriteAsync(Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(buf));
            }*/
        }
        public virtual async Task WriteAsync(byte[] buf)
        {

            await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            /*if (buf.Length > 0)
            {
                await fileStream.WriteAsync(Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(buf));
            }*/

        }
        public virtual async Task<int> ReadAsync(long pos, byte[] buf)
        {

            stream.Seek(pos, SeekOrigin.Begin);
            return await stream.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
            /*fileStream.Seek((ulong)pos);
            if (buf.Length > 0)
            {
                var rd = await fileStream.ReadAsync(Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(buf), (uint)buf.Length, InputStreamOptions.None);
                rd.CopyTo(buf);
            }
            return (int)buf.Length;
             */


        }
        public virtual void Write(long pos, byte[] buf)
        {

            stream.Seek(pos, SeekOrigin.Begin);
            stream.Write(buf, 0, buf.Length);
            /*fileStream.Seek((ulong)pos);
            if (buf.Length > 0)
            {
                fileStream.WriteAsync(Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(buf)).AsTask().Wait();
            }*/

        }
        public virtual void Write(byte[] buf)
        {

            stream.Write(buf, 0, buf.Length);
            /*if (buf.Length > 0)
            {
                fileStream.WriteAsync(Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(buf)).AsTask().Wait();
            }*/

        }
        public virtual int Read(long pos, byte[] buf)
        {

            stream.Seek(pos, SeekOrigin.Begin);
            return stream.Read(buf, 0, buf.Length);
            /*fileStream.Seek((ulong)pos);
            if (buf.Length > 0)
            {
                var rd = fileStream.ReadAsync(Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(buf), (uint)buf.Length, InputStreamOptions.None).AsTask().Result;
                rd.CopyTo(buf);
            }
            return buf.Length;*/
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
                    //streamWriter.FlushAsync().Wait();
                    stream.Flush();
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
                    //await streamWriter.FlushAsync().ConfigureAwait(false);
                    await stream.FlushAsync();
                    await fileStream.FlushAsync();
                }
            }
            catch (FileNotFoundException ex)//did not found a better way to check if file exists
            {

            }
        }


        bool isClosed = false;
        public void Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                stream.Dispose();
                stream = null;
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
            this.stream = fileStream.AsStream();

            //this.streamReader = fileStream.AsStreamForRead();

        }



    }

#endif

}
