using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Utilities
{
    internal class ObjectTable
    {
        Dictionary<string, int> columns = new Dictionary<string, int>();
        Dictionary<string, Type> columnTypes = new Dictionary<string, Type>();
        List<ObjectRow> rows = new List<ObjectRow>();
        public ObjectTable()
        {
            
        }
        public ObjectRow NewRow()
        {
            return new ObjectRow(this);
        }
        public List<ObjectRow> Rows
        {
            get { return rows;}
        }
        public Dictionary<string, int> Columns
        {
            get { return columns; }
        }
    }
}
