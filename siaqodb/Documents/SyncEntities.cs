using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents.Sync
{
    [System.Reflection.Obfuscation(Exclude = true)]
    public class ChangeSet
    {
        public List<Document> ChangedDocuments { get; set; }
        public List<DeletedDocument> DeletedDocuments { get; set; }
        public string Anchor { get; set; }
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    public class DeletedDocument
    {
        public string Key { get; set; }
        public string Version { get; set; }
    }
}
