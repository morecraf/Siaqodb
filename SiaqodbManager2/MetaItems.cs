using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo;

namespace SiaqodbManager
{
    [System.Reflection.Obfuscation(Exclude = true)]
    public class ReferenceItem : SqoDataObject
    {
        public ReferenceItem()
        {

        }
        public ReferenceItem(string item)
        {
            this.Item = item;
        }
        [Sqo.Attributes.MaxLength(2000)]
        public string Item;
        public override string ToString()
        {
            return Item;
        }
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    public class NamespaceItem : Sqo.SqoDataObject
    {
        public NamespaceItem()
        {

        }
        public NamespaceItem(string item)
        {
            this.Item = item;
        }
        [Sqo.Attributes.MaxLength(2000)]
        public string Item;
        public override string ToString()
        {
            return Item;
        }
    }
    [System.Reflection.Obfuscation(Exclude = true)]
    public class ConnectionItem : Sqo.SqoDataObject
    {
        [Sqo.Attributes.MaxLength(2000)]
        public string Item;
        public ConnectionItem(string item)
        {
            this.Item = item;
        }
        public ConnectionItem()
        {

        }
        public override string ToString()
        {
            return Item;
        }

    }
}
