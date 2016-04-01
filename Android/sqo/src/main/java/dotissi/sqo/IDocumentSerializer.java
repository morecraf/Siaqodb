package dotissi.sqo;

public interface IDocumentSerializer {
    Object deserialize(Class<? extends Object> type, byte[] objectBytes);
    byte[] serialize(Object obj);
}
