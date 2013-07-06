using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Attributes;

namespace Sqo.Indexes
{
    [System.Reflection.Obfuscation(Exclude = true)]
    class IndexInfo2
    {
        public int OID { get; set; }
        public int RootOID { get; set; }
        [Text]
        public string IndexName { get; set; }
#if SILVERLIGHT
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {

            field.SetValue(this, value);

        }
#endif
    }
}
