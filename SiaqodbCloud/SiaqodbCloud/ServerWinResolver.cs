using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqo.Documents;

namespace SiaqodbCloud
{
    public class ServerWinResolver : IConflictResolver
    {
        public Document Resolve(Document local, Document online)
        {
            return online;
        }
    }
}
