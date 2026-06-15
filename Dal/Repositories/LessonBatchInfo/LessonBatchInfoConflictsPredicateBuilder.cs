using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonBatchInfo;

public class LessonBatchInfoConflictsPredicateBuilder : IPredicateBuilder<DbLessonBatchInfo, LessonBatchInfoConflictsSearchModel>
{
    public Expression<Func<DbLessonBatchInfo, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLessonBatchInfo>();

    public Expression<Func<DbLessonBatchInfo, bool>> Build(LessonBatchInfoConflictsSearchModel searchModel)
    {
        var lessonComponentsOrBlockAllowed = searchModel.StudentGroupIds.Length > 0
                                             || searchModel.TeacherIds.Length > 0
                                             || searchModel.RoomIds.Length > 0;

        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.AcademicDiscipline.ScheduleId == searchModel.ScheduleId)
                .AndIf(lessonComponentsOrBlockAllowed, PredicateBuilderExtensions.False<DbLessonBatchInfo>()
                    .OrIf(searchModel.StudentGroupIds.Length > 0, f => f.StudentGroups.Any(x => searchModel.StudentGroupIds.Contains(x.Id)))
                    .OrIf(searchModel.TeacherIds.Length > 0, f => f.Teachers.Any(x => searchModel.TeacherIds.Contains(x.Id)))
                    .OrIf(searchModel.RoomIds.Length > 0, f => f.Rooms.Any(x => searchModel.RoomIds.Contains(x.Id))))
                .AndIf(searchModel.DateWithTimeIntervals.Length > 0, BuildDateWithTimeIntervalExpression(searchModel))
                .AndIf(searchModel.ExcludeBatchIds.Length > 0, f => !searchModel.ExcludeBatchIds.Contains(f.Id))
            ;
    }

    private static Expression<Func<DbLessonBatchInfo, bool>> BuildDateWithTimeIntervalExpression(LessonBatchInfoConflictsSearchModel searchModel) =>
        searchModel.DateWithTimeIntervals.Aggregate(PredicateBuilderExtensions.False<DbLessonBatchInfo>(), (current, dateWithTimeInterval) =>
            current.Or(f => f.DayOfWeekTimeIntervals.Any(x =>
                x.DayOfWeek == dateWithTimeInterval.Date.DayOfWeek
                && x.TimeFrom <= dateWithTimeInterval.TimeInterval.TimeTo
                && x.TimeTo >= dateWithTimeInterval.TimeInterval.TimeFrom)));
}