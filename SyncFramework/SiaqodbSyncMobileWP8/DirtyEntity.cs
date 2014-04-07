using Sqo.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbSyncMobile
{
    enum DirtyOperation { Inserted = 1, Updated = 2, Deleted = 3 }
    class DirtyEntity
    {
        public int OID { get; set; }
        public int EntityOID;
        [MaxLength(200)]
        public string EntityType;
        public DirtyOperation DirtyOp;
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
        public IList TombstoneList { get; set; }
    }
}
