using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Helpers;
using Domain.Models.Common;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Lessons;

public class LessonPredicateBuilder : IPredicateBuilder<DbLesson, LessonSearchModel>
{
    public Expression<Func<DbLesson, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLesson>();

    public Expression<Func<DbLesson, bool>> Build(LessonSearchModel searchModel)
    {
        var dates = searchModel is { DateFrom: not null, DateTo: not null }
            ? new DateInterval { DateFrom = searchModel.DateFrom!.Value, DateTo = searchModel.DateTo!.Value }.ToDateSequence().ToArray()
            : [];

        if (searchModel.SearchForConflicts)
        {
            var orBlocksAllowed = searchModel.StudentGroupIds.Length > 0
                                  || searchModel.TeacherIds.Length > 0
                                  || searchModel.RoomIds.Length > 0;

            return Predicate
                    .AndIf(searchModel.ScheduleId.HasValue, f => f.LessonBatchInfo.AcademicDiscipline.ScheduleId == searchModel.ScheduleId)
                    .AndIf(orBlocksAllowed, PredicateBuilderExtensions.False<DbLesson>()
                        .OrIf(searchModel.StudentGroupIds.Length > 0, f => f.StudentGroups.Any(x => searchModel.StudentGroupIds.Contains(x.Id)))
                        .OrIf(searchModel.TeacherIds.Length > 0, f => f.Teachers.Any(x => searchModel.TeacherIds.Contains(x.Id)))
                        .OrIf(searchModel.RoomIds.Length > 0, f => f.Rooms.Any(x => searchModel.RoomIds.Contains(x.Id))))
                    .AndIf(searchModel.Date.HasValue, f => f.Date == searchModel.Date)
                    .AndIf(dates.Length != 0, f => f.Date.HasValue && dates.Contains(f.Date!.Value))
                    // .AndIf(searchModel.DateFrom.HasValue, f => f.Date >= searchModel.DateFrom)
                    // .AndIf(searchModel.DateTo.HasValue, f => f.Date <= searchModel.DateTo)
                    .AndIf(searchModel.TimeIntervals.Length > 0, BuildTimeIntervalExpression(searchModel))
                    .AndIf(searchModel.ExcludeAllowCombining, f => f.AllowCombining == false)
                    .AndIf(searchModel.ExcludeLessonIds.Length > 0, f => !searchModel.ExcludeLessonIds.Contains(f.Id))
                    .AndIf(searchModel.ExcludeLessonBatchInfoId.HasValue, f => f.LessonBatchInfoId != searchModel.ExcludeLessonBatchInfoId)
                ;
        }
        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.LessonBatchInfo.AcademicDiscipline.ScheduleId == searchModel.ScheduleId)
                .AndIf(searchModel.AcademicDisciplineId.HasValue, f => f.LessonBatchInfo.AcademicDisciplineId == searchModel.AcademicDisciplineId)
                .AndIf(searchModel.Types.Length > 0, f => searchModel.Types.Contains(f.LessonBatchInfo.Type))
                .AndIf(searchModel.StudentGroupIds.Length > 0, f => f.StudentGroups.Any(x => searchModel.StudentGroupIds.Contains(x.Id)))
                .AndIf(searchModel.TeacherIds.Length > 0, f => f.Teachers.Any(x => searchModel.TeacherIds.Contains(x.Id)))
                .AndIf(searchModel.RoomIds.Length > 0, f => f.Rooms.Any(x => searchModel.RoomIds.Contains(x.Id)))
                .AndIf(searchModel.Date.HasValue, f => f.Date == searchModel.Date)
                .AndIf(dates.Length != 0, f => f.Date.HasValue && dates.Contains(f.Date!.Value))
                // .AndIf(searchModel.DateFrom.HasValue, f => f.Date >= searchModel.DateFrom)
                // .AndIf(searchModel.DateTo.HasValue, f => f.Date <= searchModel.DateTo)
                .AndIf(searchModel.TimeIntervals.Length > 0, BuildTimeIntervalExpression(searchModel))
                .AndIf(searchModel.DayOfWeekTimeIntervals.Length > 0, BuildDayOfWeekTimeIntervalExpression(searchModel))
                .AndIf(searchModel.ExcludeAllowCombining, f => f.AllowCombining == false)
                .AndIf(searchModel.ExcludeLessonIds.Length > 0, f => !searchModel.ExcludeLessonIds.Contains(f.Id))
                .AndIf(searchModel.LessonBatchInfoIds.Length > 0, f => searchModel.LessonBatchInfoIds.Contains(f.LessonBatchInfoId))
                .AndIf(searchModel.HasNoTimeAssignment, f => f.Date == null && f.TimeFrom == null && f.TimeTo == null)
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