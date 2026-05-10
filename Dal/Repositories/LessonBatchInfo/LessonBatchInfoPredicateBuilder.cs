using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonBatchInfo;

public class LessonBatchInfoPredicateBuilder : IPredicateBuilder<DbLessonBatchInfo, LessonBatchInfoSearchModel>
{
    public Expression<Func<DbLessonBatchInfo, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLessonBatchInfo>();

    public Expression<Func<DbLessonBatchInfo, bool>> Build(LessonBatchInfoSearchModel searchModel)
    {
        return Predicate;
    }
}