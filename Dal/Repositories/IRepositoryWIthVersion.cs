using Domain.Models.Common;

namespace Dal.Repositories;

public interface IRepositoryWithVersion<TModel> : IRepository<TModel>
    where TModel : IWithVersion, IModelWithId
{
    Task<TModel> GetByRootAsync(Guid rootId);

    Task<TModel[]> GetByRootsAsync(Guid[] rootIds);
}
