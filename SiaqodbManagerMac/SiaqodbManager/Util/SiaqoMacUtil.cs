using System;
using MonoMac.Foundation;
using SiaqodbManager.ViewModel;

namespace SiaqodbManager
{
	public class SiaqoMacUtil
	{
		public SiaqoMacUtil ()
		{
		}

		public static object FromNSObject (NSObject macValue, MetaFieldViewModel metaField)
		{
			var stringValue = (macValue as NSString).ToString();
			object value = stringValue;
			if(metaField.FieldType == typeof(Int32)){
				value = Convert.ToInt32(stringValue);
			}else if(metaField.FieldType == typeof(Int16)){
				value = Convert.ToInt16(stringValue);
			}else if(metaField.FieldType == typeof(Int64)){
				value = Convert.ToInt64(stringValue);
			}else if(metaField.FieldType == typeof(Double)){
				value = Convert.ToDouble(stringValue);
			}else if(metaField.FieldType == typeof(UInt16)){
				value = Convert.ToUInt16(stringValue);
			}else if(metaField.FieldType == typeof(UInt32)){
				value = Convert.ToUInt32(stringValue);
			}else if(metaField.FieldType == typeof(UInt64)){
				value = Convert.ToUInt64(stringValue);
			}else if(metaField.FieldType == typeof(SByte)){
				value = Convert.ToSByte(stringValue);
			}else if(metaField.FieldType == typeof(Byte)){
				value = Convert.ToByte(stringValue);
			}else if(metaField.FieldType == typeof(float)){
				value = Convert.ToDouble(stringValue);
			}else if(metaField.FieldType == typeof(decimal)){
				value = Convert.ToDecimal(stringValue);
			}else if(metaField.FieldType == typeof(bool)){
				value = Convert.ToBoolean(stringValue);
			}else if(metaField.FieldType == typeof(DateTime)){
				value = Convert.ToDateTime(stringValue);
			}
			return value;
		}

		public static object FromNSObject (Array macValue, MetaFieldViewModel metaField)
		{
			object value = null;
			var stringValue = macValue as string[];

			var i = 0;
			if (metaField.FieldType == typeof(Int32[])) {
				var intValue = new Int32[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToInt32 (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(Int16[])){
				var intValue = new Int16[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToInt16 (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(Int64[])){
				var intValue = new Int64[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToInt64 (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(Byte[])){
				var intValue = new Byte[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToByte (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(SByte[])){
				var intValue = new SByte[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToSByte (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(Double[])){
				var intValue = new Double[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToDouble (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(UInt16[])){
				var intValue = new UInt16[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToUInt16 (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(UInt32[])){
				var intValue = new UInt32[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToUInt32 (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(UInt64[])){
				var intValue = new UInt64[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToUInt64 (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(float[])){
				var intValue = new Double[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToDouble (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(bool[])){
				var intValue = new bool[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToBoolean (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(decimal[])){
				var intValue = new decimal[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToDecimal (item);
					i++;
				}
				value = intValue;
			}else if(metaField.FieldType == typeof(DateTime[])){
				var intValue = new DateTime[macValue.Length];
				foreach (var item in macValue) {
					intValue [i] = Convert.ToDateTime (item);
					i++;
				}
				value = intValue;
			}

			return value;
		}
	}
}

