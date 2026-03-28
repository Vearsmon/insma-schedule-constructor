namespace Dal.RegistryRepositories;

public interface IRegistryRepositoryOrderer<TEntity, in TSearchModel>
{
    IQueryable<TEntity> Order(IQueryable<TEntity> queryable, TSearchModel searchModel);
}
