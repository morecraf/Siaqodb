using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Sqo;

namespace Sqo
{
	/// <summary>
	/// Main interface to be used by implementers to retrieve objects from database
	/// </summary>
	/// <typeparam name="T"></typeparam>
    public interface IObjectList<T>:IList<T>
	{
	
	}
}
