using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.LessonPolicyViolations;

public class LessonPolicyViolationMapper : IRepositoryMapper<DbLessonPolicyViolation, LessonPolicyViolation>
{
    [return: NotNullIfNotNull("entity")]
    public LessonPolicyViolation? Map(DbLessonPolicyViolation? entity) => LessonPolicyViolationMappingRegister.MapEntityToModel(entity);

    public void Update(DbLessonPolicyViolation entity, LessonPolicyViolation model)
    {
        LessonPolicyViolationMappingRegister.UpdateEntityWithModel(model, entity);
    }
}