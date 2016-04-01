package dotissi.sqo;


public interface ITransaction {

    void commit();
    void rollback();
}
