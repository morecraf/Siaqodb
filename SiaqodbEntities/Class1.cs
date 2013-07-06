using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo;

namespace SiaqodbDemoEntities
{
    public class Customer : SqoDataObject
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string code;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

    }
    public class Invoice : SqoDataObject
    {
        private int customerOid;

        public int CustomerOID
        {
            get { return customerOid; }
            set { customerOid = value; }
        }
        private decimal total;

        public decimal Total
        {
            get { return total; }
            set { total = value; }
        }
        private int number;

        public int Number
        {
            get { return number; }
            set { number = value; }
        }


    }
}
