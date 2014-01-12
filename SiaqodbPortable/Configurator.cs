using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Sqo.Encryption;



namespace Sqo
{
    public class Configurator
    {
        internal  Dictionary<Type, List<string>> Indexes;
        internal  Dictionary<Type, List<string>> Constraints;
        internal  Dictionary<Type, List<string>> Ignored;
        internal  Dictionary<Type, Dictionary<string, int>> MaxLengths;
        internal  Dictionary<Type, Dictionary<string, string>> PropertyMaps;
        internal  Dictionary<Type, List<string>> Texts;
        internal  Dictionary<Type, bool> LazyLoaded;
        internal  Dictionary<Type, string> DatabaseFileNames;
        internal Dictionary<Type, List<string>> Documents;
        internal  bool RaiseLoadEvents;
        internal  DateTimeKind? DateTimeKindToSerialize;
        internal  bool OptimisticConcurrencyEnabled = true;
        internal bool encryptedDatabase = false;
        internal string encryptionPWD;
        internal BuildInAlgorithm encAlgorithm;
        internal event EventHandler LoadRelatedObjectsPropetyChanged;
        internal string LicenseKey;
        /// <summary>
        /// Add an index for a field or automatic property of a certain Type,an Index can be added also by using Attribute: Sqo.Attributes.Index;
        /// both ways of adding index are similar
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="type">Type that declare the field</param>
        public void AddIndex(string field, Type type)
        {

            if (Indexes == null)
            {
                Indexes = new Dictionary<Type, List<string>>();

            }
            if (!Indexes.ContainsKey(type))
            {
                Indexes.Add(type, new List<string>());
            }
            Indexes[type].Add(field);
        }
        /// <summary>
        /// Add an UniqueConstraint for a field of a certain Type,an UniqueConstraint can be added also by using Attribute: Sqo.Attributes.UniqueConstraint;
        /// both ways of adding UniqueConstraint are similar
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="type">Type that declare the field</param>
        public void AddUniqueConstraint(string field, Type type)
        {

            if (Constraints == null)
            {
                Constraints = new Dictionary<Type, List<string>>();

            }
            if (!Constraints.ContainsKey(type))
            {
                Constraints.Add(type, new List<string>());
            }
            Constraints[type].Add(field);
        }
        /// <summary>
        /// Put MaxLength for a string field or automatic property of a Type, MaxLength can be set also by using Attribute: Sqo.Attributes.MaxLength
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="maxLength">max length for a string</param>
        /// <param name="type">Type that declare the field</param>
        public void AddMaxLength(string field, int maxLength, Type type)
        {
            if (MaxLengths == null)
            {
                MaxLengths = new Dictionary<Type, Dictionary<string, int>>();
                MaxLengths[type] = new Dictionary<string, int>();
            }
            if (!MaxLengths.ContainsKey(type))
            {
                MaxLengths.Add(type, new Dictionary<string, int>());
            }
            MaxLengths[type].Add(field,maxLength);
        }
        /// <summary>
        /// Ignore a field or automatic property to be stored
        /// </summary>
        /// <param name="field">Name of field or automatic property</param>
        /// <param name="type">Type that declare the field</param>
        public void AddIgnore(string field, Type type)
        {
            if (Ignored == null)
            {
                Ignored = new Dictionary<Type, List<string>>();
                Ignored[type] = new List<string>();
            }
            if (!Ignored.ContainsKey(type))
            {
                Ignored.Add(type, new List<string>());
            }
            Ignored[type].Add(field);
        }
        /// <summary>
        /// Mark field to be stored as a string with unlimited length 
        /// </summary>
        /// <param name="field">Name of field or automatic property</param>
        /// <param name="type">Type that declare the field</param>
        public void AddText(string field, Type type)
        {
            if (Texts == null)
            {
                Texts = new Dictionary<Type, List<string>>();

            }
            if (!Texts.ContainsKey(type))
            {
                Texts.Add(type, new List<string>());
            }
            Texts[type].Add(field);
        }
        /// <summary>
        /// Mark a field or automatic property of a certain Type to be serialized as a Document ,it can be added also by using Attribute: Sqo.Attributes.Document;
        /// both ways of set as Document are similar
        /// </summary>
        /// <param name="field">Field name or automatic property name</param>
        /// <param name="type">Type that declare the field</param>
        public void AddDocument(string field, Type type)
        {

            if (Documents == null)
            {
                Documents = new Dictionary<Type, List<string>>();

            }
            if (!Documents.ContainsKey(type))
            {
                Documents.Add(type, new List<string>());
            }
            Documents[type].Add(field);
        }
        /// <summary>
        /// Set the name of backing field for a property in case engine cannto discover it, this also can be set by attribute: Sqo.Attributes.UseVariable
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="fieldName">Name of backing field of property</param>
        /// <param name="type"></param>
        public void PropertyUseField(string propertyName, string fieldName, Type type)
        {
            if (PropertyMaps == null)
            {
                PropertyMaps = new Dictionary<Type, Dictionary<string, string>>();
                PropertyMaps[type] = new Dictionary<string, string>();
            }
            if (!PropertyMaps.ContainsKey(type))
            {
                PropertyMaps.Add(type, new Dictionary<string, string>());
            }
            PropertyMaps[type][propertyName] = fieldName;

        }
      
        /// <summary>
        /// Set if database will be encrypted or not
        /// </summary>
        public bool EncryptedDatabase
        {
            get { return encryptedDatabase; }
            set
            {
                encryptedDatabase = value;
            }
        }
        /// <summary>
        /// Set the password for encryption algorithm used to encrypt database data
        /// </summary>
        /// <param name="pwd">The password</param>
        public void SetEncryptionPassword(string pwd)
        {
            this.encryptionPWD = pwd;

        }
        internal IEncryptor Encryptor;
        /// <summary>
        /// Set your custom encryption algorithm that implemets IEncryptor interface
        /// </summary>
        /// <param name="encryptor">The instance of custom encryption algorithm</param>
        public void SetEncryptor(IEncryptor encryptor)
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
        public  void SetEncryptor(BuildInAlgorithm alg)
        {
            this.encAlgorithm = alg;
        }

        /// <summary>
        /// Set custom fileName on disk of database file for Type T
        /// </summary>
        /// <typeparam name="T">Type of objects</typeparam>
        /// <param name="fileName">Name of database file on disk</param>
        public void SetDatabaseFileName<T>(string fileName)
        {
            SetDatabaseFileName(typeof(T), fileName);
        }
          /// <summary>
        /// Set custom fileName on disk of database file for Type 
        /// </summary>
        /// <param name="type">Type of objects</param>
        /// <param name="fileName">Name of database file on disk</param>
        public void SetDatabaseFileName(Type type, string fileName)
        {

            if (DatabaseFileNames == null)
            {
                DatabaseFileNames = new Dictionary<Type, string>();

            }
            if (!DatabaseFileNames.ContainsKey(type))
            {
                DatabaseFileNames.Add(type, fileName);
            }
        
        }
        public bool BuildIndexesAsync
        {
            get;
            set;
        }
        /// <summary>
        /// By default this is true for all types. Set this to false to not load childs entities of objects of Type provided
        /// </summary>
        /// <typeparam name="T">Type for objects</typeparam>
        /// <param name="loadRelatedObjects">true if related object need to be loaded, false if you want to load by Include(...) method</param>
        public void LoadRelatedObjects<T>(bool loadRelatedObjects)
        {
            LoadRelatedObjects(typeof(T), loadRelatedObjects);
        }
        /// <summary>
        /// By default this is true for all types. Set this to false to not load childs entities of objects of Type provided
        /// </summary>
        /// <param name="type">Type for objects</param>
        /// <param name="loadRelatedObjects">true if related object need to be loaded, false if you want to load by Include(...) method</param>
        public void LoadRelatedObjects(Type type, bool loadRelatedObjects)
        {
           
            if (LazyLoaded == null)
            {
                LazyLoaded = new Dictionary<Type, bool>();
            }
            LazyLoaded[type] = !loadRelatedObjects;
            this.OnLoadRelatedObjectsPropetyChanged(EventArgs.Empty);

        }
        /// <summary>
        /// Set the license key
        /// </summary>
        /// <param name="licenseKey">License key</param>
        public void SetLicense(string licenseKey)
        {
            this.LicenseKey = licenseKey;
        }

        /// <summary>
        /// Set true to raise Loading/Loaded events
        /// </summary>
        /// <param name="raiseLoadEvents"></param>
        public void SetRaiseLoadEvents(bool raiseLoadEvents)
        {
            RaiseLoadEvents = raiseLoadEvents;
        }
        public void SpecifyStoredDateTimeKind(DateTimeKind? kind)
        {
            DateTimeKindToSerialize = kind;
        }
        public void EnableOptimisticConcurrency(bool enabled)
        {
            OptimisticConcurrencyEnabled = enabled;
        }
        public TraceListener LoggingMethod
        {
            get;
            set;
        }
        public VerboseLevel VerboseLevel { get; set; }

        public decimal BufferingChunkPercent
        {
            get;
            set;
        }
        protected void OnLoadRelatedObjectsPropetyChanged(EventArgs e)
        {
            if (this.LoadRelatedObjectsPropetyChanged != null)
            {
                this.LoadRelatedObjectsPropetyChanged(this,e);
            }
        }
        internal IDocumentSerializer DocumentSerializer;
        /// <summary>
        /// Set your custom document serializer
        /// </summary>
        /// <param name="documentSerializer">The instance of custom document serializer</param>
        public void SetDocumentSerializer(IDocumentSerializer documentSerializer)
        {
            if (documentSerializer == null)
            {
                throw new ArgumentNullException("documentSerializer");
            }
            DocumentSerializer = documentSerializer;
        }
    }
    public enum BuildInAlgorithm {NONE, AES, XTEA }
    public delegate void TraceListener(string traceMessage, VerboseLevel level);
    public enum VerboseLevel { Off = 1, Error = 2, Warn = 3, Info = 4 }
    
}
