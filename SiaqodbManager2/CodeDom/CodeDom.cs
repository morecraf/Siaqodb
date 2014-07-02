
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SiaqodbManager
{
    public class CodeDom
    {
        private List<CodeCompileUnit> listCompileUnits = new List<CodeCompileUnit>();
        private List<CodeNamespace> listNamespaces = new List<CodeNamespace>();
        private System.Collections.Specialized.StringCollection listReferencedAssemblies =
            new System.Collections.Specialized.StringCollection();

        public CodeDom()
           
        {
        }



        public static CodeDomProvider Provider()
        {
            var providerOptions = new Dictionary<string, string>(); providerOptions.Add("CompilerVersion", "v4.0");

            return new Microsoft.CSharp.CSharpCodeProvider(providerOptions);

        }

        public CodeNamespace AddNamespace(string namespaceName)
        {
            CodeNamespace codeNamespace = new CodeNamespace(namespaceName);
            listNamespaces.Add(codeNamespace);

            return codeNamespace;
        }
        
        public CodeDom AddReference(string referencedAssembly)
        {
            listReferencedAssemblies.Add(referencedAssembly);

            return this;
        }

        public CodeTypeDeclaration Class(string className)
        {
            return new CodeTypeDeclaration(className);
        }

        public CodeSnippetTypeMember Method(string returnType, string methodName, string paramList, string methodBody)
        {
            return Method(string.Format("public static {0} {1}({2}) {{ {3} }} ", returnType, methodName, paramList, methodBody));
        }

        public CodeSnippetTypeMember Method(string methodName, string paramList, string methodBody)
        {
            return Method("void", methodName, paramList, methodBody);
        }

        public CodeSnippetTypeMember Method(string methodName, string methodBody)
        {
            return Method("void", methodName, "", methodBody);
        }

        public CodeSnippetTypeMember Method(string methodBody)
        {
            return new CodeSnippetTypeMember(methodBody);
        }

        public CodeCompileUnit CompileUnit
        {
            get
            {
                
                CodeCompileUnit compileUnit = new CodeCompileUnit();

                foreach (var ns in listNamespaces)
                    compileUnit.Namespaces.Add(ns);

                return compileUnit;
            }
        }

        public Assembly Compile(OutputErrors renderErrors)
        {
            return Compile(null,renderErrors);
        }

        public Assembly Compile(string assemblyPath,OutputErrors renderErrors)
        {
            CompilerParameters options = new CompilerParameters();
            options.IncludeDebugInformation = false;
            options.GenerateExecutable = false;
            options.GenerateInMemory = (assemblyPath == null);
            
            foreach (string refAsm in listReferencedAssemblies)
                options.ReferencedAssemblies.Add(refAsm);
            if (assemblyPath != null)
                options.OutputAssembly = assemblyPath.Replace('\\', '/');

            CodeDomProvider codeProvider = Provider();

            CompilerResults results =
               codeProvider.CompileAssemblyFromDom(options, CompileUnit);
            codeProvider.Dispose();

            if (results.Errors.Count ==  0)
                return results.CompiledAssembly;

           
			renderErrors("Errors:");
            
            foreach (CompilerError err in results.Errors)
                renderErrors(err.ToString());

            return null;
        }

        public string GenerateCode()
        {
            StringBuilder sb = new StringBuilder();
            TextWriter tw = new IndentedTextWriter(new StringWriter(sb));

            CodeDomProvider codeProvider = Provider();
            codeProvider.GenerateCodeFromCompileUnit(CompileUnit, tw, new CodeGeneratorOptions());
            codeProvider.Dispose();

            tw.Close();

            return sb.ToString();
        }
    }
	public delegate void OutputErrors(string errorLine);
}
