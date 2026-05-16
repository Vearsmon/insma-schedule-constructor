using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Lessons;

public class LessonConflictsPredicateBuilder : IPredicateBuilder<DbLesson, LessonConflictsSearchModel>
{
    public Expression<Func<DbLesson, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLesson>();

    public Expression<Func<DbLesson, bool>> Build(LessonConflictsSearchModel searchModel)
    {
        var lessonComponentsOrBlockAllowed = searchModel.StudentGroupIds.Length > 0
                                             || searchModel.TeacherIds.Length > 0
                                             || searchModel.RoomIds.Length > 0;

        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.ScheduleId == searchModel.ScheduleId)
                .AndIf(lessonComponentsOrBlockAllowed, PredicateBuilderExtensions.False<DbLesson>()
                    .OrIf(searchModel.StudentGroupIds.Length > 0, f => f.StudentGroups.Any(x => searchModel.StudentGroupIds.Contains(x.Id)))
                    .OrIf(searchModel.TeacherIds.Length > 0, f => f.Teachers.Any(x => searchModel.TeacherIds.Contains(x.Id)))
                    .OrIf(searchModel.RoomIds.Length > 0, f => f.Rooms.Any(x => searchModel.RoomIds.Contains(x.Id))))
                .AndIf(searchModel.DateWithTimeIntervals.Length > 0, BuildDateWithTimeIntervalExpression(searchModel))
            ;
    }

    private static Expression<Func<DbLesson, bool>> BuildDateWithTimeIntervalExpression(LessonConflictsSearchModel searchModel) =>
        searchModel.DateWithTimeIntervals.Aggregate(PredicateBuilderExtensions.False<DbLesson>(), (current, dateWithTimeInterval) =>
            current.Or(f => f.Date.HasValue
                            && f.Date!.Value == dateWithTimeInterval.Date
                            && f.TimeFrom <= dateWithTimeInterval.TimeInterval.TimeTo
                            && f.TimeTo >= dateWithTimeInterval.TimeInterval.TimeFrom));
}