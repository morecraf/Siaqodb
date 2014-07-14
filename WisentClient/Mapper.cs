using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cryptonor
{
    static class  Mapper
    {
        public static string GetTagByType(Type type)
        {

            if (type == typeof(int))
                return "tags_int";
            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
                return "tags_datetime";
            else if (type == typeof(float))
                return "tags_float";
            else if (type == typeof(decimal))
                return "tags_decimal";
            else if (type == typeof(double))
                return "tags_double";
            else if (type == typeof(string))
                return "tags_string";
            else if (type == typeof(long))
                return "tags_long";
            else if (type == typeof(bool))
                return "tags_bool";
            else if (type == typeof(Guid))
                return "tags_guid";


            throw new Exception("Type :" + type.ToString() + " not supported!");
        }
        public static string URLEncode(object value)
        {
            if (value.GetType() == typeof(DateTime))
            {
                return HttpUtility.UrlEncode(((DateTime)value).ToString("yyyy-MM-dd"));
            }
            return HttpUtility.UrlEncode(value.ToString());
        }
    }
}
