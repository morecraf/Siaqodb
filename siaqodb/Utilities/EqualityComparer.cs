using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Utilities
{
#if UNITY3D
    class EqualityComparer<T>:IEqualityComparer<T>
    {
        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            if (x.GetType() == typeof(int))
            {
                return Convert.ToInt32(x) ==  Convert.ToInt32(y);
            }
            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        #endregion

    }
#endif
}
