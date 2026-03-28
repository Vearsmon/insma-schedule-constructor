using Dal.Repositories;
using Domain.Models.Common;

namespace Dal.ShortRepositories;

public interface IReadonlyRepositoryWithShortModel<TModel, TShortModel> : IReadonlyRepository<TModel>
    where TModel : TShortModel, IModelWithId
    where TShortModel : IModelWithId
{
    Task<TShortModel> GetShortAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TShortModel?> FindShortAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TShortModel[]> SelectShortAsync(Guid[] ids, CancellationToken cancellationToken = default);
}
