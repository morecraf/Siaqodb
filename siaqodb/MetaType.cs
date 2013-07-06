using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo
{
	/// <summary>
	/// Class that describe Type of objects  stored in database
	/// </summary>
    public class MetaType
	{
        /// <summary>
        /// Name of Type stored in database
        /// </summary>
		public string Name { get; set; }

       
		List<MetaField> list=new List<MetaField>();
        /// <summary>
        /// List of fields
        /// </summary>
		public List<MetaField> Fields { get { return list; } }

        public string FileName { get; set; }

        public int TypeID { get; set; }
	}
}
