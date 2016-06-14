using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;

using System.Net.Http;
#endif
using System.Reflection;
namespace SiaqodbCloud
{
    static class CompatibilityHelper
    {
        public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(Uri uri)
        {
            string query = uri.Query;

            if ((query.Length > 0) && (query[0] == '?'))
            {
                query = query.Substring(1);
            }
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            int num = (query != null) ? query.Length : 0;
            for (int i = 0; i < num; i++)
            {
                int startIndex = i;
                int num4 = -1;
                while (i < num)
                {
                    char ch = query[i];
                    if (ch == '=')
                    {
                        if (num4 < 0)
                        {
                            num4 = i;
                        }
                    }
                    else if (ch == '&')
                    {
                        break;
                    }
                    i++;
                }
                string str = null;
                string str2 = null;
                if (num4 >= 0)
                {
                    str = query.Substring(startIndex, num4 - startIndex);
                    str2 = query.Substring(num4 + 1, (i - num4) - 1);
                }
                else
                {
                    str2 = query.Substring(startIndex, i - startIndex);
                }


                list.Add(new KeyValuePair<string, string>(Uri.UnescapeDataString(str), Uri.UnescapeDataString(str2)));
            }
            return list;
        }
    }
}
