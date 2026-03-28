namespace Dal.Repositories;

public interface IReadonlyRowRepository<TModel>
{
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<TModel[]> SelectAllAsync(CancellationToken cancellationToken = default);

    Task<TModel[]> SelectPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<TModel[]> SelectAllFromCacheAsync(CancellationToken cancellationToken = default);
}
