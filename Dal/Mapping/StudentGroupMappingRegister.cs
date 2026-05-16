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
        model!.Children = entity.Children.Select(x => MapEntityToModelTree(x, false)).ToArray()!;
        model.Parents = entity.Parents.Select(x => MapEntityToModelTree(x, true)).ToArray()!;
        return model;
    }

    [UserMapping(Default = true)]
    public static StudentGroupRegistryItem? MapEntityToRegistryItem(DbStudentGroup? entity)
    {
        var item = AutoMapEntityToRegistryItem(entity);
        if (entity == null) return item;
        item!.Children = entity.Children.Select(x => new StudentGroupShortRegistryItem { Id = x.Id, Name = x.Name }).ToArray();
        item.Parents = entity.Parents.Select(x => new StudentGroupShortRegistryItem { Id = x.Id, Name = x.Name }).ToArray();
        return item;
    }

    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Children))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Children))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Parents))]
    public static partial DbStudentGroup? MapModelToEntity(StudentGroup? model);

    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Children))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Children))]
    [MapperIgnoreTarget(nameof(DbStudentGroup.Parents))]
    public static partial void UpdateEntityWithModel(StudentGroup? model, DbStudentGroup? entity);

    [MapperIgnoreSource(nameof(DbStudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Children))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Parents))]
    [MapperIgnoreTarget(nameof(StudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(StudentGroup.Children))]
    [MapperIgnoreTarget(nameof(StudentGroup.Parents))]
    [MapperIgnoreTarget(nameof(StudentGroup.ChildrenFlat))]
    private static partial StudentGroup? AutoMapEntityToModel(DbStudentGroup? entity);

    [MapperIgnoreSource(nameof(DbStudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Parents))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Children))]
    [MapperIgnoreTarget(nameof(StudentGroupRegistryItem.Parents))]
    [MapperIgnoreTarget(nameof(StudentGroupRegistryItem.Children))]
    private static partial StudentGroupRegistryItem? AutoMapEntityToRegistryItem(DbStudentGroup? entity);

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
            Parents = isParentMapping ? entity.Parents.Select(x => MapEntityToModelTree(x, isParentMapping)!).ToArray() : [],
            Children = !isParentMapping ? entity.Children.Select(x => MapEntityToModelTree(x, isParentMapping)!).ToArray() : [],
        };
    }
}