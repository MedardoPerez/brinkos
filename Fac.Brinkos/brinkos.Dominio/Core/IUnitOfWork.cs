namespace brinkos.Dominio.Core
{
    public interface IUnitOfWork
    {
        void Commit(TransactionInfo transactionInfo);
    }
}