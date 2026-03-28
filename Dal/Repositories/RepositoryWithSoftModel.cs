using Dal.Entities;
using Dal.Helpers;
using Dal.Transactions;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class RepositoryWithSoftModel<TDbContext, TDbEntity, TModel>
    : Repository<TDbContext, TDbEntity, TModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbSoftDeleteEntity, IDbEntityWithId, new()
    where TModel : IModelWithId
{
    private readonly TDbContext _dbContext;

    protected RepositoryWithSoftModel(TDbContext context,
        IRepositoryMapper<TDbEntity, TModel> mapper,
        ITransactionalService transactionService) : base(context, mapper, transactionService)
    {
        _dbContext = context;
    }

    public override async Task DeleteAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        await _dbContext.SoftDeleteAsync<TDbContext, TDbEntity>(ids, cancellationToken);
    }
}
