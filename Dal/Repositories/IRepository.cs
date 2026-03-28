using Domain.Models.Common;

namespace Dal.Repositories;

public interface IRepository<TModel> : IReadonlyRepository<TModel> where TModel : IModelWithId
{
    Task<Guid> SaveAsync(TModel model, CancellationToken cancellationToken = default);

    Task<Guid[]> SaveAllAsync(TModel[] models, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id);

    Task DeleteAsync(Guid[] ids, CancellationToken cancellationToken = default);

    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}
