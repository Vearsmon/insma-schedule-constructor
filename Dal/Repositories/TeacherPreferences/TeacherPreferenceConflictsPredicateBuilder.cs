using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.TeacherPreferences;

public class TeacherPreferenceConflictsPredicateBuilder : IPredicateBuilder<DbTeacherPreference, TeacherPreferenceConflictsSearchModel>
{
    public Expression<Func<DbTeacherPreference, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbTeacherPreference>();

    public Expression<Func<DbTeacherPreference, bool>> Build(TeacherPreferenceConflictsSearchModel searchModel)
    {
        var lessonComponentsOrBlockAllowed = searchModel.RoomIds.Length > 0
                                             || searchModel.DayOfWeekTimeIntervals.Length > 0;

        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.ScheduleId == searchModel.ScheduleId)
                .AndIf(searchModel.TeacherIds.Length > 0, f => searchModel.TeacherIds.Contains(f.TeacherId))
                .AndIf(searchModel.TeacherPreferenceTypes.Length > 0, f => f.TeacherPreferenceType != null && searchModel.TeacherPreferenceTypes.Contains(f.TeacherPreferenceType!.Value))
                .AndIf(lessonComponentsOrBlockAllowed, PredicateBuilderExtensions.False<DbTeacherPreference>()
                    .OrIf(searchModel.RoomIds.Length > 0, f => f.RoomId.HasValue && searchModel.RoomIds.Contains(f.RoomId!.Value))
                    .OrIf(searchModel.DayOfWeekTimeIntervals.Length > 0, BuildDayOfWeekTimeIntervalExpression(searchModel)))
            ;
    }

    private static Expression<Func<DbTeacherPreference, bool>> BuildDayOfWeekTimeIntervalExpression(TeacherPreferenceConflictsSearchModel searchModel) =>
        searchModel.DayOfWeekTimeIntervals.Aggregate(PredicateBuilderExtensions.False<DbTeacherPreference>(), (current, dayOfWeekTimeInterval) =>
            current.Or(f => f.DayOfWeek.HasValue
                            && f.DayOfWeek!.Value == dayOfWeekTimeInterval.DayOfWeek
                            && f.TimeFrom <= dayOfWeekTimeInterval.TimeInterval.TimeTo
                            && f.TimeTo >= dayOfWeekTimeInterval.TimeInterval.TimeFrom));
}