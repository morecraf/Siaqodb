using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.Entities
{

	public class MetaEventArgs:EventArgs
    {
        public MetaType mType;
        public List<int> oids;
        public MetaEventArgs(MetaType mType, List<int> oids)
        {
            this.mType = mType;
            this.oids = oids;
        }
    }
}
