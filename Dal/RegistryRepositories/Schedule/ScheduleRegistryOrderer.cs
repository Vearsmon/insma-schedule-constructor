using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Schedule;

public class ScheduleRegistryOrderer
    : IRegistryRepositoryOrderer<DbSchedule, ScheduleRegistryInternalSearchModel>
{
    public IQueryable<DbSchedule> Order(IQueryable<DbSchedule> queryable,
        ScheduleRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}