
using System.CodeDom;

namespace SiaqodbManager
{
    public static class CodeDomeExt
    {
        public static CodeNamespace AddClass(this CodeNamespace codeNamespace, CodeTypeDeclaration codeType)
        {
            codeNamespace.Types.Add(codeType);

            return codeNamespace;
        }

        public static CodeTypeDeclaration AddMethod(this CodeTypeDeclaration classCode, CodeSnippetTypeMember methodBody)
        {
            classCode.Members.Add(methodBody);

            return classCode;
        }

        public static CodeNamespace Imports(this CodeNamespace codeNamespace, string namespaceName)
        {
            codeNamespace.Imports.Add(new CodeNamespaceImport(namespaceName));

            return codeNamespace;
        }
    }
}
