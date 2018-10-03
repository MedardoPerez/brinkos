using System.Transactions;

namespace repositorios.service.Core.Identity
{
    public static class TransactionScopeFactory
    {
        public static TransactionScope GetTransactionScope()
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };

            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}