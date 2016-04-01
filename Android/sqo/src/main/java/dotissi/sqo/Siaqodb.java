package dotissi.sqo;


import java.util.HashMap;

public class Siaqodb {

    static final int oneMega = 1024 * 1024;
    private TransactionManager transactionManager;
    public Siaqodb()
    {}
    public Siaqodb(String path)
    {}
    public Siaqodb(String path,long maxDbFileSize)
    {}
    public Siaqodb(String path,long maxDbFileSize,int maxIndexes)
    {


    }
    public void Open(String path)
    {
        this.Open(path,50*oneMega,50);
    }
    public void Open(String path, long maxDbFileSize, int maxIndexes)
    {
        this.transactionManager=new TransactionManager(path,maxDbFileSize,maxIndexes);
    }
    HashMap<String, Bucket> cache = new HashMap<String, Bucket>();

    public IBucket getBucket(String bucketName) {


        if (!cache.containsKey(bucketName)) {
            cache.put(bucketName, new Bucket(bucketName,this.transactionManager));
        }
        return cache.get(bucketName);

    }
    public void dropBucket(String bucketName) {

        Bucket buk = (Bucket) this.getBucket(bucketName);
        buk.drop();
        if (cache.containsKey(bucketName)) {
            cache.remove(bucketName);
        }

    }
    public ITransaction beginTransaction() {

        return transactionManager.beginTransaction();
    }
    public void close()
    {
        transactionManager.close();
    }

}
