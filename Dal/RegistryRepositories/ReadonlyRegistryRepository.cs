using Dal.Entities;
using Dal.Repositories;
using Domain.Dto.RegistryDto;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Dal.RegistryRepositories;

public abstract class ReadonlyRegistryRepository<TDbContext, TDbEntity, TRegistryItem, TSearchModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbEntityWithId, new()
    where TRegistryItem : IModelWithId
    where TSearchModel : IWithSearchParameters
{
    protected readonly TDbContext Context;
    protected readonly IRegistryRepositoryOrderer<TDbEntity, TSearchModel> Orderer;
    protected readonly IPredicateBuilder<TDbEntity, TSearchModel> PredicateBuilder;
    private readonly IReadonlyRepositoryMapper<TDbEntity, TRegistryItem> _registryMapper;

    protected ReadonlyRegistryRepository(TDbContext context,
                                         IReadonlyRepositoryMapper<TDbEntity, TRegistryItem> registryMapper,
                                         IRegistryRepositoryOrderer<TDbEntity, TSearchModel> orderer,
                                         IPredicateBuilder<TDbEntity, TSearchModel> predicateBuilder)
    {
        Context = context;
        _registryMapper = registryMapper;
        Orderer = orderer;
        PredicateBuilder = predicateBuilder;
        Context = context;
    }

    protected virtual IQueryable<TDbEntity> Query => Context.Set<TDbEntity>().AsNoTracking();

    public virtual async Task<RegistryDto<TRegistryItem>> SearchAsync(TSearchModel searchModel)
    {
        var predicate = PredicateBuilder.Build(searchModel);
        var entities = Query.Where(predicate);

        return await CreateRegistryDto(entities, searchModel);
    }

    public async Task<Guid[]> SelectIds(TSearchModel searchModel)
    {
        var predicate = PredicateBuilder.Build(searchModel);
        var entities = Query.Where(predicate);
        var ordered = Orderer.Order(entities, searchModel);
        if (ordered is EnumerableQuery<TDbEntity>)
        {
            return ordered.Select(x => x.Id).ToArray();
        }
        return await ordered.Select(x => x.Id)
                            .ToArrayAsync();
    }

    protected async Task<RegistryDto<TRegistryItem>> CreateRegistryDto(IQueryable<TDbEntity> entities, TSearchModel searchModel)
    {
        var paged = await Orderer.Order(entities, searchModel)
                                 .Page(searchModel.SearchParameters)
                                 .AsNoTracking()
                                 .ToArrayAsync();

        return new RegistryDto<TRegistryItem>
        {
            ItemsCount = await entities.CountAsync(),
            Items = paged.Select(_registryMapper.Map).ToArray()!
        };
    }
}
