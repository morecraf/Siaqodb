using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dotissi
{
    internal interface ISqoDataObject
    {
        int OID { get; set; }
#if SILVERLIGHT
        object GetValue(System.Reflection.FieldInfo field);
        void SetValue(System.Reflection.FieldInfo field, object value);
		
#endif
		
    }
}
