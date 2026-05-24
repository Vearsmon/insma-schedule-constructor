using Dal.Entities;
using Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class LessonPolicyViolationMappingRegister
{
    [MapperIgnoreSource(nameof(DbPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(LessonPolicyViolation.DayOfWeekTimeInterval))]
    public static partial LessonPolicyViolation? MapEntityToModel(DbPolicyViolation? entity);

    [MapperIgnoreSource(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.DayOfWeekTimeInterval))]
    [MapperIgnoreTarget(nameof(DbPolicyViolation.Lesson))]
    public static partial DbPolicyViolation? MapModelToEntity(LessonPolicyViolation? model);

    [MapperIgnoreSource(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.DayOfWeekTimeInterval))]
    [MapperIgnoreTarget(nameof(DbPolicyViolation.Lesson))]
    public static partial void UpdateEntityWithModel(LessonPolicyViolation? model, DbPolicyViolation? entity);
}