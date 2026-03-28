using Dal.Entities;
using Dal.Transactions;
using Domain.Exceptions;
using Domain.Models.Common;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public abstract class RepositoryWithVersion<TDbContext, TDbEntity, TModel>(
    TDbContext context,
    IRepositoryMapper<TDbEntity, TModel> mapper,
    ITransactionalService transactionService)
    : Repository<TDbContext, TDbEntity, TModel>(context, mapper, transactionService),
        IRepositoryWithVersion<TModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbEntityWithVersion, IDbEntityWithId, new()
    where TModel : IWithVersion, IModelWithId
{
    public async Task<TModel> GetByRootAsync(Guid rootId)
    {
        var entity = await Query().FirstOrDefaultAsync(x => x.RootId == rootId && x.IsActualVersion);

        return entity == null
            ? throw new ServiceException(ServiceExceptionTypes.EntityNotFound)
            : MapperReadonly.Map(entity);
    }

    public async Task<TModel[]> GetByRootsAsync(Guid[] rootIds)
    {
        var entities = await Query().Where(x => rootIds.Contains(x.RootId) && x.IsActualVersion)
                                    .ToArrayAsync();

        return entities.Length == 0
            ? throw new ServiceException(ServiceExceptionTypes.EntityNotFound)
            : entities.Select(x => MapperReadonly.Map(x)).ToArray();
    }
}
