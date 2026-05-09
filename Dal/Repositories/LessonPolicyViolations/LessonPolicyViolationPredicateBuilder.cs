using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonPolicyViolations;

public class LessonPolicyViolationPredicateBuilder : IPredicateBuilder<DbLessonPolicyViolation, LessonPolicyViolationSearchModel>
{
    public Expression<Func<DbLessonPolicyViolation, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbLessonPolicyViolation>();

    public Expression<Func<DbLessonPolicyViolation, bool>> Build(LessonPolicyViolationSearchModel searchModel)
    {
        return Predicate
                .AndIf(searchModel.AffectedByLessonIds.Length > 0, f => f.AffectedByLessonId.HasValue && searchModel.AffectedByLessonIds.Contains(f.AffectedByLessonId!.Value))
                .AndIf(searchModel.AffectedByAcademicDisciplineIds.Length > 0, f => f.AffectedByAcademicDisciplineId.HasValue && searchModel.AffectedByAcademicDisciplineIds.Contains(f.AffectedByAcademicDisciplineId!.Value))
                .AndIf(searchModel.LessonIds.Length > 0, f => searchModel.LessonIds.Contains(f.LessonId))
                .AndIf(searchModel.ValidationCodes.Length > 0, f => searchModel.ValidationCodes.Contains(f.Code))
            ;
    }
}