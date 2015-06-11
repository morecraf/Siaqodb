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
    internal class SqoWinRTMemoryFile : ISqoFile
    {
        protected StorageFile physicalFile;
        private MemoryStream file;
      
        string folderPath;
        string fileName;
        MemoryStream stream;
        //Stream streamWriter;
        FileRandomAccessStream fileStream;
        StorageFolder storageFolder;
        bool isModified;

        public virtual async Task WriteAsync(long pos, byte[] buf)
        {

            stream.Seek(pos, SeekOrigin.Begin);
            await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            isModified = true;
        }
        public virtual async Task WriteAsync(byte[] buf)
        {

            await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            isModified = true;

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
            isModified = true;
        }
        public virtual void Write(byte[] buf)
        {

            stream.Write(buf, 0, buf.Length);
            isModified = true;

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
                if (fileStream != null && !this.IsClosed && isModified)
                {
                    stream.Flush();
                    byte[] bytes = stream.ToArray();
                    fileStream.Seek(0);
                    var streamTemp = fileStream.AsStream();

                    streamTemp.Write(bytes, 0, bytes.Length);
                    streamTemp.Flush();

                    fileStream.FlushAsync().AsTask().Wait();
                    isModified = false;
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

                if (fileStream != null && !this.IsClosed && isModified)
                {
                    await stream.FlushAsync().ConfigureAwait(false);
                    byte[] bytes = stream.ToArray();
                    fileStream.Seek(0);
                    var streamTemp = fileStream.AsStream();

                    await streamTemp.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    await streamTemp.FlushAsync().ConfigureAwait(false); ;

                    await fileStream.FlushAsync();
                    isModified = false;
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
                this.isModified = false;
            }
        }

        public long Length
        {
            get { return (long)stream.Length; }
            set { stream.SetLength(value); }
        }

        internal SqoWinRTMemoryFile(String filePath, bool readOnly)
        {

            this.folderPath = filePath.Remove(filePath.LastIndexOf('\\'));
            this.fileName = filePath.Substring(filePath.LastIndexOf('\\') + 1);
            isClosed = false;
            storageFolder = StorageFolder.GetFolderFromPathAsync(folderPath).AsTask().Result;
            this.physicalFile = storageFolder.CreateFileAsync(this.fileName, CreationCollisionOption.OpenIfExists).AsTask().Result;
            this.fileStream = (FileRandomAccessStream)physicalFile.OpenAsync(FileAccessMode.ReadWrite).AsTask().Result;
            stream = new MemoryStream();
            byte[] fullFile = new byte[fileStream.Size];
            var streamTemp = fileStream.AsStream();

            streamTemp.Read(fullFile, 0, fullFile.Length);

            stream.Write(fullFile, 0, fullFile.Length);

        }



    }


#endif
}
