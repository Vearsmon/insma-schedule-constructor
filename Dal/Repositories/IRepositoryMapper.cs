namespace Dal.Repositories;

public interface IRepositoryMapper<in TEntity, TModel> : IReadonlyRepositoryMapper<TEntity, TModel>
{
    void Update(TEntity entity, TModel model);
}
