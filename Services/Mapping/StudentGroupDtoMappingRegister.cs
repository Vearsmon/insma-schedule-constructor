using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class StudentGroupDtoMappingRegister
{
    [MapperIgnoreSource(nameof(StudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    public static partial StudentGroupViewDto? MapModelToViewDto(StudentGroup? model);

    [MapperIgnoreSource(nameof(StudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.Children))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    [MapperIgnoreSource(nameof(StudentGroup.StudentsCount))]
    [MapperIgnoreSource(nameof(StudentGroup.SemesterNumber))]
    [MapperIgnoreSource(nameof(StudentGroup.StudentGroupType))]
    public static partial StudentGroupShortDto? MapModelToShortDto(StudentGroup? model);

    [MapperIgnoreSource(nameof(StudentGroupSaveDto.ChildIds))]
    [MapperIgnoreSource(nameof(StudentGroupSaveDto.SemiGroupToCreateNames))]
    [MapperIgnoreTarget(nameof(StudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(StudentGroup.Children))]
    [MapProperty(nameof(StudentGroupSaveDto.ParentIds), nameof(StudentGroup.Parents), Use = nameof(MapReferences))]
    public static partial StudentGroup? MapSaveDtoToModel(StudentGroupSaveDto? dto);

    [MapperIgnoreSource(nameof(StudentGroupSaveDto.SemiGroupToCreateNames))]
    [MapperIgnoreTarget(nameof(StudentGroup.Schedule))]
    [MapProperty(nameof(StudentGroupSaveDto.ChildIds), nameof(StudentGroup.Children), Use = nameof(MapReferences))]
    [MapProperty(nameof(StudentGroupSaveDto.ParentIds), nameof(StudentGroup.Parents), Use = nameof(MapReferences))]
    public static partial void UpdateModelWithSaveDto(StudentGroupSaveDto? dto, StudentGroup? model);

    public static partial StudentGroupRegistryItemDto? MapItemToItemDto(StudentGroupRegistryItem? item);

    [MapperIgnoreSource(nameof(StudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.SemesterNumber))]
    [MapperIgnoreSource(nameof(StudentGroup.StudentsCount))]
    [MapperIgnoreSource(nameof(StudentGroup.StudentGroupType))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    public static partial StudentGroupTreeDto? MapModelToTreeDto(StudentGroup model);

    private static StudentGroup[] MapReferences(Guid[] ids) => ids.Select(x => new StudentGroup { Id = x }).ToArray();
}