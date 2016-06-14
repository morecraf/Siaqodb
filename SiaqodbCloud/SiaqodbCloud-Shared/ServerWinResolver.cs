
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
