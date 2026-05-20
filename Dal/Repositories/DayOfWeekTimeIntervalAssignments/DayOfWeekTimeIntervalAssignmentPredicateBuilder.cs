using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.DayOfWeekTimeIntervalAssignments;

public class DayOfWeekTimeIntervalAssignmentPredicateBuilder : IPredicateBuilder<DbDayOfWeekTimeIntervalAssignment, DayOfWeekTimeIntervalAssignmentSearchModel>
{
    public Expression<Func<DbDayOfWeekTimeIntervalAssignment, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbDayOfWeekTimeIntervalAssignment>();

    public Expression<Func<DbDayOfWeekTimeIntervalAssignment, bool>> Build(DayOfWeekTimeIntervalAssignmentSearchModel searchModel)
    {
        return Predicate
                .AndIf(searchModel.LessonBatchInfoIds.Length > 0, f => searchModel.LessonBatchInfoIds.Contains(f.LessonBatchInfoId))
            ;
    }
}