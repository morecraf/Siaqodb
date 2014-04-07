using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo
{
    public interface IDocumentSerializer
    {
        object Deserialize(Type type, byte[] objectBytes);
        byte[] Serialize(object obj);

    }
}
