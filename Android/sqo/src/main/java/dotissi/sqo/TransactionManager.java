package dotissi.sqo;
import org.fusesource.lmdbjni.Cursor;
import org.fusesource.lmdbjni.Database;
import org.fusesource.lmdbjni.Entry;
import org.fusesource.lmdbjni.Env;
import org.fusesource.lmdbjni.GetOp;
import org.fusesource.lmdbjni.Transaction;

 class TransactionManager {
     private Env env;
     private SiaqodbTransaction activeTransaction;
     public TransactionManager(String path, long maxSize, int maxDbs) {
         this.env = new Env();
         env.setMapSize(maxSize);
         env.setMaxDbs(maxDbs);
         env.open(path);

     }

     public ITransaction beginTransaction() throws SiaqodbException {

         if(activeTransaction!=null)
         {
            throw new SiaqodbException("There is an active transaction, Commit or Abort it first");
         }
         Transaction trans = env.createTransaction();
         this.activeTransaction=new SiaqodbTransaction(trans,this);
         return activeTransaction;
     }
     public Transaction GetActiveTransaction()
     {
         return this.activeTransaction.innerTransaction;
     }
     public SiaqodbTransaction GetActiveTransaction(Status status)
     {
         if(activeTransaction==null) {
             this.beginTransaction();
             status.isStarted=true;
         }
         return activeTransaction;

     }
     public void resetActiveTransaction()
     {
         this.activeTransaction=null;
     }
     public void close()
     {
         env.close();
     }
     public Database openDatabase(Transaction tr,String dbname,int flags)
     {
         return env.openDatabase(tr,dbname,flags);

     }

 }
class Status
{
    public boolean isStarted;
}
