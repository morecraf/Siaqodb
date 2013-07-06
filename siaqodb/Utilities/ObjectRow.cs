using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Utilities
{
    internal class ObjectRow 
    {
        internal object[] cells;
        ObjectTable table;
        public ObjectRow(ObjectTable table)
        {
            this.table=table;
            this.cells = new object[table.Columns.Count];
        }
        public object this[string name]
        {
            get
            {
                

                return cells[table.Columns[name]];
            }
            set
            {

                cells[table.Columns[name]] = value;
            }
        }
        public object this[int index]
        {
            get
            {
                return cells[index];
            }
            set
            {
                cells[index] = value;
            }
        }
    }
}
