using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ValueStructure
    {
        public IntPtr size;

        public IntPtr data;
        public byte[] GetBytes()
        {
            var buffer = new byte[size.ToInt32()];
            Marshal.Copy(data, buffer, 0, buffer.Length);
            return buffer;
        }
        internal byte[] ToByteArray(int resultCode)
        {
            if (resultCode == NativeMethods.MDB_NOTFOUND)
                return null;

            var buffer = new byte[this.size.ToInt32()];
#if XIOS || MONODROID || UNITY3D
			if (this.data != IntPtr.Zero) 
#endif
            Marshal.Copy(this.data, buffer, 0, buffer.Length);

            return buffer;
        }
    }
}
