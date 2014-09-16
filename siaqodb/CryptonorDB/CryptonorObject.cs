using Cryptonor.Exceptions;
using Sqo.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif

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
        [UseVariable("key")]
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
         [UseVariable("document")]
      
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
       

        public void SetTag<T>(string tagName, T value)
        {
            this.SetTag(tagName, (object)value);

        }
        public void SetTag(string tagName, object value)
        {
            tagName = tagName.ToLower();
            Type type = value.GetType();
            if (Tags == null)
                Tags = new Dictionary<string, object>();
            if (type == typeof(int) || type == typeof(long))
            {
                Tags[tagName] = Convert.ToInt64(value);
            }
            else if (type == typeof(double) || type == typeof(float))
            {
                Tags[tagName] = Convert.ToDouble(value);
            }
            else if (type == typeof(DateTime) || type == typeof(string) || type == typeof(bool))
            {

                Tags[tagName] = value;
            }

            else
            {
                throw new CryptonorException("Tag type:" + type.ToString() + " not supported.");
            }
        }
        private byte[] tagsSerialized;
        [Ignore]
        Dictionary<string, object> tags;
        public Dictionary<string, object> Tags
        {
            get
            {
                if (tags == null && tagsSerialized != null)
                {
                    DeserializeTags();
                }
                return tags;
            }
            set
            {
                tags = value;

            }
        }

        public T GetTag<T>(string tagName)
        {
            if (Tags != null)
            {
                tagName = tagName.ToLower();
                if (Tags.ContainsKey(tagName))
                {
                    if (Tags[tagName].GetType() != typeof(T))
                    {
                        return (T)Sqo.Utilities.Convertor.ChangeType(Tags[tagName], typeof(T));
                    }
                    else
                    {
                        return (T)Tags[tagName];
                    }
                }
            }
           return default(T);
        }

        internal void SerializeTags()
        {
            if (tags != null)
            {
                tagsSerialized = TagsSerializer.GetBytes(tags);
            }
        }
        internal void DeserializeTags()
        {
            if (tagsSerialized != null)
            {
                tags = TagsSerializer.GetDictionary(tagsSerialized);
            }
        }
#if SILVERLIGHT
        public object GetValue(FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
#endif
    }
}
