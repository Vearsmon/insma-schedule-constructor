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
        var timeFrom = searchModel.TimeInterval?.TimeFrom;
        var timeTo = searchModel.TimeInterval?.TimeTo;
        var orBlocksAllowed = searchModel.RoomIds.Length > 0
                              || searchModel.DaysOfWeek.Length > 0
                              || searchModel.TimeInterval != null;

        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.ScheduleId == searchModel.ScheduleId)
                .AndIf(searchModel.TeacherIds.Length > 0, f => searchModel.TeacherIds.Contains(f.TeacherId))
                .AndIf(searchModel.TeacherPreferenceTypes.Length > 0, f => f.TeacherPreferenceType != null && searchModel.TeacherPreferenceTypes.Contains(f.TeacherPreferenceType!.Value))
                .AndIf(orBlocksAllowed, PredicateBuilderExtensions.False<DbTeacherPreference>()
                    .OrIf(searchModel.RoomIds.Length > 0, f => f.RoomId.HasValue && searchModel.RoomIds.Contains(f.RoomId!.Value))
                    .OrIf(searchModel.DaysOfWeek.Length > 0, f => f.DayOfWeek != null && searchModel.DaysOfWeek.Contains(f.DayOfWeek!.Value))
                    .OrIf(searchModel.TimeInterval != null, f => f.TimeFrom != null && f.TimeTo != null && f.TimeFrom <= timeTo && f.TimeTo >= timeFrom))
            ;
    }
}