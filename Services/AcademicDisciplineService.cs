using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.DayOfWeekTimeIntervalAssignments;
using Dal.Repositories.LessonBatchInfo;
using Dal.Repositories.Lessons;
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
    ILessonRepository lessonRepository,
    ILessonBatchInfoRepository lessonBatchInfoRepository,
    IDayOfWeekTimeIntervalAssignmentRepository dayOfWeekTimeIntervalAssignmentRepository) : IAcademicDisciplineService
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
            academicDiscipline.Id = id;
        }

        var savedLessonBatchInfos = await SaveLessonBatchInfosAsync(academicDiscipline);

        if (savedLessonBatchInfos.Length > 0)
        {
            await lessonService.UpdateLessonsByBatches(academicDiscipline.ScheduleId, savedLessonBatchInfos);
        }
        await lessonService.RecalculateConflictsForUpdatedAcademicDiscipline(academicDiscipline);
    }

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Guid lessonId)
    {
        var validationMessages = new List<ValidationMessage>();
        var lesson = await lessonRepository.GetAsync(lessonId);
        if (!lesson.AcademicDisciplineId.HasValue)
        {
            validationMessages.Add(new ValidationMessage("Для выбранного занятия не была найдена академическая дисциплина"));
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        return await lessonService.GetLessonSeriesConflictsAsync(lesson);
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

        if (academicDiscipline.GetAllBatchInfos().Any(x => x.DayOfWeekTimeIntervals.Length > x.LessonsPerWeekCount))
        {
            validationMessages.Add(new ValidationMessage("При сохранении серии занятий число отрезков времени не может быть больше требуемого количества занятий в неделю"));
        }

        // validationMessages.AddRange(availableTypes
        //     .Where(type =>
        //     {
        //         var lessonBatchInfos = academicDiscipline.GetBatchInfosByType(type);
        //         var studentGroupIds = lessonBatchInfos
        //             .SelectMany(lessonBatchInfo => lessonBatchInfo.StudentGroups.Select(studentGroup => studentGroup.Id!.Value))
        //             .ToArray();
        //         var duplicates = studentGroupIds.Where(x => studentGroupIds.Count(y => y == x) > 1).ToArray();
        //         foreach (var duplicate in duplicates)
        //         {
        //             var dateIntervals = lessonBatchInfos
        //                 .Where(x => x.StudentGroups.Any(y => y.Id == duplicate))
        //                 .Select(x => x.DateInterval)
        //                 .OrderBy(x => x.DateFrom)
        //                 .ToArray();
        //             for (var i = 0; i < dateIntervals.Length - 1; i++)
        //             {
        //                 if (dateIntervals[i].HasIntersection(dateIntervals[i + 1])) return true;
        //             }
        //         }
        //
        //         return false;
        //     })
        //     .Select(type => new ValidationMessage(
        //         $"Наборы занятий вида \"{type.GetDescription()}\" не должны иметь общие группы для одного и того же отрезка времени")));

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }
    }

    private async Task<LessonBatchInfo[]> SaveLessonBatchInfosAsync(AcademicDiscipline academicDiscipline)
    {
        var previousLessonBatchInfos = await lessonBatchInfoRepository.SearchAsync(new LessonBatchInfoSearchModel
        {
            ScheduleId = academicDiscipline.ScheduleId,
            AcademicDisciplineId = academicDiscipline.Id!.Value,
        });

        var toDelete = previousLessonBatchInfos
            .Where(x => academicDiscipline.GetAllBatchInfos().All(y => y.Id != x.Id));
        await lessonBatchInfoRepository.DeleteAsync(toDelete.Select(x => x.Id!.Value).ToArray());

        var lessonBatchInfosToSave = Enum.GetValues<AcademicDisciplineType>()
            .SelectMany(type =>
            {
                var batchInfos = academicDiscipline.GetBatchInfosByType(type);
                foreach (var batchInfo in batchInfos)
                {
                    batchInfo.AcademicDisciplineId = academicDiscipline.Id!.Value;
                    batchInfo.Type = type;
                }
                return batchInfos;
            })
            .ToArray();

        var ids = new List<Guid>();
        foreach (var lessonBatchInfo in lessonBatchInfosToSave)
        {
            var id = await lessonBatchInfoRepository.SaveAsync(lessonBatchInfo);
            ids.Add(id);
            lessonBatchInfo.Id = id;
            lessonBatchInfo.DayOfWeekTimeIntervals = await SaveTimeAssignmentsAsync(lessonBatchInfo);
        }

        return await lessonBatchInfoRepository.SelectAsync(ids.ToArray());
    }

    private async Task<DayOfWeekTimeIntervalAssignment[]> SaveTimeAssignmentsAsync(LessonBatchInfo lessonBatchInfo)
    {
        var previousTimeAssignments = await dayOfWeekTimeIntervalAssignmentRepository.SearchAsync(new DayOfWeekTimeIntervalAssignmentSearchModel
        {
            LessonBatchInfoIds = [lessonBatchInfo.Id!.Value],
        });

        var toDelete = previousTimeAssignments
            .Where(x => lessonBatchInfo.DayOfWeekTimeIntervals
                .All(y => y.Id != x.Id));
        await dayOfWeekTimeIntervalAssignmentRepository.DeleteAsync(toDelete.Select(x => x.Id!.Value).ToArray());

        foreach (var timeAssignment in lessonBatchInfo.DayOfWeekTimeIntervals)
        {
            timeAssignment.LessonBatchInfoId = lessonBatchInfo.Id!.Value;
        }

        var ids = await dayOfWeekTimeIntervalAssignmentRepository.SaveAllAsync(lessonBatchInfo.DayOfWeekTimeIntervals);
        return await dayOfWeekTimeIntervalAssignmentRepository.SelectAsync(ids);
    }
}