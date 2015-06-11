using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dotissi.Meta;
using Dotissi.Meta;

namespace Dotissi.Cache
{
    class CacheForManager
    {
        private Dictionary<string, SqoTypeInfo> cache = new Dictionary<string, SqoTypeInfo>();

        public void AddType(string type, SqoTypeInfo ti)
        {
            cache[type] = ti;
        }
        public SqoTypeInfo GetSqoTypeInfo(string t)
        {
            return cache[t];
        }
        public bool Contains(string type)
        {
            return cache.ContainsKey(type);
        }
    }
}
