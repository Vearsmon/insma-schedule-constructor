using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Schedules;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Mapping;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class AcademicDisciplineService(
    IAcademicDisciplineRepository academicDisciplineRepository,
    IAcademicDisciplineRegistryRepository academicDisciplineRegistryRepository,
    IScheduleRepository scheduleRepository,
    ILessonService lessonService) : IAcademicDisciplineService
{
    public async Task<AcademicDisciplineShortDto[]> SearchShortAsync(Guid scheduleId)
    {
        var items = await academicDisciplineRepository.SearchAsync(
            new AcademicDisciplineSearchModel { ScheduleId = scheduleId });
        return items.Select(AcademicDisciplineDtoMappingRegister.MapToRootDto).ToArray()!;
    }

    public async Task<RegistryDto<AcademicDisciplineRegistryItemDto>> SearchAsync(
        AcademicDisciplineRegistrySearchModel searchModel)
    {
        var registryEntries =
            await academicDisciplineRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<AcademicDisciplineRegistryItemDto>
        {
            Items = registryEntries.Items.Select(AcademicDisciplineDtoMappingRegister.Map).ToArray()!,
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
            && !(await academicDisciplineRepository.ExistsAsync(academicDisciplineSaveDto.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найдена академическая дисциплина для обновления"));
        }

        if (!(await scheduleRepository.ExistsAsync(academicDisciplineSaveDto.ScheduleId)))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден проект расписания для сохранения академической дисциплины"));
        }

        academicDisciplineSaveDto.AllowedLessonTypes =
            academicDisciplineSaveDto.AllowedLessonTypes.Distinct().ToArray();
        var notAllowedLessonTypes = new[]
            {
                AcademicDisciplineType.Lecture,
                AcademicDisciplineType.Practice,
                AcademicDisciplineType.Lab,
                AcademicDisciplineType.Exam,
                AcademicDisciplineType.Test,
            }
            .Except(academicDisciplineSaveDto.AllowedLessonTypes)
            .ToArray();

        var specifiedPayloads = new[]
        {
            (IsSpecified: IsSpecified(academicDisciplineSaveDto.LecturePayload),
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Lecture),
                Type: AcademicDisciplineType.Lecture),
            (IsSpecified: IsSpecified(academicDisciplineSaveDto.PracticePayload),
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Practice),
                Type: AcademicDisciplineType.Practice),
            (IsSpecified: IsSpecified(academicDisciplineSaveDto.LabPayload),
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Lab),
                Type: AcademicDisciplineType.Lab),
            (IsSpecified: IsSpecified(academicDisciplineSaveDto.ExamPayload),
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Exam),
                Type: AcademicDisciplineType.Exam),
            (IsSpecified: IsSpecified(academicDisciplineSaveDto.TestPayload),
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Test),
                Type: AcademicDisciplineType.Test),
        };

        validationMessages.AddRange(specifiedPayloads
            .Where(x => x is { IsSpecified: true, IsNotAllowed: true })
            .Select(specifiedPayload =>
                new ValidationMessage($"Дисциплина не может содержать дополнительную информацию по занятиям вида " +
                                      $"\"{specifiedPayload.Type.GetDescription()}\", если она не подразумевает их проведение")));

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var academicDiscipline = AcademicDisciplineDtoMappingRegister.MapSaveDtoToModel(academicDisciplineSaveDto)!;
        await lessonService.UpdateAcademicDisciplineLessons(academicDiscipline);

        await academicDisciplineRepository.SaveAsync(academicDiscipline);
        if (academicDisciplineSaveDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForUpdatedAcademicDiscipline(academicDiscipline);
        }

        return;

        bool IsSpecified(AcademicDisciplinePayloadDto? payload) =>
            payload != null&& (payload.LessonBatchInfos.Length > 0 || payload.TotalHoursCount != 0);
    }

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Guid academicDisciplineId,
        AcademicDisciplineType academicDisciplineType)
    {
        var academicDiscipline = await academicDisciplineRepository.GetAsync(academicDisciplineId);
        if (!academicDiscipline.AllowedLessonTypes.Contains(academicDisciplineType))
        {
            throw new ServiceException(new ValidationMessage($"Выбранная академическая дисциплина не поддерживает проведение занятий вида \"{academicDisciplineType.GetDescription()}\""));
        }

        var payload = academicDiscipline.GetPayloadByType(academicDisciplineType);

        var result = new List<LessonSeriesConflictDto>();
        foreach (var lessonBatchInfo in payload!.LessonBatchInfos)
        {
            result.AddRange(await lessonService.GetLessonSeriesConflictsAsync(academicDiscipline.Id!.Value, lessonBatchInfo, academicDisciplineType, academicDiscipline.ScheduleId));
        }
        return result.ToArray();
    }

    public async Task DeleteAsync(Guid academicDisciplineId)
    {
        await academicDisciplineRepository.DeleteAsync(academicDisciplineId);
    }
}