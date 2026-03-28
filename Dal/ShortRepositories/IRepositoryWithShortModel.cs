using Domain.Models.Common;

namespace Dal.ShortRepositories;

public interface IRepositoryWithShortModel<TModel, TShortModel> : IRepositoryWithReadonlyShortModel<TModel, TShortModel>
    where TModel : TShortModel, IModelWithId
    where TShortModel : IModelWithId
{
    Task<Guid> SaveShortAsync(TShortModel model, CancellationToken cancellationToken = default);

    Task<Guid[]> SaveShortAllAsync(TShortModel[] models, CancellationToken cancellationToken = default);
}
