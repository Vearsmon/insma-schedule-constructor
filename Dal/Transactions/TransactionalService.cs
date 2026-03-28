using System.Transactions;
using Domain.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dal.Transactions;

public class TransactionalService(
    ILogger<TransactionalService> logger,
    IOptions<TransactionOptions> transactionOptions) : ITransactionalService
{
    private readonly TransactionOptions _transactionOptions = transactionOptions.Value;

    public async Task ExecuteInTransactionAsync(Func<Task> action,
                                                TransactionTimeoutType transactionTimeoutType = TransactionTimeoutType.Default,
                                                TransactionScopeOption scopeOption = TransactionScopeOption.Required,
                                                IsolationLevel level = IsolationLevel.ReadCommitted)
    {
        try
        {
            var transactionOptions = new System.Transactions.TransactionOptions
            {
                IsolationLevel = level,
                Timeout = TimeSpan.FromSeconds(Math.Min(_transactionOptions.TransactionTimeoutType[transactionTimeoutType], _transactionOptions.TransactionTimeoutType[TransactionTimeoutType.Maximum])),
            };
            using var transactionScope = new TransactionScope(scopeOption, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);

            await action();

            transactionScope.Complete();
        }
        catch (Exception e)
        {
            logger.LogError(e.FlattenException());
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action,
                                                      TransactionTimeoutType transactionTimeoutType = TransactionTimeoutType.Default,
                                                      TransactionScopeOption scopeOption = TransactionScopeOption.Required,
                                                      IsolationLevel level = IsolationLevel.ReadCommitted)
    {
        try
        {
            var transactionOptions = new System.Transactions.TransactionOptions
            {
                IsolationLevel = level,
                Timeout = TimeSpan.FromSeconds(Math.Min(_transactionOptions.TransactionTimeoutType[transactionTimeoutType], _transactionOptions.TransactionTimeoutType[TransactionTimeoutType.Maximum])),
            };
            using var transactionScope = new TransactionScope(scopeOption, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);

            var result = await action();

            transactionScope.Complete();
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e.FlattenException());
            throw;
        }
    }
}
