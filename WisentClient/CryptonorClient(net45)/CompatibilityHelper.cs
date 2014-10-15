using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
using System.Net.Http;
#endif
using System.Reflection;


namespace CryptonorClient
{
    static class  CompatibilityHelper
    {
        public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(Uri uri)
        {      
#if NET 
            var value = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return NameValueToEnumerable(value);
#elif CF
            var value = ParseQueryString(uri.Query);
            return NameValueToEnumerable(value);
#else
            var value = uri.ParseQueryString(); 
            return value;
#endif
        }
         #if NET || CF
        private static IEnumerable<KeyValuePair<string, string>> NameValueToEnumerable(System.Collections.Specialized.NameValueCollection nameValueCollection)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            if (!nameValueCollection.AllKeys.Any())
                return list;

            foreach (var key in nameValueCollection.AllKeys)
            {
                var value = nameValueCollection[key];
                var pair = new KeyValuePair<string, string>(key, value);

                list.Add(pair);
            }
            return list;
        }
#endif
#if CF
        public static System.Collections.Specialized.NameValueCollection ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        public static System.Collections.Specialized.NameValueCollection ParseQueryString(string query, Encoding encoding)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
                return new System.Collections.Specialized.NameValueCollection();
            if (query[0] == '?')
                query = query.Substring(1);

            System.Collections.Specialized.NameValueCollection result = new System.Collections.Specialized.NameValueCollection();
            ParseQueryString(query, encoding, result);
            return result;
        }

        internal static void ParseQueryString(string query, Encoding encoding, System.Collections.Specialized.NameValueCollection result)
        {
            if (query.Length == 0)
                return;

            string decoded =HttpUtility.HtmlDecode(query);
            int decodedLength = decoded.Length;
            int namePos = 0;
            bool first = true;
            while (namePos <= decodedLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (int q = namePos; q < decodedLength; q++)
                {
                    if (valuePos == -1 && decoded[q] == '=')
                    {
                        valuePos = q + 1;
                    }
                    else if (decoded[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                if (first)
                {
                    first = false;
                    if (decoded[namePos] == '?')
                        namePos++;
                }

                string name, value;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1));
                }
                if (valueEnd < 0)
                {
                    namePos = -1;
                    valueEnd = decoded.Length;
                }
                else
                {
                    namePos = valueEnd + 1;
                }
                value = UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos));

                result.Add(name, value);
                if (namePos == -1)
                    break;
            }
        }
        public static string UrlDecode(string text)
        {
            
            text = text.Replace("+", " ");
            return System.Uri.UnescapeDataString(text);
        }
#endif


    }
#if WinRT
    class Path
    {
        public static char DirectorySeparatorChar { get { return '\\'; } }

        internal static string GetDirectoryName(string fullPath)
        {
            return fullPath.Remove(fullPath.LastIndexOf('\\'));

        }
        internal static string GetFileName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf('\\') + 1);
        }
    }
#endif
    static class TypeExtensions
    {
#if WinRT
        public static bool IsAssignableFrom(this Type type, Type fromType)
        {
            return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
        }
        public static Type GetInterface(this Type type, string name, bool ignoreCase)
        {

            List<Type> iTypes = type.GetTypeInfo().ImplementedInterfaces.ToList();
            if (iTypes != null)
            {
                foreach (Type t in iTypes)
                {
                    if (name == t.Name)
                    {
                        return t;
                    }
                }
            }

            return null;
        }
        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }
        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            PropertyInfo pi = type.GetTypeInfo().GetDeclaredProperty(name);
            if (pi == null)
            {
                if (type.GetTypeInfo().BaseType != null)
                {
                    return GetProperty(type.GetTypeInfo().BaseType, name);
                }
            }
            else return pi;

            return null;
        }
        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }
        public static PropertyInfo[] GetProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties.ToArray<PropertyInfo>(); 
        }
        public static MethodInfo GetMethod(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }
        public static FieldInfo[] GetFields(this Type type, BindingFlags flags)
        {
            return type.GetTypeInfo().DeclaredFields.ToArray<FieldInfo>();
        }
        public static Type GetBaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }
        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {

            foreach (ConstructorInfo ctor in type.GetTypeInfo().DeclaredConstructors)
            {
                ParameterInfo[] prinfos = ctor.GetParameters();
                if (prinfos.Length == types.Length)
                {
                    int ok = 0;
                    for (int i = 0; i < prinfos.Length; i++)
                    {
                        if (prinfos[i].ParameterType == types[i])
                        {
                            ok++;
                        }

                    }
                    if (ok == types.Length)
                    {
                        return ctor;
                    }
                }
            }
            return null;
        }
        public static bool IsSubclassOf(this Type type, Type t)
        {
            return t.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }
        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }
#else
        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }
        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }
        public static bool IsClass(this Type type)
        {
            return type.IsClass;
        }
        public static bool IsPrimitive(this Type type  )
        {
            return type.IsPrimitive;
        }
#endif
    }
#if WinRT
    public enum BindingFlags
    {
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
        FlattenHierarchy = 64
    }
#endif

#if CF
    internal class HtmlEntities
    {
        // Fields
        private static string[] _entitiesList = new string[] { 
            "\"-quot", "&-amp", "<-lt", ">-gt", "\x00a0-nbsp", "\x00a1-iexcl", "\x00a2-cent", "\x00a3-pound", "\x00a4-curren", "\x00a5-yen", "\x00a6-brvbar", "\x00a7-sect", "\x00a8-uml", "\x00a9-copy", "\x00aa-ordf", "\x00ab-laquo", 
            "\x00ac-not", "\x00ad-shy", "\x00ae-reg", "\x00af-macr", "\x00b0-deg", "\x00b1-plusmn", "\x00b2-sup2", "\x00b3-sup3", "\x00b4-acute", "\x00b5-micro", "\x00b6-para", "\x00b7-middot", "\x00b8-cedil", "\x00b9-sup1", "\x00ba-ordm", "\x00bb-raquo", 
            "\x00bc-frac14", "\x00bd-frac12", "\x00be-frac34", "\x00bf-iquest", "\x00c0-Agrave", "\x00c1-Aacute", "\x00c2-Acirc", "\x00c3-Atilde", "\x00c4-Auml", "\x00c5-Aring", "\x00c6-AElig", "\x00c7-Ccedil", "\x00c8-Egrave", "\x00c9-Eacute", "\x00ca-Ecirc", "\x00cb-Euml", 
            "\x00cc-Igrave", "\x00cd-Iacute", "\x00ce-Icirc", "\x00cf-Iuml", "\x00d0-ETH", "\x00d1-Ntilde", "\x00d2-Ograve", "\x00d3-Oacute", "\x00d4-Ocirc", "\x00d5-Otilde", "\x00d6-Ouml", "\x00d7-times", "\x00d8-Oslash", "\x00d9-Ugrave", "\x00da-Uacute", "\x00db-Ucirc", 
            "\x00dc-Uuml", "\x00dd-Yacute", "\x00de-THORN", "\x00df-szlig", "\x00e0-agrave", "\x00e1-aacute", "\x00e2-acirc", "\x00e3-atilde", "\x00e4-auml", "\x00e5-aring", "\x00e6-aelig", "\x00e7-ccedil", "\x00e8-egrave", "\x00e9-eacute", "\x00ea-ecirc", "\x00eb-euml", 
            "\x00ec-igrave", "\x00ed-iacute", "\x00ee-icirc", "\x00ef-iuml", "\x00f0-eth", "\x00f1-ntilde", "\x00f2-ograve", "\x00f3-oacute", "\x00f4-ocirc", "\x00f5-otilde", "\x00f6-ouml", "\x00f7-divide", "\x00f8-oslash", "\x00f9-ugrave", "\x00fa-uacute", "\x00fb-ucirc", 
            "\x00fc-uuml", "\x00fd-yacute", "\x00fe-thorn", "\x00ff-yuml", "Œ-OElig", "œ-oelig", "Š-Scaron", "š-scaron", "Ÿ-Yuml", "ƒ-fnof", "ˆ-circ", "˜-tilde", "Α-Alpha", "Β-Beta", "Γ-Gamma", "Δ-Delta", 
            "Ε-Epsilon", "Ζ-Zeta", "Η-Eta", "Θ-Theta", "Ι-Iota", "Κ-Kappa", "Λ-Lambda", "Μ-Mu", "Ν-Nu", "Ξ-Xi", "Ο-Omicron", "Π-Pi", "Ρ-Rho", "Σ-Sigma", "Τ-Tau", "Υ-Upsilon", 
            "Φ-Phi", "Χ-Chi", "Ψ-Psi", "Ω-Omega", "α-alpha", "β-beta", "γ-gamma", "δ-delta", "ε-epsilon", "ζ-zeta", "η-eta", "θ-theta", "ι-iota", "κ-kappa", "λ-lambda", "μ-mu", 
            "ν-nu", "ξ-xi", "ο-omicron", "π-pi", "ρ-rho", "ς-sigmaf", "σ-sigma", "τ-tau", "υ-upsilon", "φ-phi", "χ-chi", "ψ-psi", "ω-omega", "ϑ-thetasym", "ϒ-upsih", "ϖ-piv", 
            " -ensp", " -emsp", " -thinsp", "‌-zwnj", "‍-zwj", "‎-lrm", "‏-rlm", "–-ndash", "—-mdash", "‘-lsquo", "’-rsquo", "‚-sbquo", "“-ldquo", "”-rdquo", "„-bdquo", "†-dagger", 
            "‡-Dagger", "•-bull", "…-hellip", "‰-permil", "′-prime", "″-Prime", "‹-lsaquo", "›-rsaquo", "‾-oline", "⁄-frasl", "€-euro", "ℑ-image", "℘-weierp", "ℜ-real", "™-trade", "ℵ-alefsym", 
            "←-larr", "↑-uarr", "→-rarr", "↓-darr", "↔-harr", "↵-crarr", "⇐-lArr", "⇑-uArr", "⇒-rArr", "⇓-dArr", "⇔-hArr", "∀-forall", "∂-part", "∃-exist", "∅-empty", "∇-nabla", 
            "∈-isin", "∉-notin", "∋-ni", "∏-prod", "∑-sum", "−-minus", "∗-lowast", "√-radic", "∝-prop", "∞-infin", "∠-ang", "∧-and", "∨-or", "∩-cap", "∪-cup", "∫-int", 
            "∴-there4", "∼-sim", "≅-cong", "≈-asymp", "≠-ne", "≡-equiv", "≤-le", "≥-ge", "⊂-sub", "⊃-sup", "⊄-nsub", "⊆-sube", "⊇-supe", "⊕-oplus", "⊗-otimes", "⊥-perp", 
        };
        private static System.Collections.Hashtable _entitiesLookupTable;
        private static object _lookupLockObject = new object();

        internal static char Lookup(string entity)
        {
            if (_entitiesLookupTable == null)
            {
                lock (_lookupLockObject)
                {
                    if (_entitiesLookupTable == null)
                    {
                        System.Collections.Hashtable hashtable = new System.Collections.Hashtable();
                        foreach (string str in _entitiesList)
                        {
                            hashtable[str.Substring(2)] = str[0];
                        }
                        _entitiesLookupTable = hashtable;
                    }
                }
            }
            object obj2 = _entitiesLookupTable[entity];
            if (obj2 != null)
            {
                return (char)obj2;
            }
            return '\0';
        }
    }


    public sealed class HttpUtility
    {
        private static char[] s_entityEndingChars = new char[] { ';', '&' };

        public static string HtmlDecode(string s)
        {
            if (s == null)
            {
                return null;
            }
            if (s.IndexOf('&') < 0)
            {
                return s;
            }
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter output = new System.IO.StringWriter(sb);
            HtmlDecode(s, output);
            return sb.ToString();
        }

        public static void HtmlDecode(string s, System.IO.TextWriter output)
        {
            if (s != null)
            {
                if (s.IndexOf('&') < 0)
                {
                    output.Write(s);
                }
                else
                {
                    int length = s.Length;
                    for (int i = 0; i < length; i++)
                    {
                        char ch = s[i];
                        if (ch == '&')
                        {
                            int num3 = s.IndexOfAny(s_entityEndingChars, i + 1);
                            if ((num3 > 0) && (s[num3] == ';'))
                            {
                                string entity = s.Substring(i + 1, (num3 - i) - 1);
                                if ((entity.Length > 1) && (entity[0] == '#'))
                                {
                                    try
                                    {
                                        if ((entity[1] == 'x') || (entity[1] == 'X'))
                                        {
                                            ch = (char)int.Parse(entity.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
                                        }
                                        else
                                        {
                                            ch = (char)int.Parse(entity.Substring(1));
                                        }
                                        i = num3;
                                    }
                                    catch (FormatException)
                                    {
                                        i++;
                                    }
                                    catch (ArgumentException)
                                    {
                                        i++;
                                    }
                                }
                                else
                                {
                                    i = num3;
                                    char ch2 = HtmlEntities.Lookup(entity);
                                    if (ch2 != '\0')
                                    {
                                        ch = ch2;
                                    }
                                    else
                                    {
                                        output.Write('&');
                                        output.Write(entity);
                                        output.Write(';');
                                        goto Label_0103;
                                    }
                                }
                            }
                        }
                        output.Write(ch);
                    Label_0103: ;
                    }
                }
            }
        }
    }
#endif
}
