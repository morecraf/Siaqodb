using System;


namespace CryptonorClient.DocumentSerializer
{
    public interface IDocumentSerializer
    {
        object Deserialize(Type type, byte[] objectBytes);
        byte[] Serialize(object obj);

    }
}
