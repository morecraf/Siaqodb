using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Sqo.Meta
{
	class FieldSqoInfo
	{
		
		int attTypeId;
        Type attType;
		public FieldSqoInfo(int attTypeId,Type attType)
		{

            if (attType.IsEnum())
            {
                Type enumType = Enum.GetUnderlyingType(attType);
                this.attType = enumType;
            }
            else
            {
                this.attType = attType;
            }
            this.attTypeId=attTypeId;
            
		}
        public FieldSqoInfo( Type attType)
        {
            this.attType = attType;
        }
        public FieldSqoInfo()
        {

        }
		public int AttributeTypeId
		{
			get { return attTypeId; }
            set { attTypeId = value; }
		}

        public Type AttributeType
		{
            get { return attType; }
            set { attType = value; }
        }

        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        FieldInfo fi;
        public FieldInfo FInfo 
        {
            get{return fi;}
            set { fi = value; }
        }
        private AttributeHeader header=new AttributeHeader();
        public AttributeHeader Header
        {
            get { return header; }
           
        }
        bool isText;
        public bool IsText
        {
            get { return isText; }
            set { this.isText = value; }
        }

	}
}
