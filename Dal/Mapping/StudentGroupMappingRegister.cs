using Dal.Entities;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class StudentGroupMappingRegister
{
    [UserMapping(Default = true)]
    public static StudentGroup? MapEntityToModel(DbStudentGroup? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (entity == null) return model;
        model!.Children = entity.Children.Select(x => MapEntityToModelTree(x.ChildStudentGroup, false)).ToArray()!;
        model.Parents = entity.Parents.Select(x => MapEntityToModelTree(x.ParentStudentGroup, true)).ToArray()!;
        return model;
    }

    [UserMapping(Default = true)]
    public static DbStudentGroup? MapModelToEntity(StudentGroup? model)
    {
        var entity = AutoMapModelToEntity(model);
        if (model == null) return entity;
        entity!.Children = model.Children.Select(x => new DbStudentGroupLink { ParentStudentGroupId = model.Id ?? Guid.Empty, ChildStudentGroupId = x.Id ?? Guid.Empty }).ToArray();
        entity.Parents = model.Parents.Select(x =>new DbStudentGroupLink { ParentStudentGroupId = x.Id ?? Guid.Empty, ChildStudentGroupId = model.Id ?? Guid.Empty }).ToArray();
        return entity;
    }

    [UserMapping(Default = true)]
    public static void UpdateEntityWithModel(StudentGroup? model, DbStudentGroup? entity)
    {
        AutoUpdateEntityWithModel(model, entity);
        if (model == null) return;
        entity!.Children = model.Children.Select(x => new DbStudentGroupLink { ParentStudentGroupId = model.Id ?? Guid.Empty, ChildStudentGroupId = x.Id ?? Guid.Empty }).ToArray();
        entity.Parents = model.Parents.Select(x =>new DbStudentGroupLink { ParentStudentGroupId = x.Id ?? Guid.Empty, ChildStudentGroupId = model.Id ?? Guid.Empty }).ToArray();
    }

    [MapperIgnoreSource(nameof(DbStudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Parents))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Children))]
    public static partial StudentGroupRegistryItem? MapEntityToRegistryItem(DbStudentGroup? entity);

    [MapperIgnoreSource(nameof(DbStudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Children))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Parents))]
    [MapperIgnoreTarget(nameof(StudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(StudentGroup.Children))]
    [MapperIgnoreTarget(nameof(StudentGroup.Parents))]
    [MapperIgnoreTarget(nameof(StudentGroup.ChildrenFlat))]
    // [MapProperty(nameof(DbStudentGroup.Schedule), nameof(StudentGroup.Schedule), Use = nameof(@ScheduleMappingRegister.MapEntityToModel))]
    private static partial StudentGroup? AutoMapEntityToModel(DbStudentGroup? entity);

    [MapperIgnoreSource(nameof(StudentGroup.Children))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Children))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Parents))]
    [MapProperty(nameof(StudentGroup.Schedule), nameof(DbStudentGroup.Schedule), Use = nameof(@ScheduleMappingRegister.MapModelToEntity))]
    private static partial DbStudentGroup? AutoMapModelToEntity(StudentGroup? model);

    [MapperIgnoreSource(nameof(StudentGroup.Children))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Children))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Parents))]
    [MapProperty(nameof(StudentGroup.Schedule), nameof(DbStudentGroup.Schedule), Use = nameof(@ScheduleMappingRegister.MapModelToEntity))]
    private static partial void AutoUpdateEntityWithModel(StudentGroup? model, DbStudentGroup? entity);

    private static StudentGroup? MapEntityToModelTree(DbStudentGroup? entity, bool isParentMapping)
    {
        return entity == null ? null : new StudentGroup
        {
            Id = entity.Id,
            ScheduleId = entity.ScheduleId,
            Name = entity.Name,
            SemesterNumber = entity.SemesterNumber,
            StudentsCount = entity.StudentsCount,
            StudentGroupType = entity.StudentGroupType,
            Parents = isParentMapping ? entity.Parents.Select(x => MapEntityToModelTree(x.ParentStudentGroup, isParentMapping)!).ToArray() : [],
            Children = !isParentMapping ? entity.Children.Select(x => MapEntityToModelTree(x.ChildStudentGroup, isParentMapping)!).ToArray() : [],
        };
    }
}