using Dal.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public abstract class ReadonlyRowRepository<TDbContext, TDbEntity, TModel>(
    TDbContext context,
    IReadonlyRepositoryMapper<TDbEntity, TModel> mapper)
    : IReadonlyRowRepository<TModel>
    where TDbContext : DbContext
    where TDbEntity : class, new()
    where TModel : class
{
    private Task<TModel[]>? _cache;

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await Query().CountAsync(cancellationToken);
    }

    public virtual async Task<TModel[]> SelectAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Query().ToArrayAsync(cancellationToken);
        return entities.Select(mapper.Map).ToArray()!;
    }

    public virtual async Task<TModel[]> SelectPageAsync(int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var entities = await Query().GetPagedAsync(pageNumber, pageSize, cancellationToken);

        return entities.Select(mapper.Map).ToArray()!;
    }

    public Task<TModel[]> SelectAllFromCacheAsync(CancellationToken cancellationToken = default)
    {
        return _cache ??= SelectAllAsync(cancellationToken);
    }

    protected virtual IQueryable<TDbEntity> BaseQuery() => context.Set<TDbEntity>().AsNoTracking().IgnoreQueryFilters();

    protected virtual IQueryable<TDbEntity> Query() => BaseQuery();
}
