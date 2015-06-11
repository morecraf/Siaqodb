using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dotissi
{
    class SqoColumn
	{

		private Type sourceType;
		public Type SourceType
		{
			get
			{
				return sourceType;
			}
			set
			{
				sourceType = value;
			}
		}
		string sourcePropName;
		public string SourcePropName { get { return sourcePropName; } set { sourcePropName = value; } }
		private bool isFullObject;
		public bool IsFullObject
		{
			get { return isFullObject; }
			set { isFullObject = value; }
		}
	}
}
