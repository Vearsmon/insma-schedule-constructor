using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Campus;

public interface ICampusRegistryRepository
    : IRegistryRepository<CampusRegistryItem, CampusRegistryInternalSearchModel>
{
}