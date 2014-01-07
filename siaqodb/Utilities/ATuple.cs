using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Utilities
{
    class ATuple<T,V>
    {
        public T Name { get; set; }
        public V Value { get; set; }
        public ATuple(T name,V value)
        {
            Name = name;
            Value = value;
        }
        
    }
}
