using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSiaqodbAsync.Models
{
    public class AddressADO
    {
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public List<AddressItemADO> AddressItems { get; set; }
    }
}
