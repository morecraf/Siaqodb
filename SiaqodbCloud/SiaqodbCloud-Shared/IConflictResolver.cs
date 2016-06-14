using Sqo.Documents;


namespace SiaqodbCloud
{
    public interface IConflictResolver
    {
        Document Resolve(Document local, Document online);
        
    }
}
