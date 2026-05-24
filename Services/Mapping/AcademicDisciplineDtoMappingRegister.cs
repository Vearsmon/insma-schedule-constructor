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
public static partial class AcademicDisciplineDtoMappingRegister
{
    [UserMapping(Default = true)]
    public static AcademicDisciplineViewDto? MapModelToViewDto(AcademicDiscipline? model)
    {
        var viewDto = AutoMapModelToViewDto(model);
        if (model == null) return viewDto;
        viewDto!.LectureLessonBatchInfos = model.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Lecture).Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        viewDto.LabLessonBatchInfos = model.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Lab).Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        viewDto.PracticeLessonBatchInfos = model.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Practice).Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        viewDto.ExamLessonBatchInfos = model.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Exam).Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        viewDto.TestLessonBatchInfos = model.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Test).Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        return viewDto;
    }

    [UserMapping(Default = true)]
    public static AcademicDiscipline? MapSaveDtoToModel(AcademicDisciplineSaveDto? dto)
    {
        var model = AutoMapSaveDtoToModel(dto);
        if (dto == null) return model;
        model!.LessonBatchInfos = dto.LectureLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Lecture;
                return y;
            })
            .Concat(dto.LabLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Lab;
                return y;
            }))
            .Concat(dto.PracticeLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Practice;
                return y;
            }))
            .Concat(dto.ExamLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Exam;
                return y;
            }))
            .Concat(dto.TestLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Test;
                return y;
            }))
            .ToArray();
        return model;
    }

    [UserMapping(Default = true)]
    public static void UpdateModelWithSaveDto(AcademicDisciplineSaveDto? dto, AcademicDiscipline? model)
    {
        AutoUpdateModelWithSaveDto(dto, model);
        if (dto == null) return;
        model!.LessonBatchInfos = dto.LectureLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Lecture;
                return y;
            })
            .Concat(dto.LabLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Lab;
                return y;
            }))
            .Concat(dto.PracticeLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Practice;
                return y;
            }))
            .Concat(dto.ExamLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Exam;
                return y;
            }))
            .Concat(dto.TestLessonBatchInfos.Select(x =>
            {
                var y = LessonBatchInfoDtoMappingRegister.MapSaveDtoToModel(x)!;
                y.Type = AcademicDisciplineType.Test;
                return y;
            }))
            .ToArray();
    }

    [UserMapping(Default = true)]
    public static AcademicDisciplineRegistryItemDto? MapItemToItemDto(AcademicDisciplineRegistryItem? item)
    {
        var itemDto = AutoMapItemToItemDto(item);
        if (item == null) return itemDto;
        itemDto!.LectureLessonBatchInfos = item.LectureLessonBatchInfos.Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        itemDto.LabLessonBatchInfos = item.LabLessonBatchInfos.Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        itemDto.PracticeLessonBatchInfos = item.PracticeLessonBatchInfos.Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        itemDto.ExamLessonBatchInfos = item.ExamLessonBatchInfos.Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        itemDto.TestLessonBatchInfos = item.TestLessonBatchInfos.Select(LessonBatchInfoDtoMappingRegister.MapModelToDto).ToArray()!;
        return itemDto;
    }

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AssociatedNames))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.SemesterNumber))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AcademicDisciplineTargetType))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Comment))]
    public static partial AcademicDisciplineShortDto? MapModelToShortDto(AcademicDiscipline? model);

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineViewDto.LectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineViewDto.LabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineViewDto.PracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineViewDto.ExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineViewDto.TestLessonBatchInfos))]
    private static partial AcademicDisciplineViewDto? AutoMapModelToViewDto(AcademicDiscipline? model);

    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.LectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.LabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.PracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.ExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.TestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.LessonBatchInfos))]
    private static partial AcademicDiscipline? AutoMapSaveDtoToModel(AcademicDisciplineSaveDto? dto);

    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.LectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.LabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.PracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.ExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineSaveDto.TestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.LessonBatchInfos))]
    private static partial void AutoUpdateModelWithSaveDto(AcademicDisciplineSaveDto? dto, AcademicDiscipline? model);

    [MapperIgnoreSource(nameof(AcademicDisciplineRegistryItem.LectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineRegistryItem.LabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineRegistryItem.PracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineRegistryItem.ExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDisciplineRegistryItem.TestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItemDto.LectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItemDto.LabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItemDto.PracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItemDto.ExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItemDto.TestLessonBatchInfos))]
    private static partial AcademicDisciplineRegistryItemDto? AutoMapItemToItemDto(AcademicDisciplineRegistryItem? item);
}