using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Schedule;

internal class ScheduleRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbSchedule, ScheduleRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbSchedule, ScheduleRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbSchedule, ScheduleRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbSchedule, ScheduleRegistryItem,
            ScheduleRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        IScheduleRegistryRepository
{
}