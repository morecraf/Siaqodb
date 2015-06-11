using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Dotissi.Utilities;
using System.Collections;
using Sqo.Exceptions;
using Sqo.Utilities;

namespace Dotissi.Core
{
	class ByteConverter
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
		internal static byte[] GetBytes(object obj, Type objectType)
		{
			if (objectType == typeof(int)) return BitConverter.GetBytes((int)obj);
			if (objectType == typeof(bool)) return new byte[] { (bool)obj == true ? (byte)1 : (byte)0 };
			if (objectType == typeof(byte)) return new byte[] { (byte)obj };
			if (objectType == typeof(sbyte)) return new byte[] { (byte)((sbyte)obj) };
			if (objectType == typeof(short)) return BitConverter.GetBytes((short)obj);
			if (objectType == typeof(ushort)) return BitConverter.GetBytes((ushort)obj);
			if (objectType == typeof(uint)) return BitConverter.GetBytes((uint)obj);
			if (objectType == typeof(long)) return BitConverter.GetBytes((long)obj);
			if (objectType == typeof(ulong)) return BitConverter.GetBytes((ulong)obj);
			if (objectType == typeof(float)) return BitConverter.GetBytes((float)obj);
			if (objectType == typeof(double)) return BitConverter.GetBytes((double)obj);
			//if (objectType == typeof(decimal)) return BitConverter.GetBytes((double)obj);
			if (objectType == typeof(char)) return BitConverter.GetBytes((char)obj);

			if (objectType == typeof(IntPtr)) throw new NotSupportedException("IntPtr type is not supported.");
			if (objectType == typeof(UIntPtr)) throw new NotSupportedException("UIntPtr type is not supported.");

			throw new NotSupportedException("Could not retrieve bytes from the object type " + objectType.FullName + ".");
		}
        public static byte[] Encrypt(byte[] by,int length)
        {
            if (Sqo.SiaqodbConfigurator.EncryptedDatabase)
            {
                if (by.Length == length) // length%8==0 is the same
                {
                    Sqo.SiaqodbConfigurator.Encryptor.Encrypt(by, 0, by.Length);
                    return by;
                }
                else //padding
                {
                    byte[] b = new byte[length];
                    Array.Copy(by, 0, b, 0, by.Length);
                    Sqo.SiaqodbConfigurator.Encryptor.Encrypt(b, 0, b.Length);
                    return b;
                }
            }
            return by;

        }
        public static byte[] Decrypt(Type objectType, byte[] bytes)
        {
            if (Sqo.SiaqodbConfigurator.EncryptedDatabase)
            {
                int lengthOfType = -1;
                if (objectType == typeof(string) || objectType==typeof(byte[]))
                {
                    lengthOfType = bytes.Length;
                }
                else
                {
                    lengthOfType = Dotissi.Utilities.MetaHelper.GetLengthOfType(objectType);
                }

                Sqo.SiaqodbConfigurator.Encryptor.Decrypt(bytes, 0, bytes.Length);

                if (bytes.Length == lengthOfType)
                {
                    return bytes;
                }
                else
                {
                    byte[] realBytes = new byte[lengthOfType];
                    Array.Copy(bytes, 0, realBytes, 0, lengthOfType);//un-padd the bytes

                    return realBytes;
                }

            }
            return bytes;
        }
        public static byte[] SerializeValueType(object obj, Type objectType,int length,int realLength,int dbVersion)
        {
            if (objectType.IsGenericType())
            {
                // Nullable type?
                Type genericTypeDef = objectType.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(Nullable<>))
                {
                    objectType = objectType.GetGenericArguments()[0];
                    byte[] b = new byte[length];
                    if (obj == null)
                    {
                        b[0] = 1;// is null
                    }
                    else
                    {
                        b[0] = 0;// is not null
                        byte[] serVal = SerializeValueType(obj, objectType, length - 1, realLength, dbVersion);
                        Array.Copy(serVal, 0, b, 1, length - 1);
                    }
                    return b;
                }
                else throw new NotSupportedTypeException("Other than Nullable<> generic types is not supported");

            }
            else if (objectType == typeof(string))
            {
                if (Sqo.SiaqodbConfigurator.EncryptedDatabase)
                {
                    byte[] b = new byte[realLength];
                    byte[] strOnly = Encoding.UTF8.GetBytes((string)obj);
                    int currentLength = realLength > strOnly.Length ? strOnly.Length : realLength;
                    Array.Copy(strOnly, 0, b, 0, currentLength);
                    //added for Encryption support
                    return Encrypt(b, length);
                }
                else
                {
                    byte[] b = new byte[length];
                    byte[] strOnly = Encoding.UTF8.GetBytes((string)obj);
                    int currentLength = length > strOnly.Length ? strOnly.Length : length;
                    Array.Copy(strOnly, 0, b, 0, currentLength);
                    //added for Encryption support
                    return Encrypt(b, length);
                }



            }
            else if(objectType ==typeof(byte[]))
            {
                byte[] objBytes = (byte[])obj;
                byte[] b = new byte[length];
                Array.Copy(objBytes, 0, b, 0, objBytes.Length);
                return Encrypt(b, length);
            }
            else
            {
                byte[] b = SerializeValueType(obj, objectType, dbVersion);

                return Encrypt(b, length);


            }
        }
		public static byte[] SerializeValueType(object obj, Type objectType,int dbVersion)
		{
			
			if (objectType.IsPrimitive())
			{
				return  GetBytes(obj, objectType);
				
			}
			else if (objectType == typeof(DateTime))
			{
                if (dbVersion <= -25)
                {
                    DateTime dt = (DateTime)obj;
                    if ((Sqo.SiaqodbConfigurator.DateTimeKindToSerialize != null) && (dt.Kind != Sqo.SiaqodbConfigurator.DateTimeKindToSerialize))
                        dt = DateTime.SpecifyKind(dt, Sqo.SiaqodbConfigurator.DateTimeKindToSerialize.Value);
            
                    return GetBytes(dt.Ticks, typeof(long));
                }
                else
                {
#if SILVERLIGHT || CF
                
                    DateTime dt = (DateTime)obj;
                    if (dt.Year < 1601) throw new SiaqodbException("DateTime values must be bigger then 1 Jan 1601");
                    return GetBytes(dt.ToFileTime(), typeof(long));
                
#else

                    return GetBytes(((DateTime)obj).ToBinary(), typeof(long));
#endif
                }

			}
#if !CF
            else if (objectType == typeof(DateTimeOffset))
            {
                DateTimeOffset dt = (DateTimeOffset)obj;
                byte[] ticks = GetBytes(dt.Ticks, typeof(long));
                byte[] offsetTicks = GetBytes(dt.Offset.Ticks, typeof(long));
                byte[] allbytes = new byte[ticks.Length + offsetTicks.Length];
                Array.Copy(ticks, 0, allbytes, 0, ticks.Length);
                Array.Copy(offsetTicks, 0, allbytes, ticks.Length, offsetTicks.Length);
                return allbytes;
            }
#endif
            else if (objectType == typeof(TimeSpan))
            {
                return GetBytes((((TimeSpan)obj).Ticks), typeof(long));
            }
            else if (objectType == typeof(Guid))
            {
                return ((Guid)obj).ToByteArray();

            }
            else if (objectType == typeof(string))
            {
                return Encoding.UTF8.GetBytes((string)obj);
            }
            else if (objectType.IsEnum())
            {
                Type enumType = Enum.GetUnderlyingType(objectType);

                object realObject = Convertor.ChangeType(obj, enumType);

                return SerializeValueType(realObject, enumType, dbVersion);
            }
            else if (objectType == typeof(Decimal))
            {
                int[] bits = Decimal.GetBits((decimal)obj);
                byte[] bytes1 = IntToByteArray(bits[0]);
                byte[] bytes2 = IntToByteArray(bits[1]);
                byte[] bytes3 = IntToByteArray(bits[2]);
                byte[] bytes4 = IntToByteArray(bits[3]);
                byte[] all = new byte[16];
                Array.Copy(bytes1, 0, all, 0, 4);
                Array.Copy(bytes2, 0, all, 4, 4);
                Array.Copy(bytes3, 0, all, 8, 4);
                Array.Copy(bytes4, 0, all, 12, 4);
                return all;

            }
            else
            {

                throw new NotSupportedTypeException("Type: " + objectType.ToString() + " not supported");
            }
		}
        public static object DeserializeValueType(Type objectType, byte[] bytes,bool checkEncrypted,int dbVersion)
        {
            if (checkEncrypted)
            {
                if (objectType.IsGenericType()) //nullable only here because on  DeserializeValueType(Type objectType,byte[] bytes) is used only for Metadata directly and in Metadata is not used Nullable fields
                {
                    // Nullable type?
                    Type genericTypeDef = objectType.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof(Nullable<>))
                    {
                        objectType = objectType.GetGenericArguments()[0];
                        if (bytes[0] == 1)
                        {
                            return null;
                        }
                        else
                        {
                            byte[] realBytes = new byte[bytes.Length - 1];
                            Array.Copy(bytes, 1, realBytes, 0, bytes.Length - 1);
                            return DeserializeValueType(objectType, realBytes,true,dbVersion);
                        }
                    }
                    else throw new NotSupportedTypeException("Type: " + objectType.ToString() + " not supported");
                }

                bytes = Decrypt(objectType, bytes);

                return DeserializeValueType(objectType, bytes,dbVersion);
            }
            else
            {
                return DeserializeValueType(objectType, bytes,dbVersion);
            }
        }
        public static object DeserializeValueType(Type objectType, byte[] bytes, int dbVersion)
        {



            if (objectType.IsPrimitive())
            {

                return ReadBytes(bytes, objectType);
            }
            else if (objectType == typeof(DateTime))
            {
                long readLong = ByteArrayToLong(bytes);
                if (dbVersion <= -25)
                {
                    DateTime dt= new DateTime(readLong);
                    if (Sqo.SiaqodbConfigurator.DateTimeKindToSerialize != null)
                    {
                        return DateTime.SpecifyKind(dt,Sqo.SiaqodbConfigurator.DateTimeKindToSerialize.Value);
                    }
                    return dt;
                }
                else
                {
#if SILVERLIGHT || CF
                
                    return DateTime.FromFileTime(readLong);
                
#else

                    return DateTime.FromBinary(readLong);
#endif
                }

            }
#if !CF
            else if (objectType == typeof(DateTimeOffset))
            {
                byte[] ticks = new byte[8];
                byte[] offsetTicks = new byte[8];
                Array.Copy(bytes, 0, ticks, 0, 8);
                Array.Copy(bytes, 8, offsetTicks, 0, 8);
                return new DateTimeOffset(new DateTime(ByteArrayToLong(ticks)), new TimeSpan(ByteArrayToLong(offsetTicks)));
            }
#endif
            else if (objectType == typeof(TimeSpan))
            {
                long readLong = ByteArrayToLong(bytes);
                return TimeSpan.FromTicks(readLong);
            }
            else if (objectType == typeof(string))
            {
#if SILVERLIGHT || CF || WinRT

				string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
				return s.TrimEnd('\0');
				
#else

                string s = Encoding.UTF8.GetString(bytes);
                return s.TrimEnd('\0');

#endif

            }
            else if (objectType == typeof(Decimal))
            {
                int[] bits = new int[4];
                byte[] bytes1 = new byte[4];
                byte[] bytes2 = new byte[4];
                byte[] bytes3 = new byte[4];
                byte[] bytes4 = new byte[4];

                Array.Copy(bytes, 0, bytes1, 0, 4);
                Array.Copy(bytes, 4, bytes2, 0, 4);
                Array.Copy(bytes, 8, bytes3, 0, 4);
                Array.Copy(bytes, 12, bytes4, 0, 4);


                bits[0] = ByteArrayToInt(bytes1);
                bits[1] = ByteArrayToInt(bytes2);
                bits[2] = ByteArrayToInt(bytes3);
                bits[3] = ByteArrayToInt(bytes4);
                return new Decimal(bits);
            }
            else if (objectType == typeof(Guid))
            {

                return new Guid(bytes);
            }
            else if (objectType.IsEnum())
            {
                Type enumType = Enum.GetUnderlyingType(objectType);
                object realObject = DeserializeValueType(enumType, bytes, dbVersion);
                return Enum.ToObject(objectType, realObject);
            }
            else if (objectType == typeof(byte[]))
            {
                return bytes;
            }
            else
            {

                throw new NotSupportedTypeException("Type: " + objectType.ToString() + " not supported");

            }
        }
		internal static object ReadBytes(byte[] bytes, Type objectType)
		{
			if (objectType == typeof(bool)) return bytes[0] == (byte)1 ? true : false;
			if (objectType == typeof(byte)) return bytes[0];
			if (objectType == typeof(sbyte)) return (sbyte)bytes[0];
			if (objectType == typeof(short)) return BitConverter.ToInt16(bytes, 0);
			if (objectType == typeof(ushort)) return BitConverter.ToUInt16(bytes, 0);
			if (objectType == typeof(int)) return BitConverter.ToInt32(bytes, 0);
			if (objectType == typeof(uint)) return BitConverter.ToUInt32(bytes, 0);
			if (objectType == typeof(long)) return BitConverter.ToInt64(bytes, 0);
			if (objectType == typeof(ulong)) return BitConverter.ToUInt64(bytes, 0);
			if (objectType == typeof(float)) return BitConverter.ToSingle(bytes, 0);
			if (objectType == typeof(double)) return BitConverter.ToDouble(bytes, 0);
			if (objectType == typeof(char)) return BitConverter.ToChar(bytes, 0);

			if (objectType == typeof(IntPtr))
			{
				throw new NotSupportedTypeException("Type: IntPtr not supported");
			}

			throw new NotSupportedTypeException("Could not retrieve bytes from the object type " + objectType.FullName + ".");
		}
	}
}
