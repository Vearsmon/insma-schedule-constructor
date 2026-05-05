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
        if (dto != null && model != null)
        {
            model.StudentGroups = dto.StudentGroupIds.Select(x => new StudentGroup { Id = x }).ToArray();
        }
        return model;
    }

    public static partial LessonRegistryItemDto? MapItemToItemDto(LessonRegistryItem? item);

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapProperty(nameof(Lesson.ValidationMessages), nameof(LessonShortDto.CurrentErrorsMaxLevel), Use = nameof(GetErrorsMaxLevel))]
    public static partial LessonShortDto? MapModelToShortDto(Lesson? model);

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
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
    [MapperIgnoreTarget(nameof(Lesson.ValidationMessages))]
    private static partial Lesson? AutoMapSaveDtoToModel(LessonSaveDto? dto);

    private static LessonValidationErrorType? GetErrorsMaxLevel(LessonValidationMessage[] messages) => messages.Length == 0 ? null : messages.Max(x => x.ErrorType);
}