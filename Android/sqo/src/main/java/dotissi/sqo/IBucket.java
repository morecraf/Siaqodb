package dotissi.sqo;

import java.util.ArrayList;
import java.util.HashMap;

public interface IBucket
{
    Document load(String key);
    ArrayList<Document> find(Query query);
    Document findFirst(Query query);
    int count(Query query);
    int count();
    ArrayList<Document> loadAll();
    ArrayList<Document> loadAll(int skip, int limit);
    void store(Document doc);
    void store(Document doc, ITransaction transaction);
    void store(String key, Object obj);
    void store(String key, Object obj, HashMap<String, Object> tags);
    void storeBatch(ArrayList<Document> docs);
    void delete(String key);
    void delete(Document doc);
    void delete(Document doc,ITransaction transaction);
    String getBucketName();

}