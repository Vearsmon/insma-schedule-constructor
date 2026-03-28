using Dal.Entities;
using Dal.Repositories;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Dal.ShortRepositories;

public abstract class ReadonlyRepositoryWithShortModel<TDbContext, TDbEntity, TModel, TShortModel>(
    TDbContext context,
    IReadonlyRepositoryMapper<TDbEntity, TModel> mapper,
    IReadonlyRepositoryMapper<TDbEntity, TShortModel> shortMapper)
    : ReadonlyRepository<TDbContext, TDbEntity, TModel>(context, mapper),
        IReadonlyRepositoryWithShortModel<TModel, TShortModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbEntityWithId, new()
    where TModel : TShortModel, IModelWithId
    where TShortModel : IModelWithId
{
    public async Task<TShortModel> GetShortAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(id, BaseQuery(), cancellationToken);
        return shortMapper.Map(entity);
    }

    public async Task<TShortModel?> FindShortAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var db = await FindEntityAsync(id, BaseQuery(), cancellationToken);
        return db == null ? default : shortMapper.Map(db);
    }

    public async Task<TShortModel[]> SelectShortAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var entities = await SelectEntitiesAsync(ids, BaseQuery(), cancellationToken);
        return entities.Select(shortMapper.Map).ToArray()!;
    }

    protected async Task<TShortModel[]> SearchShortAsync<TSearchModel>(
        IPredicateBuilder<TDbEntity, TSearchModel> predicateBuilder,
        TSearchModel searchModel,
        IQueryable<TDbEntity>? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= BaseQuery().AsNoTracking();
        var predicate = predicateBuilder.Build(searchModel);
        var entities = await query
            .Where(predicate)
            .ToArrayAsync(cancellationToken);

        return entities.Select(shortMapper.Map).ToArray()!;
    }
}