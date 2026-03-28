using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Campus;

internal class CampusRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbCampus, CampusRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbCampus, CampusRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbCampus, CampusRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbCampus, CampusRegistryItem,
            CampusRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        ICampusRegistryRepository
{
}