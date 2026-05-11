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
        await ValidateAsync(academicDisciplineSaveDto);

        AcademicDiscipline academicDiscipline;
        var id = academicDisciplineSaveDto.Id;
        if (id.HasValue)
        {
            academicDiscipline = await academicDisciplineRepository.GetAsync(id.Value);
            AcademicDisciplineDtoMappingRegister.UpdateModelWithSaveDto(academicDisciplineSaveDto, academicDiscipline);
            await academicDisciplineRepository.SaveAsync(academicDiscipline);
        }
        else
        {
            academicDiscipline = AcademicDisciplineDtoMappingRegister.MapSaveDtoToModel(academicDisciplineSaveDto)!;
            id = await academicDisciplineRepository.SaveAsync(academicDiscipline);
        }

        await lessonValidationService.RemovePolicyViolations(id.Value);
        var savedAcademicDiscipline = await academicDisciplineRepository.GetAsync(id.Value);

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

    private async Task ValidateAsync(AcademicDisciplineSaveDto academicDisciplineSaveDto)
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

        academicDisciplineSaveDto.AllowedLessonTypes = academicDisciplineSaveDto.AllowedLessonTypes.Distinct().ToArray();
        var availableTypes = Enum.GetValues<AcademicDisciplineType>();

        var academicDiscipline = AcademicDisciplineDtoMappingRegister.MapSaveDtoToModel(academicDisciplineSaveDto)!;

        validationMessages.AddRange(availableTypes
            .Where(type => academicDiscipline.GetBatchInfosByType(type).Length > 0
                           && availableTypes.Except(academicDiscipline.AllowedLessonTypes).Contains(type))
            .Select(type =>
                new ValidationMessage($"Дисциплина не может содержать дополнительную информацию по занятиям вида " +
                                      $"\"{type.GetDescription()}\", если она не подразумевает их проведение")));

        validationMessages.AddRange(availableTypes
            .Where(type =>
            {
                var studentGroupIds = academicDiscipline.GetBatchInfosByType(type)
                    .SelectMany(lessonBatchInfo => lessonBatchInfo.StudentGroups.Select(studentGroup => studentGroup.Id))
                    .ToArray();
                return studentGroupIds.Distinct().Count() != studentGroupIds.Length;
            })
            .Select(type => new ValidationMessage(
                $"Наборы занятий вида \"{type.GetDescription()}\" не должны иметь общие группы")));

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }
    }
}