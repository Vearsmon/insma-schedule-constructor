using Domain.Models.Common;

namespace Dal.Repositories;

public interface IReadonlyRepository<TModel> : IReadonlyRowRepository<TModel>
    where TModel : IModelWithId
{
    Task<TModel> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TModel?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TModel[]> SelectAsync(Guid[] ids, CancellationToken cancellationToken = default);

    Task<Guid[]> SelectAllIdsAsync(CancellationToken cancellationToken = default);

    Task<Guid[]> SelectPageIdsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
