using Dal.Entities;
using Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class LessonPolicyViolationMappingRegister
{
    [UserMapping(Default = true)]
    public static void UpdateEntityWithModel(LessonPolicyViolation? model, DbPolicyViolation? entity)
    {
        AutoUpdateEntityWithModel(model, entity);
        if (model == null) return;
        entity!.Targets = model.Targets.Select(x => new DbPolicyViolationTarget
        {
            Id = x.Id ?? Guid.Empty,
            TargetId = x.TargetId,
            TargetType = x.TargetType,
            ViolationId = model.Id ?? Guid.Empty,
        }).ToArray();
    }

    [UserMapping(Default = true)]
    public static LessonPolicyViolation? MapEntityToModel(DbPolicyViolation? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (entity == null) return model;
        model!.Targets = entity.Targets.Select(x => new LessonPolicyViolationTarget
        {
            Id = x.Id,
            TargetId = x.TargetId,
            TargetType = x.TargetType,
            ViolationId = entity.Id,
        }).ToArray();
        return model;
    }

    [UserMapping(Default = true)]
    public static DbPolicyViolation? MapModelToEntity(LessonPolicyViolation? model)
    {
        var entity = AutoMapModelToEntity(model);
        if (model == null) return entity;
        entity!.Targets = model.Targets.Select(x => new DbPolicyViolationTarget
        {
            Id = x.Id ?? Guid.Empty,
            TargetId = x.TargetId,
            TargetType = x.TargetType,
            ViolationId = model.Id ?? Guid.Empty,
        }).ToArray();
        return entity;
    }

    [MapperIgnoreSource(nameof(DbPolicyViolation.Lesson))]
    [MapperIgnoreSource(nameof(DbPolicyViolation.Targets))]
    [MapperIgnoreTarget(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(LessonPolicyViolation.DayOfWeekTimeInterval))]
    [MapperIgnoreTarget(nameof(LessonPolicyViolation.Targets))]
    private static partial LessonPolicyViolation? AutoMapEntityToModel(DbPolicyViolation? entity);

    [MapperIgnoreSource(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.DayOfWeekTimeInterval))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.Targets))]
    [MapperIgnoreTarget(nameof(DbPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(DbPolicyViolation.Targets))]
    private static partial DbPolicyViolation? AutoMapModelToEntity(LessonPolicyViolation? model);

    [MapperIgnoreSource(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.DayOfWeekTimeInterval))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.Targets))]
    [MapperIgnoreTarget(nameof(DbPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(DbPolicyViolation.Targets))]
    private static partial void AutoUpdateEntityWithModel(LessonPolicyViolation? model, DbPolicyViolation? entity);
}