using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Lesson;

public class LessonRegistryPredicateBuilder : IPredicateBuilder<DbLesson, LessonRegistryInternalSearchModel>
{
    public Expression<Func<DbLesson, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLesson>();

    public Expression<Func<DbLesson, bool>> Build(LessonRegistryInternalSearchModel searchModel)
    {
        return Predicate;
    }
}