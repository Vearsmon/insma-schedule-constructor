using System.Transactions;

namespace Dal.Transactions;

public interface ITransactionalService
{
    public Task ExecuteInTransactionAsync(Func<Task> action,
                                          TransactionTimeoutType transactionTimeoutType = TransactionTimeoutType.Default,
                                          TransactionScopeOption scopeOption = TransactionScopeOption.Required,
                                          IsolationLevel level = IsolationLevel.ReadCommitted);

    public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action,
                                                TransactionTimeoutType transactionTimeoutType = TransactionTimeoutType.Default,
                                                TransactionScopeOption scopeOption = TransactionScopeOption.Required,
                                                IsolationLevel level = IsolationLevel.ReadCommitted);
}
