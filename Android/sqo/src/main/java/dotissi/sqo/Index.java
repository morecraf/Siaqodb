package dotissi.sqo;


import org.fusesource.lmdbjni.Cursor;
import org.fusesource.lmdbjni.Database;
import org.fusesource.lmdbjni.Entry;
import org.fusesource.lmdbjni.GetOp;
import org.fusesource.lmdbjni.SeekOp;
import org.fusesource.lmdbjni.Transaction;

import java.io.IOException;
import java.util.ArrayList;

public class Index implements IIndex {
    String indexName;
    Transaction transaction;
    Database db;
    public Index(String indexName, Transaction transaction,TransactionManager transactionManager)
    {
        this.indexName=indexName;
        this.transaction=transaction;
        db = transactionManager.openDatabase( this.transaction,indexName, 0x40000 | 0x04);

    }
    @Override
    public ArrayList<String> FindItem(Object key) {
        byte[] keyBytes = ByteConverter.GetBytes(key, key.getClass());


        ArrayList<String> duplicates = new ArrayList<String>();
        Cursor cursor = db.openCursor(transaction);
        Entry firstKV = cursor.seek(SeekOp.RANGE, keyBytes);
        if (firstKV!=null&&firstKV.getValue()!=null)
        {
            Object currentKey =  ByteConverter.ReadBytes(firstKV.getKey(), key.getClass());

            if (Util.Compare(currentKey, key) == 0)
            {
                ReadDuplicates(firstKV, duplicates, cursor);
            }
        }
        cursor.close();
       return  duplicates;
    }

    @Override
    public ArrayList<String> FindAllExcept(Object key) {

        Cursor cursor = db.openCursor(transaction);

        Entry firstKV = cursor.get(GetOp.FIRST);
        ArrayList<String> indexValues = new ArrayList<String>();

        while (firstKV != null) {
            Object currentKey =  ByteConverter.ReadBytes(firstKV.getKey(), key.getClass());

            int compareResult = Util.Compare(currentKey, key);
            if (compareResult != 0) {
                ReadDuplicates(firstKV, indexValues, cursor);
            }

            firstKV = cursor.get(GetOp.NEXT);

        }
        cursor.close();
        return indexValues;
    }

    @Override
    public ArrayList<String> FindItemsBetween(Object start, Object end) {
        return this.FindItemsBetweenStartEnd(start, end, true, true);
    }

    @Override
    public ArrayList<String> FindItemsBetweenExceptStart(Object start, Object end) {
        return this.FindItemsBetweenStartEnd(start, end, false, true);
    }

    @Override
    public ArrayList<String> FindItemsBetweenExceptEnd(Object start, Object end) {
        return this.FindItemsBetweenStartEnd(start, end, true, false);
    }

    @Override
    public ArrayList<String> FindItemsBetweenExceptStartEnd(Object start, Object end) {
        return this.FindItemsBetweenStartEnd(start, end, false, false);
    }
    private ArrayList<String> FindItemsBetweenStartEnd(Object start, Object end,boolean alsoEqualStart,boolean alsoEqualEnd) {
        byte[] keyBytes = ByteConverter.GetBytes(start, start.getClass());

        Cursor cursor = db.openCursor(transaction);
        Entry firstKV = cursor.seek(SeekOp.RANGE, keyBytes);
        ArrayList<String> indexValues = new ArrayList<String>();

        if (firstKV!=null) {
            Object currentKey = ByteConverter.ReadBytes(firstKV.getKey(), start.getClass());

            int compareResult = Util.Compare(currentKey, start);
            if (compareResult > 0 || (alsoEqualStart && compareResult == 0)) {
                ReadDuplicates(firstKV, indexValues, cursor);
            }
        }
        while (firstKV!=null) {
            firstKV = cursor.get(GetOp.NEXT);
            if (firstKV != null) {
                Object currentKey =  ByteConverter.ReadBytes(firstKV.getKey(), start.getClass());

                int compareResult = Util.Compare(currentKey, end);
                if (compareResult < 0 || (alsoEqualEnd && compareResult == 0)) {
                    ReadDuplicates(firstKV, indexValues, cursor);
                }
                if (compareResult >= 0)
                    break;
            }

        }

        return indexValues;

    }

    @Override
    public ArrayList<String> FindItemsBiggerThan(Object start) {
        return this.FindItemsBigger(start, false);
    }

    @Override
    public ArrayList<String> FindItemsBiggerThanOrEqual(Object start) {
        return this.FindItemsBigger(start, true);
    }
    private ArrayList<String> FindItemsBigger(Object start,boolean alsoEqual)
    {
        byte[] keyBytes = ByteConverter.GetBytes(start, start.getClass());

        Cursor cursor = db.openCursor(transaction);

        Entry firstKV = cursor.seek(SeekOp.RANGE, keyBytes);
        ArrayList<String> indexValues = new ArrayList<String>();
        while (firstKV!=null) {
            Object currentKey =  ByteConverter.ReadBytes(firstKV.getKey(), start.getClass());

            int compareResult = Util.Compare(currentKey, start);
            if (compareResult  > 0 || (alsoEqual && compareResult==0)) {
                ReadDuplicates(firstKV, indexValues, cursor);
            }
            firstKV = cursor.get(GetOp.NEXT);

        }
        return indexValues;
    }

    @Override
    public ArrayList<String> FindItemsLessThan(Object start) {
        return this.FindItemsLess(start, false);
    }

    @Override
    public ArrayList<String> FindItemsLessThanOrEqual(Object start) {
        return this.FindItemsLess(start, true);
    }
    private ArrayList<String> FindItemsLess(Object start, boolean alsoEqual) {
        byte[] keyBytes =  ByteConverter.GetBytes(start, start.getClass());

        Cursor cursor = db.openCursor(transaction);

        Entry firstKV = cursor.get(GetOp.FIRST);
        ArrayList<String> indexValues = new ArrayList<String>();
        while (firstKV!=null) {
            Object currentKey = ByteConverter.ReadBytes(firstKV.getKey(), start.getClass());

            int compareResult = Util.Compare(currentKey, start);
            if (compareResult < 0 || (alsoEqual && compareResult == 0)) {
                ReadDuplicates(firstKV, indexValues, cursor);
            }
            if (compareResult >= 0)
                break;

            firstKV = cursor.get(GetOp.NEXT);
        }
        return indexValues;

    }
    @Override
    public ArrayList<String> FindItemsStartsWith(Object start) {

        String startStr=(String)start;
        byte[] keyBytes =  ByteConverter.GetBytes(start, start.getClass());

        Cursor cursor = db.openCursor(transaction);

        Entry firstKV = cursor.seek(SeekOp.RANGE, keyBytes);
        ArrayList<String> indexValues = new ArrayList<String>();
        while (firstKV != null) {
            String currentKey = ByteConverter.ByteArrayToString(firstKV.getKey());
            int compareResult = Util.Compare(currentKey, start);
            if (compareResult >= 0) {
                if (currentKey.startsWith( startStr)) {
                    ReadDuplicates(firstKV, indexValues, cursor);
                } else
                    return indexValues;
            }
            firstKV = cursor.get(GetOp.NEXT);
        }
        return indexValues;
    }

    @Override
    public ArrayList<String> FindItemsContains(Object start) {
        Cursor cursor = db.openCursor(transaction);
        String startStr=(String)start;
        Entry firstKV = cursor.get(GetOp.FIRST);
        ArrayList<String> indexValues = new ArrayList<String>();
        while (firstKV != null) {
            String currentKey = ByteConverter.ByteArrayToString(firstKV.getKey());

            if (currentKey.contains(startStr))
            {
                ReadDuplicates(firstKV, indexValues, cursor);
            }
            firstKV = cursor.get(GetOp.NEXT);
        }
        return indexValues;
    }

    @Override
    public ArrayList<String> FindItemsEndsWith(Object start) {
        Cursor cursor = db.openCursor(transaction);
        String startStr=(String)start;
        Entry firstKV = cursor.get(GetOp.FIRST);
        ArrayList<String> indexValues = new ArrayList<String>();
        while (firstKV != null) {
            String currentKey = ByteConverter.ByteArrayToString(firstKV.getKey());

            if (currentKey.endsWith(startStr))
            {
                ReadDuplicates(firstKV, indexValues, cursor);
            }
            firstKV = cursor.get(GetOp.NEXT);
        }
        return indexValues;
    }

    @Override
    public void close() throws IOException {
       // db.close();
    }


    public void DeleteItem(Object key, String value) {
        byte[] keyBytes = ByteConverter.GetBytes(key, key.getClass());
        byte[] valueBytes = ByteConverter.GetBytes(value, value.getClass());

        db.delete( transaction,keyBytes, valueBytes);
    }
    public void AddItem(Object key, String value) {
        byte[] keyBytes = ByteConverter.GetBytes(key,key.getClass());

        db.put(transaction, keyBytes, ByteConverter.GetBytes(value,value.getClass()));
    }
    private void ReadDuplicates(Entry firstKV, ArrayList<String> duplicates,Cursor cursor) {
        Object currentVal = null;

        currentVal = ByteConverter.ReadBytes(firstKV.getValue(), String.class);

        duplicates.add((String) currentVal);
        Entry currentDuplicate = cursor.get(GetOp.NEXT_DUP);
        while (currentDuplicate != null && currentDuplicate.getValue() != null) {
            Object currentValDup = null;
            currentValDup = ByteConverter.ReadBytes(currentDuplicate.getValue(), String.class);

            duplicates.add((String) currentValDup);
            currentDuplicate = cursor.get(GetOp.NEXT_DUP);
        }
    }

}