
using Sqo.Exceptions;
using Sqo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Indexes
{
    class ByteConverterIdx
    {
        public static byte[] BooleanToByteArray(bool b)
        {

            byte[] ab = new byte[1];
            if (b)
            {
                ab[0] = (byte)1;
                return ab;
            }
            // default is 0
            return ab;
        }

        public static bool ByteArrayToBoolean(byte[] bytes)
        {
            if (bytes[0] == 0)
            {
                bytes = null;
                return false;
            }
            bytes = null;
            return true;
        }

        public static byte[] ShortToByteArray(short s)
        {

            return BitConverter.GetBytes(s);
        }

        public static short ByteArrayToShort(byte[] bytes)
        {

            return BitConverter.ToInt16(bytes, 0);
        }



        public static byte[] CharToByteArray(char c)
        {

            return BitConverter.GetBytes(c);
        }
        public static char ByteArrayToChar(byte[] bytes)
        {

            return BitConverter.ToChar(bytes, 0);
        }

        public static byte[] IntToByteArray(int l)
        {

            return BitConverter.GetBytes(l);
        }
        public static int ByteArrayToInt(byte[] bytes)
        {

            return BitConverter.ToInt32(bytes, 0);
        }



        public static byte[] LongToByteArray(long l)
        {

            return BitConverter.GetBytes(l);
        }
        public static long ByteArrayToLong(byte[] bytes)
        {

            return BitConverter.ToInt64(bytes, 0);
        }

        public static byte[] DateToByteArray(System.DateTime date)
        {
            return LongToByteArray(date.Ticks);
        }

        public static System.DateTime ByteArrayToDate(byte[] bytes)
        {
            return new System.DateTime(ByteArrayToLong(bytes));
        }

        public static byte[] FloatToByteArray(float f)
        {

            return BitConverter.GetBytes(f);

        }
        public static float ByteArrayToFloat(byte[] bytes)
        {

            return BitConverter.ToSingle(bytes, 0);
        }


        public static byte[] DoubleToByteArray(double d)
        {

            return BitConverter.GetBytes(d);
        }
        public static double ByteArrayToDouble(byte[] bytes)
        {

            return BitConverter.ToDouble(bytes, 0);
        }
        public static string ByteArrayToString(byte[] bytes)
        {

#if SILVERLIGHT || CF || WinRT

            string str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

#else
            string str = Encoding.UTF8.GetString(bytes);

#endif
            return str;
        }
        public static byte[] StringToByteArray(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        internal static byte[] GetBytes(object obj, Type objectType)
        {
            if (objectType == typeof(int)) return EndianBitConverter.Big.GetBytes((int)obj);
            if (objectType == typeof(bool)) return EndianBitConverter.Big.GetBytes((bool)obj);
            if (objectType == typeof(byte)) return new byte[] { (byte)obj };
            if (objectType == typeof(sbyte)) return new byte[] { (byte)((sbyte)obj) };
            if (objectType == typeof(short)) return EndianBitConverter.Big.GetBytes((short)obj);
            if (objectType == typeof(ushort)) return EndianBitConverter.Big.GetBytes((ushort)obj);
            if (objectType == typeof(uint)) return EndianBitConverter.Big.GetBytes((uint)obj);
            if (objectType == typeof(long)) return EndianBitConverter.Big.GetBytes((long)obj);
            if (objectType == typeof(ulong)) return EndianBitConverter.Big.GetBytes((ulong)obj);
            if (objectType == typeof(float)) return EndianBitConverter.Big.GetBytes((float)obj);
            if (objectType == typeof(double)) return EndianBitConverter.Big.GetBytes((double)obj);
            if (objectType == typeof(decimal)) return EndianBitConverter.Big.GetBytes((decimal)obj);
            if (objectType == typeof(DateTime)) return EndianBitConverter.Big.GetBytes(((DateTime)obj).Ticks);
            if (objectType == typeof(char)) return EndianBitConverter.Big.GetBytes((char)obj);
            if (objectType == typeof(string)) return StringToByteArray((string)obj);
            if (objectType == typeof(Guid)) return ((Guid)obj).ToByteArray();
            if (objectType == typeof(TimeSpan)) return EndianBitConverter.Big.GetBytes((((TimeSpan)obj).Ticks));
            if (objectType.IsEnum()) return EnumToByteArray(obj, objectType);

            if (objectType == typeof(IntPtr)) throw new NotSupportedException("IntPtr type is not supported.");
            if (objectType == typeof(UIntPtr)) throw new NotSupportedException("UIntPtr type is not supported.");

            throw new NotSupportedException("Could not retrieve bytes from the object type " + objectType.FullName + ".");
        }


        internal static object ReadBytes(byte[] bytes, Type objectType)
        {
            if (objectType == typeof(bool)) return EndianBitConverter.Big.ToBoolean(bytes, 0);
            if (objectType == typeof(byte)) return bytes[0];
            if (objectType == typeof(sbyte)) return (sbyte)bytes[0];
            if (objectType == typeof(short)) return EndianBitConverter.Big.ToInt16(bytes, 0);
            if (objectType == typeof(ushort)) return EndianBitConverter.Big.ToUInt16(bytes, 0);
            if (objectType == typeof(int)) return EndianBitConverter.Big.ToInt32(bytes, 0);
            if (objectType == typeof(uint)) return EndianBitConverter.Big.ToUInt32(bytes, 0);
            if (objectType == typeof(long)) return EndianBitConverter.Big.ToInt64(bytes, 0);
            if (objectType == typeof(ulong)) return EndianBitConverter.Big.ToUInt64(bytes, 0);
            if (objectType == typeof(float)) return EndianBitConverter.Big.ToSingle(bytes, 0);
            if (objectType == typeof(double)) return EndianBitConverter.Big.ToDouble(bytes, 0);
            if (objectType == typeof(decimal)) return EndianBitConverter.Big.ToDecimal(bytes, 0);
            if (objectType == typeof(char)) return EndianBitConverter.Big.ToChar(bytes, 0);
            if (objectType == typeof(DateTime)) return new DateTime(EndianBitConverter.Big.ToInt64(bytes, 0));
            if (objectType == typeof(string)) return ByteArrayToString(bytes);
            if (objectType == typeof(Guid)) return new Guid(bytes);
            if (objectType == typeof(TimeSpan)) return TimeSpan.FromTicks(EndianBitConverter.Big.ToInt64(bytes, 0));
            if (objectType.IsEnum())
            {
                Type enumType = Enum.GetUnderlyingType(objectType);
                object realObject = ReadBytes(bytes, enumType);
                return Enum.ToObject(objectType, realObject);
            }
                throw new NotSupportedTypeException("Could not retrieve bytes from the object type " + objectType.FullName + ".");
        }
        public static byte[] EnumToByteArray(object obj, Type objectType)
        {
            Type enumType = Enum.GetUnderlyingType(objectType);

            object realObject = Convertor.ChangeType(obj, enumType);
            return GetBytes(realObject, enumType);
        }
    }
}

