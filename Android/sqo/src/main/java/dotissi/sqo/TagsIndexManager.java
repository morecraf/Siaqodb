package dotissi.sqo;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Set;

import org.fusesource.lmdbjni.Cursor;
import org.fusesource.lmdbjni.Entry;
import org.fusesource.lmdbjni.GetOp;
import org.fusesource.lmdbjni.Transaction;
import org.fusesource.lmdbjni.Database;

class TagsIndexManager
{
    HashMap<String, String> indexMetaInfo = new HashMap<String, String>();
    String indexInfoDBName;
    TransactionManager transactionManager;
    public TagsIndexManager(String indexInfoDBName, TransactionManager transactionManager)
    {
        //if (Sqo.Utilities.SqoLicense.isStarterEdition)
       // {
      //      throw new Exceptions.InvalidLicenseException("You cannot store documents with Starter Edition.");
        //}
        this.indexInfoDBName = indexInfoDBName;
        this.transactionManager=transactionManager;
       Status status=new Status();
        SiaqodbTransaction transaction = transactionManager.GetActiveTransaction(status);

        try
        {

            Transaction lmdbTransaction = transactionManager.GetActiveTransaction();
            Database db =   transactionManager.openDatabase(lmdbTransaction, indexInfoDBName, 0x40000);

            Cursor cursor = db.openCursor(lmdbTransaction);
            Entry current = cursor.get(GetOp.FIRST);

            while (current != null && current.getKey() != null) {
                byte[] keyBytes = current.getValue();
                String currentKey = ByteConverter.ByteArrayToString(keyBytes);
                indexMetaInfo.put(currentKey,currentKey);

                current = cursor.get(GetOp.NEXT);

            }

        }
        finally {
            if (status.isStarted)
            {
                transaction.commit();
            }
        }


    }
    public HashMap<String, Object> PrepareUpdateIndexes(byte[] keyBytes, Transaction transaction, Database db)
    {
        byte[] crObjBytes = db.get(transaction,keyBytes);
        if (crObjBytes != null)
        {
            IDocumentSerializer serializer = SiaqodbConfigurator.getSerializer();
            Document obj = (Document)serializer.deserialize(Document.class, crObjBytes);
            return obj!=null?obj.getTags():new HashMap<String,Object>();
        }
        return null;
    }
    public void UpdateIndexes(String crKey, HashMap<String, Object> oldTags, HashMap<String, Object> newTags, Transaction transaction, String bucketName)
    {
        Index index = null;
        if (oldTags != null && oldTags.size() > 0)
        {
            for (String key : oldTags.keySet())
            {

                if (newTags != null && newTags.containsKey(key))
                {
                    Object oldVal = oldTags.get(key);
                    if (newTags.get(key).getClass() != oldVal.getClass())
                    {
                        oldVal = Util.ChangeType(oldTags.get(key), newTags.get(key).getClass());
                    }
                    if (!newTags.get(key).equals(oldVal))
                    {
                        index = this.GetIndex(bucketName,key,transaction);
                        index.DeleteItem(oldTags.get(key), crKey);
                        index.AddItem(newTags.get(key),crKey);
                    }
                }
                else//tag is removed
                {
                    index = this.GetIndex(bucketName, key, transaction);
                    index.DeleteItem(oldTags.get(key), crKey);
                }
            }
            if (newTags != null)
            {
                for (String key : newTags.keySet())
                {
                    if (!oldTags.containsKey(key))
                    {
                        index = this.GetIndex(bucketName, key, transaction);
                        index.AddItem(newTags.get(key), crKey);
                    }
                }
            }

        }
        else//add
        {
            if (newTags != null)
            {
                for (String key : newTags.keySet())
                {
                    index = this.GetIndex(bucketName, key, transaction);
                    index.AddItem(newTags.get(key), crKey);
                }
            }
        }
        if (index != null)
        {
            //index.Dispose();
        }
    }
    public void UpdateIndexesAfterDelete(String crKey, HashMap<String, Object> oldTags, Transaction transaction, String bucketName)
    {
        if (oldTags != null && oldTags.size() > 0)
        {
            for (String key : oldTags.keySet())
            {
                Index index = this.GetIndex(bucketName, key, transaction);
                index.DeleteItem(oldTags.get(key), crKey);
            }
        }
    }
    private Index GetIndex(String bucket, String tagName, Transaction transaction)
    {
        String indexName = bucket + "_tags_" + tagName;
        if (!indexMetaInfo.containsKey(indexName))
        {
            this.StoreIndexInfo(indexName, transaction);
            indexMetaInfo.put(indexName,indexName);
        }
        return new Index(indexName, transaction,transactionManager);
    }

    private void StoreIndexInfo(String indexName, Transaction transaction)
    {
        Database db =   transactionManager.openDatabase(transaction, indexInfoDBName, 0x40000);

        byte[] keyBytes = ByteConverter.StringToByteArray(indexName);
        db.put(transaction, keyBytes, keyBytes);

    }

    ArrayList<String> LoadKeysByIndex(Where query, String bucketName, Transaction transaction) {

        Index index = this.GetIndex(bucketName, query.TagName, transaction);
        return IndexQueryFinder.FindKeys(index, query);

    }
    Set<String> GetIndexes() {
        return indexMetaInfo.keySet();
    }
}