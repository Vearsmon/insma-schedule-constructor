using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.Enums;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonBatchInfo;

public class LessonBatchInfoPredicateBuilder : IPredicateBuilder<DbLessonBatchInfo, LessonBatchInfoSearchModel>
{
    public Expression<Func<DbLessonBatchInfo, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLessonBatchInfo>();

    public Expression<Func<DbLessonBatchInfo, bool>> Build(LessonBatchInfoSearchModel searchModel)
    {
        return Predicate
                .AndIf(searchModel.ScheduleId.HasValue, f => f.AcademicDiscipline.ScheduleId == searchModel.ScheduleId)
                .AndIf(searchModel is { DateFrom: not null, DateTo: not null }, f => f.DateFrom <= searchModel.DateTo && searchModel.DateFrom <= f.DateTo)
                .AndIf(searchModel.AcademicDisciplineId.HasValue, f => f.AcademicDisciplineId == searchModel.AcademicDisciplineId)
                .AndIf(searchModel.IntersectsEvenWeek, f => f.RepeatType != DisciplineLessonRepeatType.OddWeeks)
                .AndIf(searchModel.IntersectsOddWeek, f => f.RepeatType != DisciplineLessonRepeatType.EvenWeeks)
            ;
    }
}