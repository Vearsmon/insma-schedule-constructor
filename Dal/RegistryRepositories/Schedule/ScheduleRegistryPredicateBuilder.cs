using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Schedule;

public class ScheduleRegistryPredicateBuilder : IPredicateBuilder<DbSchedule, ScheduleRegistryInternalSearchModel>
{
    public Expression<Func<DbSchedule, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbSchedule>();

    public Expression<Func<DbSchedule, bool>> Build(ScheduleRegistryInternalSearchModel searchModel)
    {
        return Predicate;
    }
}