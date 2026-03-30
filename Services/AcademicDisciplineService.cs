using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Schedules;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Mapping;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
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
    public async Task<RegistryDto<AcademicDisciplineRegistryItemDto>> SearchAsync(
        AcademicDisciplineRegistrySearchModel searchModel)
    {
        var registryEntries =
            await academicDisciplineRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<AcademicDisciplineRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
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

        if (saveAcademicDisciplineDto.SemesterNumber is < 1 or > 8)
        {
            validationMessages.Add(new ValidationMessage(
                $"Указанный номер семестра ({saveAcademicDisciplineDto.SemesterNumber}) должен лежать в интервале от 1 до 8"));
        }

        saveAcademicDisciplineDto.AllowedLessonTypes =
            saveAcademicDisciplineDto.AllowedLessonTypes.Distinct().ToArray();
        var unspecifiedLessonTypes = new[]
            {
                AcademicDisciplineType.Lecture,
                AcademicDisciplineType.Practice,
                AcademicDisciplineType.Lab
            }
            .Except(saveAcademicDisciplineDto.AllowedLessonTypes)
            .ToArray();
        var specifiedPayloadsPairs = new[]
        {
            (saveAcademicDisciplineDto.LecturePayload != null,
                unspecifiedLessonTypes.Contains(AcademicDisciplineType.Lecture),
                AcademicDisciplineType.Lecture),
            (saveAcademicDisciplineDto.PracticePayload != null,
                unspecifiedLessonTypes.Contains(AcademicDisciplineType.Practice),
                AcademicDisciplineType.Practice),
            (saveAcademicDisciplineDto.LabPayload != null,
                unspecifiedLessonTypes.Contains(AcademicDisciplineType.Lab),
                AcademicDisciplineType.Lab),
        };

        foreach (var unspecifiedLessonType in specifiedPayloadsPairs.Where(x => x is { Item1: true, Item2: true }))
        {
            validationMessages.Add(
                new ValidationMessage($"Дисциплина не может содержать дополнительную информацию по занятиям вида " +
                                      $"\"{unspecifiedLessonType.Item3.GetDescription()}\", если она не подразумевает их проведение"));
        }

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var academicDiscipline = DtoMappingRegister.Map(saveAcademicDisciplineDto)!;
        if (saveAcademicDisciplineDto.Id.HasValue)
        {
            await lessonService.UpdateAcademicDisciplineLessons(academicDiscipline);
        }

        await academicDisciplineRepository.SaveAsync(academicDiscipline);
        if (saveAcademicDisciplineDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForUpdatedAcademicDiscipline(academicDiscipline);
        }
    }
}