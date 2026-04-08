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
        return items.Select(DtoMappingRegister.MapToRootDto).ToArray()!;
    }

    public async Task<RegistryDto<AcademicDisciplineRegistryItemDto>> SearchAsync(
        AcademicDisciplineRegistrySearchModel searchModel)
    {
        var registryEntries =
            await academicDisciplineRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<AcademicDisciplineRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<AcademicDisciplineViewDto> GetViewAsync(Guid academicDisciplineId)
    {
        var academicDiscipline = await academicDisciplineRepository.GetAsync(academicDisciplineId);
        return DtoMappingRegister.Map(academicDiscipline)!;
    }

    public async Task SaveAsync(SaveAcademicDisciplineDto saveAcademicDisciplineDto)
    {
        // todo: провалидировать, что все указанные группы находятся на одном уровне
        var validationMessages = new List<ValidationMessage>();
        if (saveAcademicDisciplineDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }

        if (saveAcademicDisciplineDto.Cypher == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие шифра"));
        }

        if (saveAcademicDisciplineDto.Id.HasValue
            && !(await academicDisciplineRepository.ExistsAsync(saveAcademicDisciplineDto.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найдена академическая дисциплина для обновления"));
        }

        if (!(await scheduleRepository.ExistsAsync(saveAcademicDisciplineDto.ScheduleId)))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден проект расписания для сохранения академической дисциплины"));
        }

        if (saveAcademicDisciplineDto.SemesterNumber is < 1 or > 12)
        {
            validationMessages.Add(new ValidationMessage(
                $"Указанный номер семестра ({saveAcademicDisciplineDto.SemesterNumber}) должен лежать в интервале от 1 до 12"));
        }

        saveAcademicDisciplineDto.AllowedLessonTypes =
            saveAcademicDisciplineDto.AllowedLessonTypes.Distinct().ToArray();
        var notAllowedLessonTypes = new[]
            {
                AcademicDisciplineType.Lecture,
                AcademicDisciplineType.Practice,
                AcademicDisciplineType.Lab
            }
            .Except(saveAcademicDisciplineDto.AllowedLessonTypes)
            .ToArray();
        var specifiedPayloads = new[]
        {
            (IsSpecified: saveAcademicDisciplineDto.LecturePayload != null,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Lecture),
                Type: AcademicDisciplineType.Lecture),
            (IsSpecified: saveAcademicDisciplineDto.PracticePayload != null,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Practice),
                Type: AcademicDisciplineType.Practice),
            (IsSpecified: saveAcademicDisciplineDto.LabPayload != null,
                IsNotAllowed: notAllowedLessonTypes.Contains(AcademicDisciplineType.Lab),
                Type: AcademicDisciplineType.Lab),
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

        var academicDiscipline = DtoMappingRegister.Map(saveAcademicDisciplineDto)!;
        await lessonService.UpdateAcademicDisciplineLessons(academicDiscipline);

        await academicDisciplineRepository.SaveAsync(academicDiscipline);
        if (saveAcademicDisciplineDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForUpdatedAcademicDiscipline(academicDiscipline);
        }
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

        return await lessonService.GetLessonSeriesConflictsAsync(academicDiscipline.Id!.Value, payload!.LessonBatchInfo!, academicDisciplineType, academicDiscipline.ScheduleId);
    }

    public async Task<string[]> SearchCyphersAsync(Guid scheduleId)
    {
        return await academicDisciplineRepository.SearchCyphersAsync(scheduleId);
    }

    public async Task DeleteAsync(Guid academicDisciplineId)
    {
        await academicDisciplineRepository.DeleteAsync(academicDisciplineId);
    }
}