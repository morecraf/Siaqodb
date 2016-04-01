package dotissi.sqo;

/**
 * Created by Cristi on 3/17/2016.
 */
class SiaqodbTransaction implements ITransaction {
    org.fusesource.lmdbjni.Transaction innerTransaction;
    TransactionManager manager;
    public SiaqodbTransaction(org.fusesource.lmdbjni.Transaction innerTransaction,TransactionManager manager)
    {
        this.innerTransaction=innerTransaction;
        this.manager=manager;
    }
    @Override
    public void commit() {
        innerTransaction.commit();
        manager.resetActiveTransaction();
    }

    @Override
    public void rollback() {
        innerTransaction.abort();
        manager.resetActiveTransaction();
    }
}
