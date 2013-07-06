using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Attributes
{
	/// <summary>
	/// Use this attribute if you use a Property and inside that
    /// property use some complex code and when Siaqodb engine is not able 
    /// to get what is backing field of that Property, variableName is used for Siaqodb engine when that property is used
    /// 
	/// </summary>
    [AttributeUsage(AttributeTargets.Property)]
	public class UseVariableAttribute : System.Attribute
	{
		internal string variableName;
		public UseVariableAttribute(string variableName)
		{
			this.variableName = variableName;
		}

	}
}
