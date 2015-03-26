using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Sqo.Meta
{

	
	class SqoTypeInfo
	{
		Type type;
        public SqoTypeInfo(Type type)
        {


            this.type = type;
#if SILVERLIGHT
			string tName = type.AssemblyQualifiedName;
#else

            string tName = BuildTypeName(type);
#endif

            this.typeName = tName;


        }

        
		public SqoTypeInfo()
		{

		}
        private string BuildTypeName(Type type)
        {
            
            string onlyTypeName = type.Namespace+"."+type.Name;
            AddGenericsInfo(type, ref onlyTypeName);

            #if SILVERLIGHT
            string assemblyName = type.Assembly.FullName.Split(',')[0];
#elif WinRT
            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
#else
            string assemblyName = type.Assembly.GetName().Name;
#endif

            string[] tNames = new string[] { onlyTypeName, assemblyName };

            return tNames[0] + ", " + tNames[1];

        }
        private void AddGenericsInfo(System.Type type, ref string onlyTypeName)
        {
            if (type.IsGenericType())
            {
                Type[] gParams = type.GetGenericArguments();
                StringBuilder builder = new StringBuilder(onlyTypeName);
                builder.Append("[");
                for (int i = 0; i < gParams.Length; ++i)
                {
                    if (i > 0) builder.Append(", ");
                    builder.Append("[");
                    builder.Append(BuildTypeName(gParams[i]));
                    builder.Append("]");
                }
                builder.Append("]");
                onlyTypeName = builder.ToString();
            }

        }
		private string typeName;

		public string TypeName
		{
			get { return typeName; }
			set { typeName = value; }
		}
        public Type Type
        {
            get { return type; }
            set { type = value; }
        }
        public List<FieldSqoInfo> Fields = new List<FieldSqoInfo>();

        private TypeHeader header=new TypeHeader();
        public TypeHeader Header
        {
            get { return header; }
        }
        public List<FieldSqoInfo> UniqueFields = new List<FieldSqoInfo>();
        public List<FieldSqoInfo> IndexedFields = new List<FieldSqoInfo>();

		public bool IsOld;
        string fileNameForManager;
        public string FileNameForManager
        {
            get { return this.fileNameForManager; }
            set { this.fileNameForManager = value; }
        }
        public string GetDBName()
        {
            string customName = Cache.CacheCustomFileNames.GetFileName(typeName);
            if (customName != null)
            {
                return customName;
            }

            string assemblyName = typeName.Substring(typeName.LastIndexOf(',') + 1);
            string onlyTypeName = typeName.Substring(0, typeName.LastIndexOf(','));
            string fileName =string.Format("{0}.{1}" , onlyTypeName,assemblyName);
            return fileName;
        }
	
	}
}
