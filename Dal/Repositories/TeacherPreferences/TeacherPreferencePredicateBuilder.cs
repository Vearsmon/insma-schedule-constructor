using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.TeacherPreferences;

internal class TeacherPreferencePredicateBuilder : IPredicateBuilder<DbTeacherPreference, TeacherPreferenceSearchModel>
{
    public Expression<Func<DbTeacherPreference, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbTeacherPreference>();

    public Expression<Func<DbTeacherPreference, bool>> Build(TeacherPreferenceSearchModel searchModel)
    {
        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.ScheduleId == searchModel.ScheduleId)
                .AndIf(searchModel.TeacherId.HasValue, f => f.TeacherId == searchModel.TeacherId)
                .AndIf(searchModel.DaysOfWeek.Length > 0, f => f.DayOfWeek != null && searchModel.DaysOfWeek.Contains(f.DayOfWeek!.Value))
                // .And(PredicateBuilder.False<DbTeacherPreference>()
                    // .OrIf(searchModel.TimeFrom.HasValue, f => f.TimeSequences.Any(t => t.Item1 <= searchModel.TimeFrom && t.Item2 >= searchModel.TimeFrom))
                    // .OrIf(searchModel.TimeTo.HasValue, f => f.TimeSequences.Any(t => t.Item1 <= searchModel.TimeTo && t.Item2 >= searchModel.TimeTo)))
                .AndIf(searchModel.TeacherPreferenceTypes.Length > 0, f => f.TeacherPreferenceType != null && searchModel.TeacherPreferenceTypes.Contains(f.TeacherPreferenceType!.Value))
            ;
    }
}