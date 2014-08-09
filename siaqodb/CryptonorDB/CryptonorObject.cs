using Cryptonor.Exceptions;
using Sqo.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonor
{
    public class CryptonorObject
    {
        [Index]
        private string key;
        public int OID { get; set; }

        public bool ShouldSerializeOID()
        {
            return false;
        }
        public bool ShouldSerializeIsDirty()
        {
            return false;
        }
        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }
        private byte[] document;
        public byte[] Document
        {
            get { return document; }
            set { document = value; }
        }
        public string Version { get; set; }

        public bool IsDirty { get; set; }

        internal CryptonorObject(string key, byte[] document)
        {
            this.Key = key;
            this.Document = document;
        }
        public CryptonorObject()
        {
        }

        private Dictionary<string, long> tags_Int;

        private Dictionary<string, DateTime> tags_DateTime;

        private Dictionary<string, string> tags_String;

        private Dictionary<string, double> tags_Double;

        private Dictionary<string, bool> tags_Bool;

        public void SetTag(string tagName, object value)
        {
            Type type = value.GetType();
            if (type == typeof(int) || type == typeof(long))
            {
                if (tags_Int == null)
                    tags_Int = new Dictionary<string, long>();
                tags_Int.Add(tagName, Convert.ToInt64(value));
            }
            else if (type == typeof(DateTime))
            {
                if (tags_DateTime == null)
                    tags_DateTime = new Dictionary<string, DateTime>();
                tags_DateTime.Add(tagName, (DateTime)value);
            }

            else if (type == typeof(double) || type == typeof(float))
            {
                if (tags_Double == null)
                    tags_Double = new Dictionary<string, double>();
                tags_Double.Add(tagName, Convert.ToDouble(value));
            }
            else if (type == typeof(string))
            {
                if (tags_String == null)
                    tags_String = new Dictionary<string, string>();
                tags_String.Add(tagName, (string)value);
            }

            else if (type == typeof(bool))
            {
                if (tags_Bool == null)
                    tags_Bool = new Dictionary<string, bool>();
                tags_Bool.Add(tagName, (bool)value);
            }
            else
            {
                throw new CryptonorException("Tag type:" + type.ToString() + " not supported.");
            }

        }
        [Ignore]
        Dictionary<string, object> tags;
        public Dictionary<string, object> Tags
        {
            get
            {
                tags = this.GetAllTags();
                return tags;
            }
            set
            {
                tags = value;
                foreach (string key in value.Keys)
                {
                    this.SetTag(key, value[key]);
                }

            }
        }
        public T GetTag<T>(string tagName)
        {
            Type type = typeof(T);
            return (T)this.GetTag(tagName, type);
        }
        public object GetTag(string tagName, Type expectedType)
        {
            Type type = expectedType;
            if (type == typeof(int) || type == typeof(long))
            {
                if (tags_Int != null && tags_Int.ContainsKey(tagName))
                    return Convert.ChangeType(tags_Int[tagName], type);
            }
            else if (type == typeof(DateTime))
            {
                if (tags_DateTime != null && tags_DateTime.ContainsKey(tagName))
                    return Convert.ChangeType(tags_DateTime[tagName], type);
            }

            else if (type == typeof(double) || type == typeof(float))
            {
                if (tags_Double != null && tags_Double.ContainsKey(tagName))
                    return Convert.ChangeType(tags_Double[tagName], type);
            }
            else if (type == typeof(string))
            {
                if (tags_String != null && tags_String.ContainsKey(tagName))
                    return Convert.ChangeType(tags_String[tagName], type);
            }

            else if (type == typeof(bool))
            {
                if (tags_Bool != null && tags_Bool.ContainsKey(tagName))
                    return Convert.ChangeType(tags_Bool[tagName], type);
            }
            else
            {
                throw new CryptonorException("Tag type:" + type.ToString() + " not supported.");
            }
            return null;
        }
        internal Dictionary<string, object> GetAllTags()
        {
            Dictionary<string, object> tags = new Dictionary<string, object>();
            CopyDictionary(tags, this.tags_Int);
            CopyDictionary(tags, this.tags_String);
            CopyDictionary(tags, this.tags_DateTime);
            CopyDictionary(tags, this.tags_Double);
            CopyDictionary(tags, this.tags_Bool);
            if (tags.Count == 0)
                return null;
            return tags;
        }
        private void CopyDictionary(Dictionary<string, object> tags, IDictionary dict_to_copy)
        {
            if (dict_to_copy != null)
            {

                foreach (string key in dict_to_copy.Keys)
                {
                    tags.Add(key, dict_to_copy[key]);
                }

            }
        }

    }
}
