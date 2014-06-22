using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiaqodbManager.Entities
{
    class TypeMetaDetail
    {
        public string TypeName { get; set; }
        public string FileName { get; set; }
        public int NumberOfObjects { get; set; }
        public decimal ObjectSize { get; set; }
        public decimal PhysicalFileSize { get; set; }
    }
}
