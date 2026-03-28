using Dal.Entities;
using Dal.Repositories;
using Dal.Transactions;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Dal.ShortRepositories;

public abstract class RepositoryWithShortModel<TDbContext, TDbEntity, TModel, TShortModel>(
    TDbContext context,
    IRepositoryMapper<TDbEntity, TModel> mapper,
    IRepositoryMapper<TDbEntity, TShortModel> shortMapper,
    ITransactionalService transactionService)
    : RepositoryWithReadonlyShortModel<TDbContext, TDbEntity, TModel, TShortModel>(context, mapper, shortMapper,
            transactionService),
        IRepositoryWithShortModel<TModel, TShortModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbEntityWithId, new()
    where TModel : TShortModel, IModelWithId
    where TShortModel : IModelWithId
{
    protected readonly IRepositoryMapper<TDbEntity, TShortModel> ShortMapper = shortMapper;

    public async Task<Guid> SaveShortAsync(TShortModel model, CancellationToken cancellationToken = default)
    {
        var ids = await SaveAllInternalAsync([model], ShortMapper, cancellationToken);
        return ids[0];
    }

    public async Task<Guid[]> SaveShortAllAsync(TShortModel[] models, CancellationToken cancellationToken = default)
    {
        return await SaveAllInternalAsync(models, ShortMapper, cancellationToken);
    }
}
