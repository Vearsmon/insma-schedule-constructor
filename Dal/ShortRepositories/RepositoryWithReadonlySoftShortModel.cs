using Dal.Entities;
using Dal.Helpers;
using Dal.Repositories;
using Dal.Transactions;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Dal.ShortRepositories;

public class
    RepositoryWithReadonlySoftShortModel<TDbContext, TDbEntity, TModel, TShortModel>
    : RepositoryWithReadonlyShortModel<TDbContext, TDbEntity, TModel, TShortModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbSoftDeleteEntity, IDbEntityWithId, new()
    where TModel : TShortModel, ISoftDeleteModel, IModelWithId
    where TShortModel : ISoftDeleteModel, IModelWithId
{
    private readonly TDbContext _dbContext;

    protected RepositoryWithReadonlySoftShortModel(TDbContext context,
        IRepositoryMapper<TDbEntity, TModel> mapper,
        IReadonlyRepositoryMapper<TDbEntity, TShortModel> shortMapper,
        ITransactionalService transactionService) : base(context, mapper, shortMapper, transactionService)
    {
        _dbContext = context;
    }

    public override async Task DeleteAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        await _dbContext.SoftDeleteAsync<TDbContext, TDbEntity>(ids, cancellationToken);
    }
}