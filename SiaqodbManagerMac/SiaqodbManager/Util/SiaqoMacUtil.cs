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
			object value = null;
			var stringValue = (macValue as NSString).ToString();
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
			if(metaField.FieldType == typeof(Int32[])){
				var intValue = new Int32[macValue.Length];
				foreach(var item in macValue){
					intValue[i] = Convert.ToInt32(item);
					i++;
				}
				value = intValue;
//			}else if(metaField.FieldType == typeof(Int16)){
//				value = Convert.ToInt16(stringValue);
//			}else if(metaField.FieldType == typeof(Int64)){
//				value = Convert.ToInt64(stringValue);
//			}else if(metaField.FieldType == typeof(Double)){
//				value = Convert.ToDouble(stringValue);
//			}else if(metaField.FieldType == typeof(UInt16)){
//				value = Convert.ToUInt16(stringValue);
//			}else if(metaField.FieldType == typeof(UInt32)){
//				value = Convert.ToUInt32(stringValue);
//			}else if(metaField.FieldType == typeof(UInt64)){
//				value = Convert.ToUInt64(stringValue);
//			}else if(metaField.FieldType == typeof(float)){
//				value = Convert.ToDouble(stringValue);
//			}else if(metaField.FieldType == typeof(decimal)){
//				value = Convert.ToDecimal(stringValue);
//			}else if(metaField.FieldType == typeof(bool)){
//				value = Convert.ToBoolean(stringValue);
//			}else if(metaField.FieldType == typeof(DateTime)){
//				value = Convert.ToDateTime(stringValue);
			}
			return value;
		}
	}
}

