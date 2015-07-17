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
			if(metaField.FieldType == typeof(Int32)){
				var number = macValue as NSString;
				value = Convert.ToInt32(number.ToString());
			}else if(metaField.FieldType == typeof(Int16)){
				var number = macValue as NSNumber;
				value = number.Int16Value;
			}else if(metaField.FieldType == typeof(Int64)){
				var number = macValue as NSNumber;
				value = number.Int64Value;
			}else if(metaField.FieldType == typeof(Double)){
				var number = macValue as NSNumber;
				value = number.DoubleValue;
			}else if(metaField.FieldType == typeof(UInt16)){
				var number = macValue as NSNumber;
				value = number.UInt16Value;
			}else if(metaField.FieldType == typeof(UInt32)){
				var number = macValue as NSNumber;
				value = number.UInt32Value;
			}else if(metaField.FieldType == typeof(UInt64)){
				var number = macValue as NSNumber;
				value = number.UInt64Value;
			}else if(metaField.FieldType == typeof(float)){
				var number = macValue as NSNumber;
				value = number.FloatValue;
			}
			return value;
		}
	}
}

