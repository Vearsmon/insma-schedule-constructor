using System.Linq.Expressions;
using Dal.Entities;
using Domain.Exceptions;
using Domain.Models.Common;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using static Dal.Helpers.PaginationExtensions;

namespace Dal.Repositories;

public abstract class ReadonlyRepository<TDbContext, TDbEntity, TModel>(
    TDbContext context,
    IReadonlyRepositoryMapper<TDbEntity, TModel> mapper)
    : IReadonlyRepository<TModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbEntityWithId, new()
    where TModel : IModelWithId
{
    protected readonly TDbContext Context = context;
    protected readonly IReadonlyRepositoryMapper<TDbEntity, TModel> MapperReadonly = mapper;

    private readonly Task<TModel[]>? _cache = null;

    public virtual async Task<TModel> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(id, cancellationToken);
        return MapperReadonly.Map(entity);
    }

    public virtual async Task<TModel?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entities = await SelectAsync([id], cancellationToken);
        return entities.FirstOrDefault();
    }

    public virtual async Task<TModel[]> SelectAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var entities = await SelectEntitiesAsync(ids, cancellationToken);

        return entities.Select(f => MapperReadonly.Map(f)).ToArray();
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await Query().CountAsync(cancellationToken);
    }

    public virtual async Task<TModel[]> SelectAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Query().AsNoTracking().ToArrayAsync(cancellationToken);

        return entities.Select(f => MapperReadonly.Map(f)).ToArray();
    }

    public async Task<Guid[]> SelectAllIdsAsync(CancellationToken cancellationToken = default)
    {
        return await Query().Select(x => x.Id)
                            .ToArrayAsync(cancellationToken);
    }

    public virtual async Task<TModel[]> SelectPageAsync(
        int pageNumber = PageNumberDefault,
        int pageSize = PageSizeDefault,
        CancellationToken cancellationToken = default)
    {
        var entities = await Query().AsNoTracking().GetPagedAsync(pageNumber, pageSize, cancellationToken);

        return entities.Select(x => MapperReadonly.Map(x)).ToArray();
    }

    public virtual async Task<Guid[]> SelectPageIdsAsync(
        int pageNumber = PageNumberDefault,
        int pageSize = PageSizeDefault,
        CancellationToken cancellationToken = default)
    {
        return await Query().Paged(pageNumber, pageSize)
                            .Select(x => x.Id)
                            .ToArrayAsync(cancellationToken);
    }

    public Task<TModel[]> SelectAllFromCacheAsync(CancellationToken cancellationToken = default)
    {
        return _cache ?? SelectAllAsync(cancellationToken);
    }

    protected async Task<bool> ExistAsync<TSearchModel>(IPredicateBuilder<TDbEntity, TSearchModel> predicateBuilder,
        TSearchModel searchModel,
        IQueryable<TDbEntity>? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= Query().AsNoTracking();
        return await query.AnyAsync(predicateBuilder.Build(searchModel), cancellationToken);
    }

    protected async Task<TModel[]> SearchAsync<TSearchModel>(IPredicateBuilder<TDbEntity, TSearchModel> predicateBuilder,
        TSearchModel searchModel,
        IQueryable<TDbEntity>? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= Query().AsNoTracking();
        var predicate = predicateBuilder.Build(searchModel);

        var entities = await query
            .Where(predicate)
            .ToArrayAsync(cancellationToken);

        return entities.Select(x => MapperReadonly.Map(x)).ToArray();
    }

    protected async Task<TResult[]> SearchAsync<TSearchModel, TResult>(IPredicateBuilder<TDbEntity, TSearchModel> predicateBuilder,
       TSearchModel searchModel,
       Expression<Func<TDbEntity, TResult>> projector,
       IQueryable<TDbEntity>? query = null,
       CancellationToken cancellationToken = default)
    {
        var predicate = predicateBuilder.Build(searchModel);
        query ??= Query().AsNoTracking();

        return await query
            .Where(predicate)
            .Select(projector)
            .ToArrayAsync(cancellationToken);
    }

    protected IQueryable<TDbEntity> BaseQuery() => Context.Set<TDbEntity>().IgnoreQueryFilters();

    protected virtual IQueryable<TDbEntity> Query() => BaseQuery();

    protected async Task<TDbEntity> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetEntityAsync(id, Query(), cancellationToken);
    }

    protected async Task<TDbEntity> GetEntityAsync(Guid id, IQueryable<TDbEntity> query, CancellationToken cancellationToken = default)
    {
        var entity = await query.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? throw new ServiceException(ServiceExceptionTypes.EntityNotFound);
    }

    protected async Task<TDbEntity?> FindEntityAsync(Guid id, IQueryable<TDbEntity> query, CancellationToken cancellationToken = default)
    {
        return await query.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    protected async Task<TDbEntity[]> SelectEntitiesAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        return await SelectEntitiesAsync(ids, Query(), cancellationToken);
    }

    protected async Task<TDbEntity[]> SelectEntitiesAsync(Guid[] ids, IQueryable<TDbEntity> query, CancellationToken cancellationToken = default)
    {
        if (ids.Length == 0)
        {
            return [];
        }
        var entities = await query.Where(i => ids.Contains(i.Id))
                                  .AsNoTracking()
                                  .ToArrayAsync(cancellationToken);
        return entities;
    }
}
