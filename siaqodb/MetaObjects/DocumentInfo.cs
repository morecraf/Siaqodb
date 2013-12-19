using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.MetaObjects
{
     [System.Reflection.Obfuscation(Exclude = true)]
    class DocumentInfo
    {
        public int OID { get; set; }
        [Attributes.MaxLength(300)]
        public string TypeName;
        public byte[] Document;
       

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
