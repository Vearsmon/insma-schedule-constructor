using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Lessons;

public class LessonPredicateBuilder : IPredicateBuilder<DbLesson, LessonSearchModel>
{
    public Expression<Func<DbLesson, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLesson>();

    public Expression<Func<DbLesson, bool>> Build(LessonSearchModel searchModel)
    {
        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.ScheduleId == searchModel.ScheduleId)
                .AndIf(searchModel.AcademicDisciplineId.HasValue, f => f.AcademicDisciplineId == searchModel.AcademicDisciplineId)
                .AndIf(searchModel.Types.Length > 0, f => f.AcademicDisciplineType != null && searchModel.Types.Contains(f.AcademicDisciplineType!.Value))
                .AndIf(searchModel.StudentGroupIds.Length > 0, f => searchModel.StudentGroupIds.Contains(f.StudentGroupId))
                .AndIf(searchModel.RoomIds.Length > 0, f => f.RoomId != null && searchModel.RoomIds.Contains(f.RoomId!.Value))
                .AndIf(searchModel.Date.HasValue, f => f.Date == searchModel.Date)
                .AndIf(searchModel.DateFrom.HasValue, f => f.Date >= searchModel.DateFrom)
                .AndIf(searchModel.DateTo.HasValue, f => f.Date <= searchModel.DateTo)
                .AndIf(searchModel.TeacherId.HasValue, f => f.TeacherId == searchModel.TeacherId)
                .AndIf(searchModel.TimeIntervals.Length > 0, BuildTimeIntervalExpression(searchModel))
                .AndIf(searchModel.DayOfWeekTimeIntervals.Length > 0, BuildDayOfWeekTimeIntervalExpression(searchModel))
                .AndIf(searchModel.CreatedFromDiscipline, f => f.CreatedFromDiscipline == true)
                .AndIf(searchModel.ExcludeAllowCombining, f => f.AllowCombining == false)
            ;
    }

    private static Expression<Func<DbLesson, bool>> BuildTimeIntervalExpression(LessonSearchModel searchModel) =>
        searchModel.TimeIntervals.Aggregate(PredicateBuilderExtensions.False<DbLesson>(), (current, timeInterval) =>
            current.Or(f => f.TimeFrom <= timeInterval.TimeTo && f.TimeTo >= timeInterval.TimeFrom));

    private static Expression<Func<DbLesson, bool>> BuildDayOfWeekTimeIntervalExpression(LessonSearchModel searchModel) =>
        searchModel.DayOfWeekTimeIntervals.Aggregate(PredicateBuilderExtensions.False<DbLesson>(), (current, dayOfWeekTimeInterval) =>
            current.Or(f => f.Date.HasValue
                            && f.Date!.Value.DayOfWeek == dayOfWeekTimeInterval.DayOfWeek
                            && f.TimeFrom <= dayOfWeekTimeInterval.TimeInterval.TimeTo
                            && f.TimeTo >= dayOfWeekTimeInterval.TimeInterval.TimeFrom));
}