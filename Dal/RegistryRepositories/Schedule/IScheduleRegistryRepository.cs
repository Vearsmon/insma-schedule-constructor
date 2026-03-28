using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Schedule;

public interface IScheduleRegistryRepository
    : IRegistryRepository<ScheduleRegistryItem, ScheduleRegistryInternalSearchModel>
{
}