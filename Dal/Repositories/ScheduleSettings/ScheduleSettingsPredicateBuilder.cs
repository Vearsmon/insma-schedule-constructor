using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.ScheduleSettings;

public class ScheduleSettingsPredicateBuilder : IPredicateBuilder<DbScheduleSettings, ScheduleSettingsSearchModel>
{
    public Expression<Func<DbScheduleSettings, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbScheduleSettings>();

    public Expression<Func<DbScheduleSettings, bool>> Build(ScheduleSettingsSearchModel searchModel)
    {
        return Predicate;
    }
}