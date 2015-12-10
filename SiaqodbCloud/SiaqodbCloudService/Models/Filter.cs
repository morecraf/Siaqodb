using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbCloudService.Models
{
    public class Filter
    {
      
        public string TagName { get; set; }
        public object Value { get; set; }
        public object Start { get; set; }
        public object End { get; set; }
        public int? Skip { get; set; }
        public int? Limit { get; set; }
        public bool? Descending { get; set; }
        public string TagType { get; set; }
        public object[] In { get; set; }
    }
}
