using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonValidationMessages;

public class LessonValidationMessagePredicateBuilder : IPredicateBuilder<DbLessonValidationMessage, LessonValidationMessageSearchModel>
{
    public Expression<Func<DbLessonValidationMessage, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLessonValidationMessage>();

    public Expression<Func<DbLessonValidationMessage, bool>> Build(LessonValidationMessageSearchModel searchModel)
    {
        return Predicate
                .AndIf(searchModel.AffectedByLessonIds.Length > 0, f => f.AffectedByLessonId.HasValue && searchModel.AffectedByLessonIds.Contains(f.AffectedByLessonId!.Value))
                .AndIf(searchModel.LessonIds.Length > 0, f => searchModel.LessonIds.Contains(f.LessonId))
                .AndIf(searchModel.ValidationCodes.Length > 0, f => searchModel.ValidationCodes.Contains(f.Code))
            ;
    }
}