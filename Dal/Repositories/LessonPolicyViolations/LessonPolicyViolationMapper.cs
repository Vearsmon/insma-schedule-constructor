using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.LessonPolicyViolations;

public class LessonPolicyViolationMapper : IRepositoryMapper<DbPolicyViolation, LessonPolicyViolation>
{
    [return: NotNullIfNotNull("entity")]
    public LessonPolicyViolation? Map(DbPolicyViolation? entity) => LessonPolicyViolationMappingRegister.MapEntityToModel(entity);

    public void Update(DbPolicyViolation entity, LessonPolicyViolation model)
    {
        LessonPolicyViolationMappingRegister.UpdateEntityWithModel(model, entity);
    }
}