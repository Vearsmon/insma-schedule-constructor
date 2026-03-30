using Dal.RegistryRepositories.TeacherPreference;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Mapping;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class TeacherPreferenceService(
    ITeacherPreferenceRepository teacherPreferenceRepository,
    ITeacherPreferenceRegistryRepository teacherPreferenceRegistryRepository,
    IScheduleRepository scheduleRepository,
    ITeacherRepository teacherRepository,
    IRoomRepository roomRepository,
    ILessonService lessonService) : ITeacherPreferenceService
{
    public async Task<RegistryDto<TeacherPreferenceRegistryItemDto>> SearchAsync(
        TeacherPreferenceRegistrySearchModel searchModel)
    {
        var registryEntries =
            await teacherPreferenceRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<TeacherPreferenceRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<TeacherPreferenceViewDto> GetViewAsync(Guid teacherId, Guid scheduleId)
    {
        var teacherPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = scheduleId,
            TeacherId = teacherId,
        });
        return new TeacherPreferenceViewDto
        {
            TeacherTimeAvailabilities = teacherPreferences
                .Where(x => x.DayOfWeekTimeInterval != null)
                .Select(x => new TeacherTimeAvailabilityDto
                {
                    DayOfWeekTimeInterval = x.DayOfWeekTimeInterval!,
                    TeacherPreferenceType = x.TeacherPreferenceType!.Value,
                })
                .ToArray(),
            TeacherRoomPreferences = teacherPreferences
                .Where(x => x.RoomId != null)
                .Select(x => new TeacherRoomPreferenceDto
                {
                    RoomId = x.RoomId!.Value,
                    TeacherPreferenceType = x.TeacherPreferenceType!.Value,
                })
                .ToArray(),
            Comment = teacherPreferences.First(x => x.Comment != null).Comment,
        };
    }

    public async Task SaveAsync(SaveTeacherPreferenceDto saveTeacherPreferenceDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (!(await scheduleRepository.ExistsAsync(saveTeacherPreferenceDto.ScheduleId)))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден проект расписания для сохранения пожеланий преподавателя"));
        }

        if (!(await teacherRepository.ExistsAsync(saveTeacherPreferenceDto.TeacherId)))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден преподаватель для сохранения пожеланий преподавателя"));
        }

        var mergedTimeAvailabilities = new List<TeacherTimeAvailabilityDto>();
        var mergedTimeAvailabilitiesByDayOfWeek =
            new Dictionary<DayOfWeek, List<(TeacherPreferenceType, List<TimeInterval>)>>();
        var dayOfWeekTimeIntervalsByPreferenceType = saveTeacherPreferenceDto.TeacherTimeAvailabilities
            .GroupBy(x => x.TeacherPreferenceType);
        foreach (var groupByPreferenceType in dayOfWeekTimeIntervalsByPreferenceType)
        {
            var timeIntervalsByDayOfWeek = groupByPreferenceType
                .GroupBy(x => x.DayOfWeekTimeInterval.DayOfWeek);
            foreach (var groupByDayOfWeek in timeIntervalsByDayOfWeek)
            {
                var unmergedIntervals = groupByDayOfWeek
                    .Select(x => x.DayOfWeekTimeInterval.TimeInterval)
                    .ToArray();
                var mergedIntervals = unmergedIntervals.MergeIntersections();

                if (!mergedTimeAvailabilitiesByDayOfWeek.TryGetValue(groupByDayOfWeek.Key,
                        out var timeAvailabilities))
                {
                    timeAvailabilities = [];
                    mergedTimeAvailabilitiesByDayOfWeek[groupByDayOfWeek.Key] = timeAvailabilities;
                }

                timeAvailabilities.Add((groupByPreferenceType.Key, mergedIntervals.ToList()));

                mergedTimeAvailabilities.AddRange(mergedIntervals
                    .Select(mergedInterval => new TeacherTimeAvailabilityDto
                    {
                        TeacherPreferenceType = groupByPreferenceType.Key,
                        DayOfWeekTimeInterval = new DayOfWeekTimeInterval(groupByDayOfWeek.Key, mergedInterval)
                    }));
            }
        }

        foreach (var (dayOfWeek, preferenceTypeTimeIntervalsPairs) in mergedTimeAvailabilitiesByDayOfWeek)
        {
            for (var i = 0; i < preferenceTypeTimeIntervalsPairs.Count - 1; i++)
            {
                for (var j = i + 1; j < preferenceTypeTimeIntervalsPairs.Count; j++)
                {
                    foreach (var firstTimeInterval in preferenceTypeTimeIntervalsPairs[i].Item2)
                    {
                        foreach (var secondTimeInterval in preferenceTypeTimeIntervalsPairs[j].Item2)
                        {
                            if (firstTimeInterval.HasIntersection(secondTimeInterval))
                            {
                                validationMessages.Add(
                                    new ValidationMessage($"Отрезок времени {firstTimeInterval} с видом пожелания" +
                                                          $" \"{preferenceTypeTimeIntervalsPairs[i].Item1.GetDescription()}\"" +
                                                          $" пересекается с отрезком времени {secondTimeInterval} с видом пожелания " +
                                                          $"\"{preferenceTypeTimeIntervalsPairs[j].Item1.GetDescription()}\""));
                            }
                        }
                    }
                }
            }
        }

        var teacherPreferenceRoomIds = saveTeacherPreferenceDto.TeacherRoomPreferences
            .Select(x => x.RoomId)
            .Distinct()
            .ToArray();
        if (teacherPreferenceRoomIds.Length != 0)
        {
            var rooms = await roomRepository.SelectAsync(teacherPreferenceRoomIds);
            if (rooms.Length != teacherPreferenceRoomIds.Length)
            {
                validationMessages.Add(
                    new ValidationMessage("Не найдены аудитории для сохранения пожеланий преподавателя"));
            }
        }

        var teacherRoomPreferencesByType = saveTeacherPreferenceDto.TeacherRoomPreferences
            .GroupBy(x => x.TeacherPreferenceType)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.RoomId).ToArray())
            .ToArray();
        for (var i = 0; i < teacherRoomPreferencesByType.Length - 1; i++)
        {
            for (var j = i + 1; j < teacherRoomPreferencesByType.Length; j++)
            {
                if (teacherRoomPreferencesByType[i].Value.Intersect(teacherRoomPreferencesByType[j].Value).Any())
                {
                    validationMessages.Add(
                        new ValidationMessage($"Одна и та же аудитория не может быть указана для пожеланий вида " +
                                              $"\"{teacherRoomPreferencesByType[i].Key.GetDescription()}\" " +
                                              $"и \"{teacherRoomPreferencesByType[j].Key.GetDescription()}\""));
                }
            }
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var teacherPreferences =
            mergedTimeAvailabilities
                .Select(x => new TeacherPreference
                {
                    ScheduleId = saveTeacherPreferenceDto.ScheduleId,
                    TeacherId = saveTeacherPreferenceDto.TeacherId,
                    DayOfWeekTimeInterval = x.DayOfWeekTimeInterval,
                    TeacherPreferenceType = x.TeacherPreferenceType,
                })
                .Concat(saveTeacherPreferenceDto.TeacherRoomPreferences
                    .Select(x => new TeacherPreference
                    {
                        ScheduleId = saveTeacherPreferenceDto.ScheduleId,
                        TeacherId = saveTeacherPreferenceDto.TeacherId,
                        TeacherPreferenceType = x.TeacherPreferenceType,
                        RoomId = x.RoomId,
                    }));
        if (saveTeacherPreferenceDto.Comment != null)
        {
            teacherPreferences = teacherPreferences.Concat([
                new TeacherPreference
                {
                    ScheduleId = saveTeacherPreferenceDto.ScheduleId,
                    TeacherId = saveTeacherPreferenceDto.TeacherId,
                    Comment = saveTeacherPreferenceDto.Comment,
                }
            ]);
        }

        var previousPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = saveTeacherPreferenceDto.ScheduleId,
            TeacherId = saveTeacherPreferenceDto.TeacherId,
        });

        await teacherPreferenceRepository.DeleteAsync(previousPreferences.Select(x => x.Id!.Value).ToArray());
        var teacherPreferencesArray = teacherPreferences.ToArray();
        if (teacherPreferencesArray.Length > 0)
        {
            await lessonService.RecalculateConflictsForNewTeacherPreferences(teacherPreferencesArray);
            await teacherPreferenceRepository.SaveAllAsync(teacherPreferencesArray);
        }
    }
}