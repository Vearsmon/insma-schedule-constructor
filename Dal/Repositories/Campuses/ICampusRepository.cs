using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Campuses;

public interface ICampusRepository : IRepository<Campus>
{
    Task<Campus[]> SearchAsync(CampusSearchModel searchModel);

    Task<bool> ExistsAsync(Guid id);
}