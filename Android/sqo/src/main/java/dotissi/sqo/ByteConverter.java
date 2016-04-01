package dotissi.sqo;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.nio.charset.Charset;
import java.util.Date;


class ByteConverter {
    public static byte[] BooleanToByteArray(boolean b)
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
    public static boolean ByteArrayToBoolean(byte[] bytes)
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
        return serialize(s);
    }

    public static short ByteArrayToShort(byte[] bytes)
    {
        return (short) deserialize(bytes);
    }



    public static byte[] CharToByteArray(char c)
    {
        return serialize(c);
    }
    public static char ByteArrayToChar(byte[] bytes)
    {

        return (char) deserialize(bytes);
    }

    public static byte[] IntToByteArray(int l)
    {

        return serialize(l);
    }
    public static int ByteArrayToInt(byte[] bytes)
    {

        return (int) deserialize(bytes);
    }



    public static byte[] LongToByteArray(long l)
    {

        return serialize(l);
    }
    public static long ByteArrayToLong(byte[] bytes)
    {

        return (long) deserialize(bytes);
    }

    public static byte[] DateToByteArray(Date date)
    {
        return LongToByteArray(date.getTime());
    }

    public static Date ByteArrayToDate(byte[] bytes)
    {
        return new Date(ByteArrayToLong(bytes));
    }

    public static byte[] FloatToByteArray(float f)
    {

        return serialize(f);

    }
    public static float ByteArrayToFloat(byte[] bytes)
    {

        return (float) deserialize(bytes);
    }


    public static byte[] DoubleToByteArray(double d)
    {

        return serialize(d);
    }
    public static double ByteArrayToDouble(byte[] bytes)
    {

        return (double) deserialize(bytes);
    }
    public static String ByteArrayToString(byte[] bytes)
    {
        String str = new String(bytes,Charset.forName("UTF-8"));
        return str;
    }
    public static byte[] StringToByteArray(String str)
    {
        return  str.getBytes(Charset.forName("UTF-8"));
    }
    public static byte[] GetBytes(Object obj, Class objectType)
    {
        if (objectType == int.class) return serialize((int)obj);
        if (objectType == boolean.class) return serialize((boolean)obj);
        if (objectType == byte.class) return new byte[] { (byte)obj };
        if (objectType == short.class) return serialize((short)obj);
        if (objectType == long.class) return serialize((long)obj);
        if (objectType == float.class) return serialize((float)obj);
        if (objectType == double.class) return serialize((double)obj);
        if (objectType == Date.class) return serialize(((Date)obj));
        // if (objectType == DateTime.class) return serialize(((DateTime)obj));
        if (objectType == char.class) return serialize((char)obj);
        if (objectType == String.class) return StringToByteArray((String)obj);
        if (objectType == Integer.class) return serialize((Integer)obj);
        if (objectType == Boolean.class) return serialize((Boolean)obj);
        if (objectType == Short.class) return serialize((Short)obj);
        if (objectType == Long.class) return serialize((Long)obj);
        if (objectType == Float.class) return serialize((Float)obj);
        if (objectType == Double.class) return serialize((Double)obj);
        //if (objectType == Date.class) return serialize(((DateTime)obj));


        throw new UnsupportedOperationException("Could not retrieve bytes from the Object type " + objectType.getName() + ".");
    }


    public static Object ReadBytes(byte[] bytes, Class objectType)
    {
        if (objectType == boolean.class) return deserialize(bytes);
        if (objectType == byte.class) return bytes[0];
        if (objectType == short.class) return deserialize(bytes);
        if (objectType == int.class) return deserialize(bytes);
        if (objectType == long.class) return deserialize(bytes);
        if (objectType == float.class) return deserialize(bytes);
        if (objectType == double.class) return deserialize(bytes);
        if (objectType == char.class) return deserialize(bytes);
        if (objectType == Date.class) return  deserialize(bytes);
        // if (objectType == DateTime.class) return deserialize(bytes);
        if (objectType == Integer.class) return deserialize(bytes);
        if (objectType == Boolean.class) return deserialize(bytes);
        if (objectType == Short.class) return deserialize(bytes);
        if (objectType == Long.class) return deserialize(bytes);
        if (objectType == Float.class) return deserialize(bytes);
        if (objectType == Double.class) return deserialize(bytes);
        if (objectType == Date.class) return deserialize(bytes);
        if (objectType == String.class) return new String(bytes,Charset.forName("UTF-8"));

        throw new UnsupportedOperationException("Could not retrieve bytes from the Object type " + objectType.getName() + ".");
    }
    public static byte[] serialize(Object obj) {
        ByteArrayOutputStream b = new ByteArrayOutputStream();
        try {
            ObjectOutputStream o = new ObjectOutputStream(b);
            o.writeObject(obj);
            return b.toByteArray();
        } catch (IOException ex) {
            throw new UnsupportedOperationException("Could not retrieve bytes from the Object type " + obj.getClass());
        }
    }

    public static Object deserialize(byte[] bytes)  {
        ByteArrayInputStream b = new ByteArrayInputStream(bytes);
        try {
            ObjectInputStream o = new ObjectInputStream(b);
            return o.readObject();
        }
        catch (IOException|ClassNotFoundException ex)
        {
            throw new UnsupportedOperationException("Could not retrieve Object from bytes ");
        }

    }
}