using System;
using Sqo;

namespace SiaqodbManager.Repo
{
	public class SiaqodbRepo
	{
		public static bool Opened { get; private set;}

		private static Siaqodb UniqueInstance;

		public static Siaqodb Instance {
			get{
				if(UniqueInstance == null){
					new SiaqodbRepo ();
				}
				return UniqueInstance;
			}
		}

		private SiaqodbRepo ()
		{
			Opened = false;
		}

		public static void Open(string path){
			UniqueInstance = Sqo.Internal._bs._b(path);;
			Opened = true;
		}

		#region IDisposable implementation

		public static void Dispose ()
		{
			if(UniqueInstance != null){
				UniqueInstance.Close ();
			}
			Opened = false;
		}

		#endregion
	}
}

