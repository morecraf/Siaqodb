// Copyright 2010 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Globalization;
using Sqo;
#if SERVER
using Microsoft.Synchronization.Services;
using System.Data;
#elif CLIENT
using Microsoft.Synchronization.ClientServices;
using Microsoft.Synchronization.Services.Formatters;
#endif

namespace Microsoft.Synchronization.Services.Formatters
{
    /// <summary>
    /// Class that will use .NET Reflection to serialize and deserialize an Entity to Atom/JSON
    /// </summary>
    class ReflectionUtility
    {
        static object _lockObject = new object();
        static Dictionary<string, IEnumerable<PropertyInfo>> _stringToPropInfoMapping = new Dictionary<string, IEnumerable<PropertyInfo>>();
        static Dictionary<string, IEnumerable<PropertyInfo>> _stringToPKPropInfoMapping = new Dictionary<string, IEnumerable<PropertyInfo>>();
        static Dictionary<string, ConstructorInfo> _stringToCtorInfoMapping = new Dictionary<string, ConstructorInfo>();

        public static IEnumerable<PropertyInfo> GetPropertyInfoMapping(Type type)
        {
            IEnumerable<PropertyInfo> props;

            if (!_stringToPropInfoMapping.TryGetValue(type.FullName, out props))
            {
                lock (_lockObject)
                {
                    if (!_stringToPropInfoMapping.TryGetValue(type.FullName, out props))
                    {
                        props = type.GetProperties();
                        props = props.Where(e =>
                            (!e.Name.Equals("ServiceMetadata", StringComparison.Ordinal) &&
                            e.GetMethod != null &&
                            e.SetMethod != null &&
                            e.DeclaringType == type)).ToArray();

                        _stringToPropInfoMapping[type.FullName] = props;


                        // Look for the fields marked with [Key()] Attribute
                        PropertyInfo[] keyFields = props.Where(e => e.GetCustomAttributes(typeof(KeyAttribute), true).Any()).ToArray();


                        if (keyFields.Length == 0)
                        {
                            throw new InvalidOperationException(string.Format("Entity {0} does not have the any property marked with the [DataAnnotations.KeyAttribute]. or [SQLite.PrimaryKeyAttribute]", type.Name));
                        }

                        _stringToPKPropInfoMapping[type.FullName] = keyFields;


                        // Look for the constructor info
                        ConstructorInfo ctorInfo = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(e => !e.GetParameters().Any());

                        if (ctorInfo == null)
                            throw new InvalidOperationException(string.Format("Type {0} does not have a public parameterless constructor.", type.FullName));

                        _stringToCtorInfoMapping[type.FullName] = ctorInfo;

                    }
                }
            }
            return props;
        }

        /// <summary>
        /// Get the PropertyInfo array for all Key fields
        /// </summary>
        /// <param name="type">Type to reflect on</param>
        /// <returns>PropertyInfo[]</returns>
        public static IEnumerable<PropertyInfo> GetPrimaryKeysPropertyInfoMapping(Type type)
        {
            IEnumerable<PropertyInfo> props;

            if (!_stringToPKPropInfoMapping.TryGetValue(type.FullName, out props))
            {
                GetPropertyInfoMapping(type);
                _stringToPKPropInfoMapping.TryGetValue(type.FullName, out props);
            }
            return props;
        }

        /// <summary>
        /// Build the OData Atom primary keystring representation
        /// </summary>
        /// <param name="live">Entity for which primary key is required</param>
        /// <returns>String representation of the primary key</returns>
        public static string GetPrimaryKeyString(IOfflineEntity live)
        {
            StringBuilder builder = new StringBuilder();

            string sep = string.Empty;
            foreach (PropertyInfo keyInfo in GetPrimaryKeysPropertyInfoMapping(live.GetType()))
            {
                if (keyInfo.PropertyType == FormatterConstants.GuidType)
                    builder.AppendFormat("{0}{1}=guid'{2}'", sep, keyInfo.Name, keyInfo.GetValue(live, null));
                else if (keyInfo.PropertyType == FormatterConstants.StringType)
                    builder.AppendFormat("{0}{1}='{2}'", sep, keyInfo.Name, keyInfo.GetValue(live, null));
                else
                    builder.AppendFormat("{0}{1}={2}", sep, keyInfo.Name, keyInfo.GetValue(live, null));

                if (string.IsNullOrEmpty(sep))
                    sep = ", ";

            }
            return builder.ToString();
        }

        public static IOfflineEntity GetObjectForType(EntryInfoWrapper wrapper, Type[] knownTypes)
        {
            Type entityType;

            ConstructorInfo ctorInfo;

            // See if its cached first.
            if (!_stringToCtorInfoMapping.TryGetValue(wrapper.TypeName, out ctorInfo))
            {
                // Its not cached. Try to look for it then in list of known types.
                if (knownTypes != null)
                {
                    entityType = knownTypes.FirstOrDefault(e => e.FullName.Equals(wrapper.TypeName, StringComparison.CurrentCultureIgnoreCase));

                    if (entityType == null)
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to find a matching type for entry '{0}' in list of KnownTypes.", wrapper.TypeName));

                }
                else
                {
                    throw new InvalidOperationException(string.Format("Unable to find a matching type for entry '{0}' in the loaded assemblies. Specify the type name in the KnownTypes argument to the SyncReader instance.", wrapper.TypeName));
                }

                // Reflect this entity and get necessary info
                GetPropertyInfoMapping(entityType);
                ctorInfo = _stringToCtorInfoMapping[wrapper.TypeName];
            }
            else
            {
                entityType = ctorInfo.DeclaringType;
            }

            // Invoke the ctor
            object obj = ctorInfo.Invoke(null);

            // Set the parameters only for non tombstone items
            if (!wrapper.IsTombstone)
            {
                IEnumerable<PropertyInfo> props = GetPropertyInfoMapping(entityType);
                foreach (PropertyInfo pinfo in props)
                {
                    string value;
                    if (wrapper.PropertyBag.TryGetValue(pinfo.Name, out value))
                        pinfo.SetValue(obj, GetValueFromType(pinfo.PropertyType, value), null);
                }
            }

            IOfflineEntity entity = (IOfflineEntity)obj;
            entity.ServiceMetadata = new OfflineEntityMetadata(wrapper.IsTombstone, wrapper.Id, wrapper.ETag, wrapper.EditUri);
            return entity;
        }


        private static object GetValueFromType(Type type, string value)
        {
            if (value == null)
            {
                if (type.IsGenericType())
                    return null;
                if (!type.IsPrimitive())
                    return null;

                throw new InvalidOperationException("Error in deserializing type " + type.FullName);
            }

            if (type.IsGenericType() && type.GetGenericTypeDefinition() == FormatterConstants.NullableType)
                type = type.GetGenericArguments()[0];

            if (FormatterConstants.StringType.IsAssignableFrom(type))
                return value;

            if (FormatterConstants.ByteArrayType.IsAssignableFrom(type))
                return Convert.FromBase64String(value);

            if (FormatterConstants.GuidType.IsAssignableFrom(type))
                return new Guid(value);

            if (FormatterConstants.DateTimeType.IsAssignableFrom(type) ||
                FormatterConstants.DateTimeOffsetType.IsAssignableFrom(type) ||
                FormatterConstants.TimeSpanType.IsAssignableFrom(type))
                return FormatterUtilities.ParseDateTimeFromString(value, type);

            if (type.IsPrimitive() ||
                FormatterConstants.DecimalType.IsAssignableFrom(type) ||
                FormatterConstants.FloatType.IsAssignableFrom(type))
                return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

            return value;
        }

    }
}
