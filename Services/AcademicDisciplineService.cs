using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.LessonBatchInfo;
using Dal.Repositories.Schedules;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Mapping;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class AcademicDisciplineService(
    IAcademicDisciplineRepository academicDisciplineRepository,
    ILessonBatchInfoRepository lessonBatchInfoRepository,
    IAcademicDisciplineRegistryRepository academicDisciplineRegistryRepository,
    IScheduleRepository scheduleRepository,
    ILessonService lessonService,
    ILessonValidationService lessonValidationService) : IAcademicDisciplineService
{
    public async Task<AcademicDisciplineShortDto[]> SearchShortAsync(Guid scheduleId)
    {
        var items = await academicDisciplineRepository.SearchAsync(
            new AcademicDisciplineSearchModel { ScheduleId = scheduleId });
        return items.Select(AcademicDisciplineDtoMappingRegister.MapModelToShortDto).ToArray()!;
    }

    public async Task<RegistryDto<AcademicDisciplineRegistryItemDto>> SearchAsync(
        AcademicDisciplineRegistrySearchModel searchModel)
    {
        var registryEntries =
            await academicDisciplineRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<AcademicDisciplineRegistryItemDto>
        {
            Items = registryEntries.Items.Select(AcademicDisciplineDtoMappingRegister.MapItemToItemDto).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<AcademicDisciplineViewDto> GetViewAsync(Guid academicDisciplineId)
    {
        var academicDiscipline = await academicDisciplineRepository.GetAsync(academicDisciplineId);
        return AcademicDisciplineDtoMappingRegister.MapModelToViewDto(academicDiscipline)!;
    }

    public async Task SaveAsync(AcademicDisciplineSaveDto academicDisciplineSaveDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (academicDisciplineSaveDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }

        if (academicDisciplineSaveDto.Id.HasValue
            && !await academicDisciplineRepository.ExistsAsync(academicDisciplineSaveDto.Id!.Value))
        {
            validationMessages.Add(new ValidationMessage("Не найдена академическая дисциплина для обновления"));
        }

        if (!await scheduleRepository.ExistsAsync(academicDisciplineSaveDto.ScheduleId))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден проект расписания для сохранения академической дисциплины"));
        }

        academicDisciplineSaveDto.AllowedLessonTypes =
            academicDisciplineSaveDto.AllowedLessonTypes.Distinct().ToArray();
        var allLessonTypes = new[]
        {
            AcademicDisciplineType.Lecture,
            AcademicDisciplineType.Practice,
            AcademicDisciplineType.Lab,
            AcademicDisciplineType.Exam,
            AcademicDisciplineType.Test,
        };
        var notAllowedLessonTypes = allLessonTypes
            .Except(academicDisciplineSaveDto.AllowedLessonTypes)
            .ToArray();

        var specifiedBatchInfos = new[]
        {
            (IsSpecified: academicDisciplineSaveDto.LectureLessonBatchInfos.Length > 0,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Lecture),
                Type: AcademicDisciplineType.Lecture),
            (IsSpecified: academicDisciplineSaveDto.PracticeLessonBatchInfos.Length > 0,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Practice),
                Type: AcademicDisciplineType.Practice),
            (IsSpecified: academicDisciplineSaveDto.LabLessonBatchInfos.Length > 0,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Lab),
                Type: AcademicDisciplineType.Lab),
            (IsSpecified: academicDisciplineSaveDto.ExamLessonBatchInfos.Length > 0,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Exam),
                Type: AcademicDisciplineType.Exam),
            (IsSpecified: academicDisciplineSaveDto.TestLessonBatchInfos.Length > 0,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Test),
                Type: AcademicDisciplineType.Test),
        };

        validationMessages.AddRange(specifiedBatchInfos
            .Where(x => x is { IsSpecified: true, IsNotAllowed: true })
            .Select(specifiedBatchInfo =>
                new ValidationMessage($"Дисциплина не может содержать дополнительную информацию по занятиям вида " +
                                      $"\"{specifiedBatchInfo.Type.GetDescription()}\", если она не подразумевает их проведение")));

        var batchesStudentGroupIds = new[]
        {
            (Type: AcademicDisciplineType.Lecture,
                StudentGroupIds: academicDisciplineSaveDto.LectureLessonBatchInfos.SelectMany(x => x.StudentGroups)),
            (Type: AcademicDisciplineType.Practice,
                StudentGroupIds: academicDisciplineSaveDto.PracticeLessonBatchInfos.SelectMany(x => x.StudentGroups)),
            (Type: AcademicDisciplineType.Lab,
                StudentGroupIds: academicDisciplineSaveDto.LabLessonBatchInfos.SelectMany(x => x.StudentGroups)),
            (Type: AcademicDisciplineType.Exam,
                StudentGroupIds: academicDisciplineSaveDto.ExamLessonBatchInfos.SelectMany(x => x.StudentGroups)),
            (Type: AcademicDisciplineType.Test,
                StudentGroupIds: academicDisciplineSaveDto.TestLessonBatchInfos.SelectMany(x => x.StudentGroups)),
        };

        validationMessages.AddRange(batchesStudentGroupIds
            .Where(x => x.StudentGroupIds.Distinct().Count() != x.StudentGroupIds.Count())
            .Select(batchStudentGroupIds =>
                new ValidationMessage($"Наборы занятий вида \"{batchStudentGroupIds.Type.GetDescription()}\" " +
                                      $"не должны иметь общие группы")));

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        AcademicDiscipline academicDiscipline;
        var id = academicDisciplineSaveDto.Id;
        if (id.HasValue)
        {
            academicDiscipline = AcademicDisciplineDtoMappingRegister.MapSaveDtoToModel(academicDisciplineSaveDto)!;
            foreach (var lessonType in allLessonTypes)
            {
                var ids = await lessonBatchInfoRepository.SaveAllAsync(academicDiscipline.GetBatchInfosByType(lessonType).ToArray());
                switch (lessonType)
                {
                    case AcademicDisciplineType.Lecture:
                        academicDiscipline.LectureLessonBatchInfos = ids.Select(x => new LessonBatchInfo { Id = x }).ToArray();
                        break;
                    case AcademicDisciplineType.Practice:
                        academicDiscipline.PracticeLessonBatchInfos = ids.Select(x => new LessonBatchInfo { Id = x }).ToArray();
                        break;
                    case AcademicDisciplineType.Lab:
                        academicDiscipline.LabLessonBatchInfos = ids.Select(x => new LessonBatchInfo { Id = x }).ToArray();
                        break;
                    case AcademicDisciplineType.Exam:
                        academicDiscipline.ExamLessonBatchInfos = ids.Select(x => new LessonBatchInfo { Id = x }).ToArray();
                        break;
                    case AcademicDisciplineType.Test:
                        academicDiscipline.TestLessonBatchInfos = ids.Select(x => new LessonBatchInfo { Id = x }).ToArray();
                        break;
                }
            }
            academicDiscipline = await academicDisciplineRepository.GetAsync(id.Value);
            AcademicDisciplineDtoMappingRegister.UpdateModelWithSaveDto(academicDisciplineSaveDto, academicDiscipline);
            await academicDisciplineRepository.SaveAsync(academicDiscipline);
        }
        else
        {
            academicDiscipline = AcademicDisciplineDtoMappingRegister.MapSaveDtoToModel(academicDisciplineSaveDto)!;
            id = await academicDisciplineRepository.SaveAsync(academicDiscipline);
        }

        // var academicDiscipline = AcademicDisciplineDtoMappingRegister.MapSaveDtoToModel(academicDisciplineSaveDto)!;
        // var schedule = await scheduleRepository.GetAsync(academicDisciplineSaveDto.ScheduleId);
        // academicDiscipline.Schedule = schedule;

        // var id = await academicDisciplineRepository.SaveAsync(academicDiscipline);
        await lessonValidationService.RemovePolicyViolations(id!.Value);
        var savedAcademicDiscipline = await academicDisciplineRepository.GetAsync(id!.Value);

        await lessonService.UpdateAcademicDisciplineLessons(savedAcademicDiscipline);

        if (academicDisciplineSaveDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForUpdatedAcademicDiscipline(savedAcademicDiscipline);
        }
    }

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Guid academicDisciplineId,
        AcademicDisciplineType academicDisciplineType, Guid lessonBatchInfoId)
    {
        var academicDiscipline = await academicDisciplineRepository.GetAsync(academicDisciplineId);
        if (!academicDiscipline.AllowedLessonTypes.Contains(academicDisciplineType))
        {
            throw new ServiceException(new ValidationMessage($"Выбранная академическая дисциплина не поддерживает проведение занятий вида \"{academicDisciplineType.GetDescription()}\""));
        }

        var lessonBatchInfo = academicDiscipline.GetBatchInfosByType(academicDisciplineType).Single(x => x.Id == lessonBatchInfoId);
        return await lessonService.GetLessonSeriesConflictsAsync(lessonBatchInfo, academicDiscipline.ScheduleId);
    }

    public async Task DeleteAsync(Guid academicDisciplineId)
    {
        await academicDisciplineRepository.DeleteAsync(academicDisciplineId);
    }
}