using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using System.Reflection;
using Sqo.Exceptions;
using Sqo.Core;
using Sqo.Encryption;

namespace Sqo
{
    /// <summary>
    /// Class responsible for configurations of Siaqodb database engine
    /// </summary>
    public static class SiaqodbConfigurator
    {
        internal static Dictionary<Type, List<string>> Indexes;
        internal static Dictionary<Type, List<string>> Constraints;
        internal static Dictionary<Type, List<string>> Ignored;
        internal static Dictionary<Type, List<string>> LazyLoadedFields;
        internal static Dictionary<Type, List<string>> Texts;
        internal static Dictionary<Type, List<string>> Documents;

        internal static Dictionary<Type, Dictionary<string,int>> MaxLengths;
        internal static Dictionary<Type, Dictionary<string, string>> PropertyMaps;
        internal static Dictionary<Type, bool> LazyLoaded;
        
        internal static bool RaiseLoadEvents;
        internal static DateTimeKind? DateTimeKindToSerialize;
        internal static bool OptimisticConcurrencyEnabled=true;
        internal static Configurator defaultConfigurator;
        internal static KeyValuePair<int,DateTime> CurrentVersion = new KeyValuePair<int,DateTime>(50,new DateTime(2015,7,2));
       
        /// <summary>
        /// Add an index for a field or automatic property of a certain Type,an Index can be added also by using Attribute: Sqo.Attributes.Index;
        /// both ways of adding index are similar
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="type">Type that declare the field</param>
        public static void AddIndex(string field, Type type)
        {

            if (Indexes == null)
            {
                Indexes = new Dictionary<Type, List<string>>();
                
            }
            AddSettingToField(field, type, Indexes);

        }
        /// <summary>
        /// Add an UniqueConstraint for a field of a certain Type,an UniqueConstraint can be added also by using Attribute: Sqo.Attributes.UniqueConstraint;
        /// both ways of adding UniqueConstraint are similar
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="type">Type that declare the field</param>
        public static void AddUniqueConstraint(string field, Type type)
        {

            if (Constraints == null)
            {
                Constraints = new Dictionary<Type, List<string>>();
               
            }
            if (!Constraints.ContainsKey(type))
            {
                Constraints.Add(type, new List<string>());
            }
            AddSettingToField(field, type, Constraints);
        }
        /// <summary>
        /// Mark a field as LazyLoad, LazyLoad field can be marked also by using Attribute: Sqo.Attributes.LazyLoad
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="type">Type that declare the field</param>
        public static void AddLazyLoadField(string field, Type type)
        {

            if (LazyLoadedFields == null)
            {
                LazyLoadedFields = new Dictionary<Type, List<string>>();
            }
            AddSettingToField(field, type, LazyLoadedFields);

        }
        private static void AddSettingToField(string field, Type type, Dictionary<Type, List<string>>  settings)
        {

            if (!settings.ContainsKey(type))
            {
                settings.Add(type, new List<string>());
            }
            List<FieldInfo> fi = new List<FieldInfo>();
            Dictionary<FieldInfo, PropertyInfo> automaticProperties = new Dictionary<FieldInfo, PropertyInfo>();
            MetaExtractor.FindFields(fi, automaticProperties, type);
            bool found = false;
            foreach (FieldInfo f in fi)
            {
                if (f.Name == field)
                {
                    found = true;
                    settings[type].Add(f.Name);

                    break;
                }
                else if (automaticProperties.ContainsKey(f))
                {
                    if (field == automaticProperties[f].Name)
                    {
                        found = true;
                        settings[type].Add(f.Name);
                        break;
                    }
                }
            }
            if (!found)
            {
                throw new SiaqodbException("Field:" + field + " not found as field or as automatic property of Type provided");
            }
        }
        /// <summary>
        /// Put MaxLength for a string field or automatic property of a Type, MaxLength can be set also by using Attribute: Sqo.Attributes.MaxLength
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="maxLength">max length for a string</param>
        /// <param name="type">Type that declare the field</param>
        public static void AddMaxLength(string field,int maxLength, Type type)
        {
            if (MaxLengths == null)
            {
                MaxLengths = new Dictionary<Type, Dictionary<string, int>>();
                MaxLengths[type] = new Dictionary<string, int>();
            }
            if (!MaxLengths.ContainsKey(type))
            {
                MaxLengths.Add(type, new Dictionary<string,int>());
            }
            List<FieldInfo> fi = new List<FieldInfo>();
            Dictionary<FieldInfo, PropertyInfo> automaticProperties = new Dictionary<FieldInfo, PropertyInfo>();
            MetaExtractor.FindFields(fi, automaticProperties, type);
            bool found = false;
            foreach (FieldInfo f in fi)
            {
                if (f.Name == field)
                {
                    if (f.FieldType == typeof(string))
                    {
                        MaxLengths[type].Add(f.Name, maxLength);
                        found = true;
                        break;
                    }
                }
                else if (automaticProperties.ContainsKey(f))
                {
                    if (field == automaticProperties[f].Name)
                    {
                        if (f.FieldType == typeof(string))
                        {
                            found = true;
                            MaxLengths[type].Add(f.Name, maxLength);
                            break;
                        }
                    }
                }
            }
            if (!found)
            {
                throw new SiaqodbException("Field:" + field + " not found as field or as automatic property of Type provided");
            }

        }
        /// <summary>
        /// Ignore a field or automatic property to be stored
        /// </summary>
        /// <param name="field">Name of field or automatic property</param>
        /// <param name="type">Type that declare the field</param>
        public static void AddIgnore(string field, Type type)
        {
            if (Ignored == null)
            {
                Ignored = new Dictionary<Type, List<string>>();
            }
            AddSettingToField(field, type, Ignored);
        
        }
        /// <summary>
        /// Mark field to be stored as a string with unlimited length 
        /// </summary>
        /// <param name="field">Name of field or automatic property</param>
        /// <param name="type">Type that declare the field</param>
        public static void AddText(string field, Type type)
        {
            if (Texts == null)
            {
                Texts = new Dictionary<Type, List<string>>();

            }
            AddSettingToField(field, type, Texts);
        }
        /// <summary>
        /// Mark a field or automatic property of a certain Type to be serialized as a Document ,it can be added also by using Attribute: Sqo.Attributes.Document;
        /// both ways of set as Document are similar
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="type">Type that declare the field</param>
        public static void AddDocument(string field, Type type)
        {

            if (Documents == null)
            {
                Documents = new Dictionary<Type, List<string>>();

            }
            AddSettingToField(field, type, Documents);

        }
        /// <summary>
        /// Set the name of backing field for a property in case engine cannto discover it, this also can be set by attribute: Sqo.Attributes.UseVariable
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="fieldName">Name of backing field of property</param>
        /// <param name="type"></param>
        public static void PropertyUseField(string propertyName, string fieldName,Type type)
        {
            if (PropertyMaps == null)
            {
                PropertyMaps = new Dictionary<Type, Dictionary<string,string>>();
                PropertyMaps[type] = new Dictionary<string, string>();
            }
            if (!PropertyMaps.ContainsKey(type))
            {
                PropertyMaps.Add(type, new Dictionary<string,string>());
            }
            PropertyMaps[type][propertyName] = fieldName;

        }
        private static bool encryptedDatabase=false;
        /// <summary>
        /// Set if database will be encrypted or not
        /// </summary>
        public static bool EncryptedDatabase
        {
            get { return encryptedDatabase; }
            set { 
                encryptedDatabase = value;
                if (encryptedDatabase)
                { 
                    Encryptor = new AESEncryptor();
                }
            }
        }
        /// <summary>
        /// Set the password for encryption algorithm used to encrypt database data
        /// </summary>
        /// <param name="pwd">The password</param>
        public static void SetEncryptionPassword(string pwd)
        {
            int keyLength=32;//used by XTEA algorithm

            byte[] key = ByteConverter.SerializeValueType(pwd, typeof(string),keyLength,keyLength,0 );
            XTEAEncryptor enc = Encryptor as XTEAEncryptor;
            if (enc != null)
            {
                enc.SetKey(key);
            }
            AESEncryptor encAes = Encryptor as AESEncryptor;
            if (encAes != null)
            {
                encAes.SetKey(key);
            }

        }
        internal static IEncryptor Encryptor;
        /// <summary>
        /// Set your custom encryption algorithm that implemets IEncryptor interface
        /// </summary>
        /// <param name="encryptor">The instance of custom encryption algorithm</param>
        public static void SetEncryptor(IEncryptor encryptor)
        {
            if (encryptor == null)
            {
                throw new ArgumentNullException("encryptor");
            }
            Encryptor = encryptor;
        }
        /// <summary>
        /// Set build-in encryption algorithm 
        /// </summary>
        /// <param name="alg">Encryption algorithm</param>
        public static void SetEncryptor(BuildInAlgorithm alg)
        {
            if (alg == BuildInAlgorithm.AES)
            {
                Encryptor = new AESEncryptor();
            }
            else if (alg == BuildInAlgorithm.XTEA)
            {
                Encryptor = new XTEAEncryptor();
            }
        }
        
#if SILVERLIGHT

        static bool useLargeBuffers = true;
        /// <summary>
        /// Set this to true in case that is needed to cache more data,when there is big amount of data to import/insert and use Flush() when inserts are finished.
        /// Set this to false in case that is needed more reads in the app and when inserts occur that inserts are not in loops, also in OOB mode if you store data on My...folders'
        /// By default the value set is TRUE
        /// </summary>
        public static bool UseLargeBuffers { get { return useLargeBuffers; } set { useLargeBuffers = value; } }
        static bool useLongDBFileNames=false;
        /// <summary>
        /// Set this to true in case that is needed database files names to be same like on .NET version (composed by Namespace+Assemblyname);
        /// By default the values is set to FALSE and database file names will look like 3324434.sqo
        /// </summary>
        public static bool UseLongDBFileNames { get { return useLongDBFileNames; } set { useLongDBFileNames = value; } }
#endif

        /// <summary>
        /// Set custom fileName on disk of database file for Type T
        /// </summary>
        /// <typeparam name="T">Type of objects</typeparam>
        /// <param name="fileName">Name of database file on disk</param>
       
        public static void SetDatabaseFileName<T>(string fileName)
        {
            SetDatabaseFileName(typeof(T), fileName);
        }
        /// <summary>
        /// Set custom fileName on disk of database file for Type 
        /// </summary>
        /// <param name="type">Type of objects</typeparam>
        /// <param name="fileName">Name of database file on disk</param>
        public static void SetDatabaseFileName(Type type,string fileName)
        {

            SqoTypeInfo ti = MetaExtractor.GetSqoTypeInfo(type);
            Cache.CacheCustomFileNames.AddFileNameForType(ti.TypeName, fileName);

        }
        public static void SetDatabaseFileNameWithoutException(Type type, string fileName)
        {

            SqoTypeInfo ti = MetaExtractor.GetSqoTypeInfo(type);
            Cache.CacheCustomFileNames.AddFileNameForType(ti.TypeName, fileName, false);

        }
        /// <summary>
        /// By default this is true for all types. Set this to false to not load childs entities of objects of Type provided
        /// </summary>
        /// <typeparam name="T">Type for objects</typeparam>
        /// <param name="loadRelatedObjects">true if related object need to be loaded, false if you want to load by Include(...) method</param>
        public static void LoadRelatedObjects<T>(bool loadRelatedObjects)
        {
            LoadRelatedObjects(typeof(T), loadRelatedObjects);

        }
        /// <summary>
        /// By default this is true for all types. Set this to false to not load childs entities of objects of Type provided
        /// </summary>
        /// <param name="type">Type for objects</param>
        /// <param name="loadRelatedObjects">true if related object need to be loaded, false if you want to load by Include(...) method</param>
        public static void LoadRelatedObjects(Type type, bool loadRelatedObjects)
        {

            if (LazyLoaded == null)
            {
                LazyLoaded = new Dictionary<Type, bool>();
            }
            LazyLoaded[type] = !loadRelatedObjects;
           

        }
        /// <summary>
        /// Set the license key
        /// </summary>
        /// <param name="licenseKey">License key</param>
        public static void SetLicense(string licenseKey)
        {
            Sqo.Utilities.SqoLicense.LicenseValid(licenseKey);
        }

        /// <summary>
        /// Set true to raise Loading/Loaded events
        /// </summary>
        /// <param name="raiseLoadEvents"></param>
        public static void SetRaiseLoadEvents(bool raiseLoadEvents)
        {
            RaiseLoadEvents = raiseLoadEvents;
        }
        public static void SpecifyStoredDateTimeKind(DateTimeKind? kind)
        {
            DateTimeKindToSerialize = kind;
        }
        public static void EnableOptimisticConcurrency(bool enabled)
        {
            OptimisticConcurrencyEnabled = enabled;
        }
        public static TraceListener LoggingMethod
        {
            get;
            set;
        }
        public static VerboseLevel VerboseLevel { get; set; }

        internal static void LogMessage(string message, VerboseLevel level)
        {
            if (VerboseLevel >= level && LoggingMethod!=null)
            {
                LoggingMethod(message, level);
            }
        }
       
        internal static IDocumentSerializer DocumentSerializer;
        /// <summary>
        /// Set your custom document serializer
        /// </summary>
        /// <param name="documentSerializer">The instance of custom document serializer</param>
        public static void SetDocumentSerializer(IDocumentSerializer documentSerializer)
        {
            if (documentSerializer == null)
            {
                throw new ArgumentNullException("documentSerializer");
            }
            DocumentSerializer = documentSerializer;
        }
        /// <summary>
        /// Apply default configurations
        /// </summary>
        /// <param name="config">Configurator instance</param>
        public static void ApplyConfigurator(Configurator config)
        {
            if (config == null)
                throw new ArgumentNullException();
            ReInit();
            defaultConfigurator = config;
            defaultConfigurator.LoadRelatedObjectsPropetyChanged -= defaultConfigurator_LoadRelatedObjectsPropetyChanged;
            defaultConfigurator.LoadRelatedObjectsPropetyChanged += defaultConfigurator_LoadRelatedObjectsPropetyChanged;
            if (config.Indexes != null)
            {
                foreach (var item in config.Indexes.Keys)
                {
                    foreach (string field in config.Indexes[item])
                    {
                        AddIndex(field, item);
                    }
                }
            }
            if (config.Constraints != null)
            {
                foreach (var item in config.Constraints.Keys)
                {
                    foreach (string field in config.Constraints[item])
                    {
                        AddUniqueConstraint(field, item);
                    }
                }
            }
            if (config.MaxLengths != null)
            {
                foreach (var item in config.MaxLengths.Keys)
                {
                    foreach (string field in config.MaxLengths[item].Keys)
                    {
                        AddMaxLength(field,config.MaxLengths[item][field], item);
                    }
                }
            }
            if (config.Ignored != null)
            {
                foreach (var item in config.Ignored.Keys)
                {
                    foreach (string field in config.Ignored[item])
                    {
                        AddIgnore(field, item);
                    }
                }
            }
            if (config.Texts != null)
            {
                foreach (var item in config.Texts.Keys)
                {
                    foreach (string field in config.Texts[item])
                    {
                        AddText(field, item);
                    }
                }
            }
            if (config.Documents != null)
            {
                foreach (var item in config.Documents.Keys)
                {
                    foreach (string field in config.Documents[item])
                    {
                        AddDocument(field, item);
                    }
                }
            }
            if (config.LazyLoadedFields != null)
            {
                foreach (var item in config.LazyLoadedFields.Keys)
                {
                    foreach (string field in config.LazyLoadedFields[item])
                    {
                        AddLazyLoadField(field, item);
                    }
                }
            }
            if (config.PropertyMaps != null)
            {
                foreach (var item in config.PropertyMaps.Keys)
                {
                    foreach (string field in config.PropertyMaps[item].Keys)
                    {
                        PropertyUseField(field,config.PropertyMaps[item][field], item);
                    }
                }
            }
            EncryptedDatabase = config.EncryptedDatabase;
            if (config.Encryptor != null)
            {
                SetEncryptor(config.Encryptor);
            }
            else if (config.encAlgorithm != BuildInAlgorithm.NONE)
            {
                SetEncryptor(config.encAlgorithm);
            }
            if (config.encryptionPWD != null)
            {
                SetEncryptionPassword(config.encryptionPWD);
            }
            if (config.DatabaseFileNames != null)
            {
                foreach (var item in config.DatabaseFileNames.Keys)
                {
                    SetDatabaseFileNameWithoutException(item, config.DatabaseFileNames[item]);
                }
            }
            if (config.LazyLoaded != null)
            {
                foreach (var item in config.LazyLoaded.Keys)
                {
                    LoadRelatedObjects(item, !config.LazyLoaded[item]);
                }
            }
            if (!string.IsNullOrEmpty(config.LicenseKey))
            {
                SetLicense(config.LicenseKey);
            }
            if (config.DocumentSerializer != null)
            {
                SetDocumentSerializer(config.DocumentSerializer);
            }
           
            SetRaiseLoadEvents(config.RaiseLoadEvents);
            SpecifyStoredDateTimeKind(config.DateTimeKindToSerialize);
            EnableOptimisticConcurrency(config.OptimisticConcurrencyEnabled);
            LoggingMethod = config.LoggingMethod;
            VerboseLevel = config.VerboseLevel;
            
        }

        static void defaultConfigurator_LoadRelatedObjectsPropetyChanged(object sender, EventArgs e)
        {
            LazyLoaded = null;
            if (defaultConfigurator.LazyLoaded != null)
            {
                foreach (var item in defaultConfigurator.LazyLoaded.Keys)
                {
                    LoadRelatedObjects(item, !defaultConfigurator.LazyLoaded[item]);
                }
            }
        }
        private static void ReInit()
        {
            Indexes = null;
            Constraints = null;
            MaxLengths = null;
            Ignored = null;
            Texts = null;
            PropertyMaps = null;
            LazyLoaded = null;
            LazyLoadedFields = null;
        }
        private static decimal bufferingChunkPercent = 10;

        internal static decimal BufferingChunkPercent
        {
            get { return bufferingChunkPercent; }
            set
            {
                if (value > 100 || value <= 0)
                {
                    throw new SiaqodbException("Max percent must be 100");
                }
                bufferingChunkPercent = value;
            }
        }
    }
    
}
