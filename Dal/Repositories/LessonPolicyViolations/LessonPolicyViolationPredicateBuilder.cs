using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.Enums;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonPolicyViolations;

public class LessonPolicyViolationPredicateBuilder : IPredicateBuilder<DbPolicyViolation, LessonPolicyViolationSearchModel>
{
    public Expression<Func<DbPolicyViolation, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbPolicyViolation>();

    public Expression<Func<DbPolicyViolation, bool>> Build(LessonPolicyViolationSearchModel searchModel)
    {
        return Predicate
                .AndIf(searchModel.AffectedByLessonIds.Length > 0,
                    f => f.Targets.Any(x => x.TargetType == LessonPolicyViolationTargetType.Lesson
                                            && searchModel.AffectedByLessonIds.Contains(x.TargetId)))
                .AndIf(searchModel.AffectedByAcademicDisciplineIds.Length > 0,
                    f => f.Targets.Any(x => x.TargetType == LessonPolicyViolationTargetType.AcademicDiscipline
                                            && searchModel.AffectedByAcademicDisciplineIds.Contains(x.TargetId)))
                .AndIf(searchModel.LessonIds.Length > 0, f => f.LessonId.HasValue && searchModel.LessonIds.Contains(f.LessonId!.Value))
                .AndIf(searchModel.LessonBatchInfoIds.Length > 0, f => f.LessonBatchInfoId.HasValue && searchModel.LessonBatchInfoIds.Contains(f.LessonBatchInfoId!.Value))
                .AndIf(searchModel.ValidationCodes.Length > 0, f => searchModel.ValidationCodes.Contains(f.Code))
            ;
    }
}