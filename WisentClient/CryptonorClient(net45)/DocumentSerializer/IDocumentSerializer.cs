using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.DocumentSerializer
{
    public interface IDocumentSerializer
    {
        object Deserialize(Type type, byte[] objectBytes);
        byte[] Serialize(object obj);

    }
}
