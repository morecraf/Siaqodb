using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sqo;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.Transactions;

namespace TestSiaqodbAsync.Models
{
    class ParentObject
    {
        [Index]
        public int ID;
        [UseVariable("ID")]
        public int IDProp { get { return ID; } }

        public int IDPropWithoutAtt { get { return ID; } }

        [UseVariable("IDs")]
        public int IDPropWithNonExistingVar { get { if (ID > 9) return 1; else return -1; } }
        [MaxLength(20)]
        public string Name;
        public bool IsTrue(string s)
        {
            return s == "ADH3";
        }
        public string stringWithoutAtt;

        private int oid;
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
        private ulong tickCount;

        public PropertyADO PropData { get; set; }
    }
}
