using Sqo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbCloud
{
    public interface IConflictResolver
    {
        Document Resolve(Document local, Document online);
        
    }
}
