using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo
{
    /// <summary>
    /// Basic class that any siaqodb storable class may inherits from 
    /// </summary>
	[System.Reflection.Obfuscation(Exclude = true)]
    public class SqoDataObject :ISqoDataObject
    {

        private int oid;
        /// <summary>
        /// Object Identifier(unique per Type)
        /// </summary>
        public int OID
        {
            get { return oid; }
            set { oid = value; }
        }
       #if SILVERLIGHT
        /// <summary>
        /// Get value of a field by reflection
        /// </summary>
        /// <param name="field"> field</param>
        /// <returns>value of object</returns>
		protected internal virtual object GetValue(System.Reflection.FieldInfo field)
		{

			return field.GetValue(this);

		}
        /// <summary>
        /// Set value of a field by reflection
        /// </summary>
        /// <param name="field">field</param>
        /// <param name="value">value for field</param>
		protected internal virtual void SetValue(System.Reflection.FieldInfo field, object value)
		{

			field.SetValue(this, value);

		}
      

      

        #region ISqoDataObject Members

        /// <summary>
        /// Get value of a field by reflection
        /// </summary>
        /// <param name="field"> field</param>
        /// <returns>value of object</returns>
        object ISqoDataObject.GetValue(System.Reflection.FieldInfo field)
        {
            return this.GetValue(field);
        }
        /// <summary>
        /// Set value of a field by reflection
        /// </summary>
        /// <param name="field">field</param>
        /// <param name="value">value for field</param>
        void ISqoDataObject.SetValue(System.Reflection.FieldInfo field, object value)
        {
            this.SetValue(field, value);
        }

        #endregion

#endif
    }
}
