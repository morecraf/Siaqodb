using System;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using MonoMac;

namespace SiaqodbManager.Util
{
	public class BindingUtil
	{
		private static NSDictionary continuoslyUpdateValue = NSDictionary.FromObjectAndKey (NSNumber.FromInt32 (1), ContinuouslyUpdatesValueBindingOptionKey);

		static NSString continuouslyUpdatesValueBindingOptionKey;

		private static NSString ContinuouslyUpdatesValueBindingOptionKey{
			get
			{
				if (continuouslyUpdatesValueBindingOptionKey == null) {
					var libHandle = Dlfcn.dlopen(Constants.AppKitLibrary,0);
					continuouslyUpdatesValueBindingOptionKey = Dlfcn.GetStringConstant (libHandle,"NSContinuouslyUpdatesValueBindingOption");
					Dlfcn.dlclose (libHandle);
				}
				return continuouslyUpdatesValueBindingOptionKey;
			}
		}
		public static NSDictionary ContinuouslyUpdatesValue{ get{ return continuoslyUpdateValue;}} 
	}
}

