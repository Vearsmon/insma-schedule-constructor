using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Schedules;

public class SchedulePredicateBuilder : IPredicateBuilder<DbSchedule, ScheduleSearchModel>
{
    public Expression<Func<DbSchedule, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbSchedule>();

    public Expression<Func<DbSchedule, bool>> Build(ScheduleSearchModel searchModel)
    {
        return Predicate;
    }
}