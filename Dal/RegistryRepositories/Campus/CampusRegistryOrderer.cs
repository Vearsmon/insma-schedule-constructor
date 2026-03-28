using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Campus;

public class CampusRegistryOrderer : IRegistryRepositoryOrderer<DbCampus, CampusRegistryInternalSearchModel>
{
    public IQueryable<DbCampus> Order(IQueryable<DbCampus> queryable, CampusRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}