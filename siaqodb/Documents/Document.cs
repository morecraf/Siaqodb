using Sqo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sqo.Documents
{
    [System.Reflection.Obfuscation(Exclude = true)]
    public class Document
    {
        public Document()
        {
        }
        internal Document(string key, byte[] content)
        {
            this.Key = key;
            this.content = content;
        }
       
        internal bool ShouldSerializeIsDirty()
        {
            return false;
        }
        private string key;

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                key = value;
            }
        }
        private byte[] content;

        public byte[] Content
        {
            get { return content; }
            set { content = value; }
        }
        public string Version { get; set; }
        public void SetContent<T>(T objValue)
        {
            this.SetContent((object)objValue);
        }
        public void SetContent(object objValue)
        {

            if (objValue == null)
                throw new ArgumentNullException("objValue");

            if (this.Key == null && SiaqodbConfigurator.KeyConventions.ContainsKey(objValue.GetType()))
            {
                this.Key = SiaqodbConfigurator.KeyConventions[objValue.GetType()](objValue);
            }
            if (this.Version == null && SiaqodbConfigurator.VersionSetConventions.ContainsKey(objValue.GetType()))
            {
                this.Version = SiaqodbConfigurator.VersionSetConventions[objValue.GetType()](objValue);
            }
            byte[] serializedObj = SiaqodbConfigurator.DocumentSerializer.Serialize(objValue);
            this.Content = serializedObj;
            
        }
        public object GetContent( Type type)
        {
            if (this.Content == null || this.Content.Length == 0)
                return null;
        
            object obj = SiaqodbConfigurator.DocumentSerializer.Deserialize(type, this.Content);
            if (SiaqodbConfigurator.VersionGetConventions.ContainsKey(type))
            {
                SiaqodbConfigurator.VersionGetConventions[type](obj, this.Version);
            }
            return obj;
        }
        public T GetContent<T>()
        {
            return (T)((object)this.GetContent(typeof(T)));
        }




        public void SetTag<T>(string tagName, T value)
        {
            this.SetTag(tagName, (object)value);

        }
        public void SetTag(string tagName, object value)
        {
            tagName = tagName.ToLower();
            Type type = value.GetType();
            if (!ValidTagName(tagName))
            {
                throw new SiaqodbException("Tag name:" + tagName + " is not valid.");
            }
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
                throw new SiaqodbException("Tag type:" + type.ToString() + " not supported.");
            }
        }

        Dictionary<string, object> tags;
        public Dictionary<string, object> Tags
        {
            get
            {
                return tags;
            }
            set
            {
                tags = value;

            }
        }

        internal bool ValidTagName(string tagName)
        {

            if (!Regex.IsMatch(tagName, "^[a-zA-Z][0-9a-zA-Z_-]*$"))
            {
                return false;
            }
            return true;
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
                        return (T)Convert.ChangeType(Tags[tagName], typeof(T));
                    }
                    else
                    {
                        return (T)Tags[tagName];
                    }
                }
            }
            return default(T);
        }


    }
}
