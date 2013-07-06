using Sqo.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbSyncMobile
{
    class DirtyEntity
    {
        public int OID { get; set; }
        public int EntityOID;
        [MaxLength(200)]
        public string EntityType;                                                                                                                                                               
        public bool IsTombstone;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
    }
    class Anchor
    {
        public DateTime TimeStamp;
        [MaxLength(200)]
        public string EntityType;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
    }
    class DownloadedBatch
    {
       
        public DateTime TimeStamp{get;set;}
        public IList ItemsList{get;set;}
    }
}
