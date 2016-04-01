package dotissi.sqo;

import org.fusesource.lmdbjni.Cursor;
import org.fusesource.lmdbjni.Database;
import org.fusesource.lmdbjni.Entry;
import org.fusesource.lmdbjni.GetOp;
import org.fusesource.lmdbjni.Transaction;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.HashMap;
import java.util.Set;

public class Bucket implements IBucket{
    private String bucketName;
    private TransactionManager transactionManager;
    private TagsIndexManager indexManag;
    String indexInfoDB;
    String dirtyEntitiesDB;
    String anchorDB;

    public Bucket(String bucketName,TransactionManager transactionManager) {
        this.bucketName = "buk_" + bucketName;
        this.transactionManager=transactionManager;
        indexInfoDB = this.bucketName + "_sys_indexinfo";
        dirtyEntitiesDB = this.bucketName + "_sys_dirtydb";
        this.anchorDB = this.bucketName + "_sys_anchordb";

        indexManag = new TagsIndexManager(indexInfoDB, this.transactionManager);
    }
    @Override
    public Document load(String key) {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
            Document object = load(key, lmdbTransaction);
            lmdbTransaction.commit();
            return object;
        }catch (Exception ex) {
            if (status.isStarted)
                transaction.rollback();

            throw ex;
        }
        finally
        {
            if (status.isStarted)
                transaction.commit();
        }
    }
    private Document load(String key, Transaction transaction)
    {
        Database db = null;
        try{
            db = transactionManager.openDatabase(transaction,bucketName,0x40000);
            byte[] keyBytes = ByteConverter.StringToByteArray(key);
            byte[] crObjBytes = db.get(transaction, keyBytes);
            if (crObjBytes != null)
            {
                IDocumentSerializer serializer = SiaqodbConfigurator.getSerializer();
                return (Document) serializer.deserialize(Document.class, crObjBytes);
            }

            return null;
        }finally{
            if(db!=null){
                db.close();
            }
        }
    }
    @Override
    public ArrayList<Document> find(Query query) {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try
        {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
            ArrayList<Document> allFiltered = new ArrayList<Document>();
            ArrayList<String> keysLoaded = this.getKeys(lmdbTransaction, query);

            for (String key:keysLoaded)
            {
                Document obj = load(key, lmdbTransaction);
                allFiltered.add(obj);
            }
            return allFiltered;
        }
        finally
        {
            if (status.isStarted)
                transaction.commit();
        }
    }

    private ArrayList<String> getKeys(Transaction lmdbTransaction, Query query) {
        QueryRunner qr = new QueryRunner(indexManag, lmdbTransaction, this.bucketName, this.transactionManager);
        ArrayList<String> keys = qr.run(query);
        ArrayList<String> keysLoaded = keys;
        if (query.skip != null)
            keysLoaded = this.skip(keysLoaded,query.skip);
        if (query.limit != null)
            keysLoaded = this.take(keysLoaded, query.limit);
        return keysLoaded;
    }

    private ArrayList<String> take(ArrayList<String> keysLoaded, Integer limit) {
        if(limit<0)
            return keysLoaded;
        for(int i=limit;i<keysLoaded.size();i++)
            keysLoaded.remove(i);
        return keysLoaded;
    }

    private ArrayList<String> skip(ArrayList<String> keysLoaded, Integer skip) {
        if(skip<0)
            return keysLoaded;
        for(int i=0;i<skip;i++)
            keysLoaded.remove(i);
        return keysLoaded;
    }

    @Override
    public Document findFirst(Query query) {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try
        {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
            ArrayList<Document> allFiltered = new ArrayList<Document>();
            ArrayList<String> keysLoaded = this.getKeys(lmdbTransaction, query);
            if(keysLoaded.size()>=1)
                return load(keysLoaded.get(0),lmdbTransaction);

        }
        finally
        {
            if (status.isStarted)
                transaction.commit();
        }
        return null;
    }

    @Override
    public int count(Query query) {
        Status status = new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
            ArrayList<Document> allFiltered = new ArrayList<Document>();
            ArrayList<String> keysLoaded = this.getKeys(lmdbTransaction, query);
            return keysLoaded.size();
        } finally {
            if (status.isStarted)
                transaction.commit();
        }

    }

    @Override
    public int count() {
       Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
            Database db = transactionManager.openDatabase(lmdbTransaction, bucketName, 0x40000);
            Cursor cursor = db.openCursor(lmdbTransaction);
            Entry current = cursor.get(GetOp.FIRST);
            int i=0;
            while (current != null && current.getValue() != null) {

                current = cursor.get(GetOp.NEXT);
                i++;
            }
            return i;

        }
        finally
        {
            if (status.isStarted)
                transaction.commit();
        }
    }

    @Override
    public ArrayList<Document> loadAll() {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();

            Database db = transactionManager.openDatabase(lmdbTransaction, bucketName, 0x40000);

            IDocumentSerializer serializer = SiaqodbConfigurator.getSerializer();
            ArrayList<Document> list = new ArrayList<Document>();

            Cursor cursor = db.openCursor(lmdbTransaction);
            Entry current = cursor.get(GetOp.FIRST);

            while (current != null && current.getValue() != null) {
                byte[] crObjBytes = current.getValue();
                if (crObjBytes != null) {
                    Object obj = serializer.deserialize(Document.class, crObjBytes);
                    list.add((Document) obj);
                }
                current = cursor.get(GetOp.NEXT);

            }

            return list;
        }finally {
            if (status.isStarted)
                transaction.commit();
        }
    }

    @Override
    public ArrayList<Document> loadAll(int skip, int limit) {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();

            Database db = transactionManager.openDatabase(lmdbTransaction, bucketName, 0x40000);

            IDocumentSerializer serializer = SiaqodbConfigurator.getSerializer();
            ArrayList<Document> list = new ArrayList<Document>();

            Cursor cursor = db.openCursor(lmdbTransaction);
            Entry current = cursor.get(GetOp.FIRST);

            int i = 0;
            while (current != null && current.getValue() != null) {
                if (i < skip) {
                    current = cursor.get(GetOp.NEXT);
                    continue;
                }
                byte[] crObjBytes = current.getValue();
                if (crObjBytes != null) {
                    Object obj = serializer.deserialize(Document.class, crObjBytes);
                    list.add((Document) obj);
                }
                if (list.size() == limit)
                    break;
                current = cursor.get(GetOp.NEXT);
                i++;

            }

            return list;
        }finally {
            if (status.isStarted)
                transaction.commit();
        }
    }

    @Override
    public void store(Document doc) {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {

            this.store(doc,transaction);
        }
        finally {

            if(status.isStarted)
                transaction.commit();
        }
    }

    @Override
    public void store(Document doc, ITransaction transaction) {
        this.store(doc, transaction, true);
    }
    void store(Document doc, ITransaction transaction,boolean isDirty) {
        DirtyOperation dop;
        Transaction lmdbTransaction = transactionManager.GetActiveTransaction();

        Database db = transactionManager.openDatabase(lmdbTransaction, bucketName, 0x40000);
        byte[] keyBytes = ByteConverter.StringToByteArray(doc.getKey());
        HashMap<String, Object> oldTags = indexManag.PrepareUpdateIndexes(keyBytes, lmdbTransaction, db);
        dop = oldTags != null ? DirtyOperation.Updated : DirtyOperation.Inserted;

        IDocumentSerializer serializer = SiaqodbConfigurator.getSerializer();
        byte[] crObjBytes = serializer.serialize(doc);
        db.put(lmdbTransaction, keyBytes, crObjBytes);
        indexManag.UpdateIndexes(doc.getKey(), oldTags, doc.getTags(), lmdbTransaction, bucketName);
        if (SiaqodbConfigurator.IsBucketSyncable(this.bucketName) && isDirty)
        {
            CreateDirtyEntity(dop, lmdbTransaction, doc);
        }


    }
    private void CreateDirtyEntity(DirtyOperation dop, Transaction transaction, Document obj)
    {
        CreateDirtyEntity(dop, transaction, obj.getKey(), obj.getVersion());
    }
    private void CreateDirtyEntity(DirtyOperation dirtyOperation, Transaction transaction, String key, String version)
    {
        DirtyEntity dirtyEntity = new DirtyEntity();
        dirtyEntity.DirtyOp = dirtyOperation;
        dirtyEntity.Key = key;
        dirtyEntity.OperationTime = Calendar.getInstance().getTime();
        dirtyEntity.Version = version;

        Database db = transactionManager.openDatabase(transaction, dirtyEntitiesDB, 0x40000 | 0x04);

        IDocumentSerializer serializer = SiaqodbConfigurator.getSerializer();
        byte[] dirtyEntityBytes = serializer.serialize(dirtyEntity);
        byte[] keyBytes = ByteConverter.StringToByteArray(key);
        db.put(transaction, keyBytes, dirtyEntityBytes);


    }

    @Override
    public void store(String key, Object obj) {
        this.store(key, obj, null);
    }

    @Override
    public void store(String key, Object obj, HashMap<String, Object> tags) {
        Document doc = new Document();
        doc.setKey(key);
        doc.setContent(obj);

        if (tags != null)
        {
            for (String tagName : tags.keySet()) {

                doc.setTag(tagName, tags.get(tagName));
            }
        }
        store(doc);
    }
    @Override
    public void storeBatch(ArrayList<Document> docs) {
        this.storeBatch(docs, true);
    }

    private void storeBatch(ArrayList<Document> docs,boolean isDirty) {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {

            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();

            Database db = transactionManager.openDatabase(lmdbTransaction, bucketName, 0x40000);

            for (Document doc : docs) {
                byte[] keyBytes = ByteConverter.StringToByteArray(doc.getKey());
                HashMap<String, Object> oldTags = indexManag.PrepareUpdateIndexes(keyBytes, lmdbTransaction, db);
                DirtyOperation dop = oldTags != null ? DirtyOperation.Updated : DirtyOperation.Inserted;

                IDocumentSerializer serializer = SiaqodbConfigurator.getSerializer();
                byte[] crObjBytes = serializer.serialize(doc);
                db.put(lmdbTransaction,keyBytes,crObjBytes);
                indexManag.UpdateIndexes(doc.getKey(), oldTags, doc.getTags(), lmdbTransaction, this.bucketName);
                if (SiaqodbConfigurator.IsBucketSyncable(this.bucketName) && isDirty) {
                    CreateDirtyEntity(dop, lmdbTransaction, doc);
                }
            }
        }
        finally {
            if(status.isStarted)
                transaction.commit();
        }
    }

    @Override
    public void delete(String key) {
        Document doc = this.load(key);
        if (doc != null)
        {
            delete(doc);
        }
    }

    @Override
    public void delete(Document doc) {
        Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {

            delete(doc,transaction,true);
        }
        finally {

            if(status.isStarted)
                transaction.commit();
        }
    }

    @Override
    public void delete(Document doc, ITransaction transaction) {

        this.delete(doc,transaction,true);
    }
    private void delete(Document doc, ITransaction transaction, boolean isDirty)
    {
        Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
        Database db = transactionManager.openDatabase(lmdbTransaction, bucketName, 0x40000);

        byte[] keyBytes = ByteConverter.StringToByteArray(doc.getKey());
        HashMap<String,Object> oldTags = indexManag.PrepareUpdateIndexes(keyBytes, lmdbTransaction, db);
        db.delete(lmdbTransaction, keyBytes);

        if (SiaqodbConfigurator.IsBucketSyncable(this.bucketName) && isDirty)
        {
            CreateDirtyEntity(DirtyOperation.Deleted, lmdbTransaction, doc.getKey(), doc.getVersion());
        }
        indexManag.UpdateIndexesAfterDelete(doc.getKey(), oldTags, lmdbTransaction, this.bucketName);

    }

    @Override
    public String getBucketName() {
        return this.bucketName;
    }

    public void drop()
    {
        Status status = new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);
        try {
            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
            DropIndexes(lmdbTransaction);

            Database db = transactionManager.openDatabase(lmdbTransaction, bucketName, 0x40000);
            db.drop(lmdbTransaction, true);

            Database dbAnc = transactionManager.openDatabase(lmdbTransaction,anchorDB , 0x40000);
            dbAnc.drop(lmdbTransaction, true);

            Database dbDirty = transactionManager.openDatabase(lmdbTransaction, dirtyEntitiesDB, 0x40000 | 0x04);
            dbDirty.drop(lmdbTransaction, true);

            Database dbIndexInfo = transactionManager.openDatabase(lmdbTransaction, indexInfoDB, 0x40000);
            dbIndexInfo.drop(lmdbTransaction, true);


            lmdbTransaction.commit();


        } catch (Exception ex) {
            transaction.rollback();
            throw ex;
        } finally {

            if (status.isStarted)
                transaction.commit();
        }

    }

    private void DropIndexes(Transaction lmdbTransaction) {
        Set<String> indexes = indexManag.GetIndexes();
        for (String index:indexes)
        {
            Database dbIndex = transactionManager.openDatabase(lmdbTransaction, index, 0x40000 | 0x04);
            dbIndex.drop(lmdbTransaction, true);
        }
    }
}
