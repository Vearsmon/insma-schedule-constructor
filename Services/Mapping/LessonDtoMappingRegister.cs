using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class LessonDtoMappingRegister
{
    [UserMapping(Default = true)]
    public static LessonViewDto? MapModelToViewDto(Lesson? model)
    {
        var dto = AutoMapModelToViewDto(model);
        if (model != null && dto != null)
        {
            dto.StudentGroupIds = model.StudentGroups.Select(x => x.Id!.Value).ToArray();
            dto.TeacherIds = model.Teachers.Select(x => x.Id!.Value).ToArray();
            dto.RoomIds = model.Rooms.Select(x => x.Id!.Value).ToArray();
        }
        return dto;
    }

    [UserMapping(Default = true)]
    public static Lesson? MapSaveDtoToModel(LessonSaveDto? dto)
    {
        var model = AutoMapSaveDtoToModel(dto);
        if (dto == null) return model;
        model!.StudentGroups = dto.StudentGroupIds.Select(x => new StudentGroup { Id = x }).ToArray();
        model.Teachers = dto.TeacherIds.Select(x => new Teacher { Id = x }).ToArray();
        model.Rooms = dto.RoomIds.Select(x => new Room { Id = x }).ToArray();
        return model;
    }

    [UserMapping(Default = true)]
    public static LessonShortDto? MapModelToShortDto(Lesson? model)
    {
        var shortDto = AutoMapModelToShortDto(model);
        if (model == null) return shortDto;
        shortDto!.StudentGroups = model.StudentGroups.Select(StudentGroupDtoMappingRegister.MapModelToShortDto).ToArray()!;
        shortDto.Teachers = model.Teachers.Select(TeacherDtoMappingRegister.MapModelToShortDto).ToArray()!;
        shortDto.Rooms = model.Rooms.Select(RoomDtoMappingRegister.MapModelToShortDto).ToArray()!;
        return shortDto;
    }

    [UserMapping(Default = true)]
    public static void UpdateModelWithSaveDto(LessonSaveDto? dto, Lesson? model)
    {
        AutoUpdateModelWithSaveDto(dto, model);
        if (dto == null) return;
        model!.StudentGroups = dto.StudentGroupIds.Select(x => new StudentGroup { Id = x }).ToArray();
        model.Teachers = dto.TeacherIds.Select(x => new Teacher { Id = x }).ToArray();
        model.Rooms = dto.RoomIds.Select(x => new Room { Id = x }).ToArray();
    }

    public static partial LessonRegistryItemDto? MapItemToItemDto(LessonRegistryItem? item);

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
    [MapperIgnoreSource(nameof(Lesson.LessonBatchInfoId))]
    [MapperIgnoreSource(nameof(Lesson.LessonBatchInfo))]
    [MapperIgnoreTarget(nameof(LessonViewDto.StudentGroupIds))]
    [MapperIgnoreTarget(nameof(LessonViewDto.TeacherIds))]
    [MapperIgnoreTarget(nameof(LessonViewDto.RoomIds))]
    private static partial LessonViewDto? AutoMapModelToViewDto(Lesson? model);

    [MapperIgnoreSource(nameof(LessonSaveDto.StudentGroupIds))]
    [MapperIgnoreSource(nameof(LessonSaveDto.TeacherIds))]
    [MapperIgnoreSource(nameof(LessonSaveDto.RoomIds))]
    [MapperIgnoreTarget(nameof(Lesson.Schedule))]
    [MapperIgnoreTarget(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(Lesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(Lesson.Teachers))]
    [MapperIgnoreTarget(nameof(Lesson.Rooms))]
    [MapperIgnoreTarget(nameof(Lesson.LessonBatchInfoId))]
    [MapperIgnoreTarget(nameof(Lesson.LessonBatchInfo))]
    [MapperIgnoreTarget(nameof(Lesson.Violations))]
    private static partial Lesson? AutoMapSaveDtoToModel(LessonSaveDto? dto);

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
    [MapperIgnoreSource(nameof(Lesson.LessonBatchInfo))]
    [MapperIgnoreTarget(nameof(LessonShortDto.StudentGroups))]
    [MapperIgnoreTarget(nameof(LessonShortDto.Teachers))]
    [MapperIgnoreTarget(nameof(LessonShortDto.Rooms))]
    [MapperIgnoreTarget(nameof(LessonShortDto.LessonPolicyViolationDescription))]
    [MapProperty(nameof(Lesson.Violations), nameof(LessonShortDto.CurrentErrorsMaxLevel), Use = nameof(GetViolationsMaxLevel))]
    private static partial LessonShortDto? AutoMapModelToShortDto(Lesson? model);

    [MapperIgnoreSource(nameof(LessonSaveDto.StudentGroupIds))]
    [MapperIgnoreSource(nameof(LessonSaveDto.TeacherIds))]
    [MapperIgnoreSource(nameof(LessonSaveDto.RoomIds))]
    [MapperIgnoreTarget(nameof(Lesson.Schedule))]
    [MapperIgnoreTarget(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(Lesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(Lesson.Teachers))]
    [MapperIgnoreTarget(nameof(Lesson.Rooms))]
    [MapperIgnoreTarget(nameof(Lesson.LessonBatchInfoId))]
    [MapperIgnoreTarget(nameof(Lesson.LessonBatchInfo))]
    [MapperIgnoreTarget(nameof(Lesson.Violations))]
    private static partial void AutoUpdateModelWithSaveDto(LessonSaveDto? dto, Lesson? model);

    private static LessonValidationErrorType? GetViolationsMaxLevel(LessonPolicyViolation[] violations) =>
        violations.Length == 0 ? null : violations.Max(x => x.ErrorType);
}