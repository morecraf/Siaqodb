using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using Sqo.Exceptions;

namespace Sqo.Utilities
{
    class ObjectTableHelper
    {
        public static ObjectList<T> CreateObjectsFromTable<T>(ObjectTable table, SqoTypeInfo actualType)
        {
            ObjectList<T> obList = new ObjectList<T>();
            foreach (ObjectRow row in table.Rows)
            {
                T currentObj = default(T);
                currentObj = Activator.CreateInstance<T>();
                //ISqoDataObject dObj = currentObj as ISqoDataObject;

                foreach (string column in table.Columns.Keys)
                {
                    FieldSqoInfo fi = MetaHelper.FindField(actualType.Fields, column);
                    if (fi != null)
                    {
#if SILVERLIGHT
                            
                        try
                            {
                                //dObj.SetValue(fi.FInfo, row[column]);
                                MetaHelper.CallSetValue(fi.FInfo, row[column], currentObj, actualType.Type);
                                
                            }
                            catch (Exception ex)
                            {
                                throw new SiaqodbException("Override GetValue and SetValue methods of SqoDataObject-Silverlight limitation to private fields");
                            }
                            
#else
                        if (row[column] != null && fi.FInfo.FieldType.IsNullableEnum())
                        {
                            Type enumType = Nullable.GetUnderlyingType(fi.FInfo.FieldType);
                            row[column] = Enum.ToObject(enumType, row[column]);

                        }
                        fi.FInfo.SetValue(currentObj, row[column]);
#endif

                    }
                }


                obList.Add(currentObj);

            }
            return obList;
        }
    }
}
