using Dal.RegistryRepositories.TeacherPreference;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
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
            Items = registryEntries.Items.Select(TeacherPreferenceDtoMappingRegister.MapItemToItemDto).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<TeacherPreferenceViewDto> GetViewAsync(Guid teacherId, Guid scheduleId)
    {
        var teacherPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = scheduleId,
            TeacherIds = [teacherId],
        });
        return new TeacherPreferenceViewDto
        {
            TeacherTimePreferences = teacherPreferences
                .Where(x => x.DayOfWeekTimeInterval != null)
                .Select(x => new TeacherTimePreferenceViewDto
                {
                    DayOfWeekTimeInterval = x.DayOfWeekTimeInterval!,
                    TeacherPreferenceType = x.TeacherPreferenceType!.Value,
                })
                .ToArray(),
            TeacherRoomPreferences = teacherPreferences
                .Where(x => x.RoomId != null)
                .Select(x => new TeacherRoomPreferenceViewDto
                {
                    RoomId = x.RoomId!.Value,
                    RoomName = $"{x.Room!.Campus.Name} - {x.Room!.Name}",
                    TeacherPreferenceType = x.TeacherPreferenceType!.Value,
                })
                .ToArray(),
            Comment = teacherPreferences.SingleOrDefault(x => x.Comment != null)?.Comment,
        };
    }

    public async Task SaveAsync(TeacherPreferenceSaveDto teacherPreferenceSaveDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (!await scheduleRepository.ExistsAsync(teacherPreferenceSaveDto.ScheduleId))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден проект расписания для сохранения пожеланий преподавателя"));
        }

        if (!await teacherRepository.ExistsAsync(teacherPreferenceSaveDto.TeacherId))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден преподаватель для сохранения пожеланий преподавателя"));
        }

        var mergedTimeAvailabilities = new List<TeacherTimePreferenceViewDto>();
        var mergedTimeAvailabilitiesByDayOfWeek = new Dictionary<DayOfWeek, List<(TeacherPreferenceType, IEnumerable<TimeInterval>)>>();

        var grouped = teacherPreferenceSaveDto.TeacherTimePreferences
            .GroupBy(x => new { x.TeacherPreferenceType, x.DayOfWeekTimeInterval.DayOfWeek });
        foreach (var group in grouped)
        {
            var mergedIntervals = group
                .Select(x => x.DayOfWeekTimeInterval.TimeInterval)
                .MergeIntersections();

            if (!mergedTimeAvailabilitiesByDayOfWeek.TryGetValue(group.Key.DayOfWeek, out var timeAvailabilities))
            {
                timeAvailabilities = [];
                mergedTimeAvailabilitiesByDayOfWeek[group.Key.DayOfWeek] = timeAvailabilities;
            }
            timeAvailabilities.Add((group.Key.TeacherPreferenceType, mergedIntervals));

            mergedTimeAvailabilities.AddRange(
                mergedIntervals.Select(interval => new TeacherTimePreferenceViewDto
                {
                    TeacherPreferenceType = group.Key.TeacherPreferenceType,
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval
                    {
                        DayOfWeek = group.Key.DayOfWeek,
                        TimeInterval = interval
                    }
                }));
        }

        var messages = mergedTimeAvailabilitiesByDayOfWeek.Values
            .SelectMany(pairs => pairs
                .SelectMany((pair1, i) => pairs
                    .Skip(i + 1)
                    .SelectMany(pair2 =>
                        pair1.Item2.SelectMany(interval1 =>
                            pair2.Item2.Where(interval1.HasIntersection)
                                .Select(interval2 =>
                                    new ValidationMessage(
                                        $"Отрезок времени {interval1} с видом пожелания \"{pair1.Item1.GetDescription()}\" " +
                                        $"пересекается с отрезком времени {interval2} с видом пожелания \"{pair2.Item1.GetDescription()}\""))))));

        validationMessages.AddRange(messages);

        var teacherPreferenceRoomIds = teacherPreferenceSaveDto.TeacherRoomPreferences
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

        var teacherRoomPreferencesByType = teacherPreferenceSaveDto.TeacherRoomPreferences
            .GroupBy(x => x.TeacherPreferenceType)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.RoomId).ToArray())
            .ToArray();

        var roomMessages = teacherRoomPreferencesByType
            .SelectMany((typeRoomIdsPair1, i) => teacherRoomPreferencesByType
                .Skip(i + 1)
                .Where(timeRoomIdsPair2 => typeRoomIdsPair1.Value.Intersect(timeRoomIdsPair2.Value).Any())
                .Select(timeRoomIdsPair2 => new ValidationMessage(
                    $"Одна и та же аудитория не может быть указана для пожеланий вида " +
                    $"\"{typeRoomIdsPair1.Key.GetDescription()}\" " +
                    $"и \"{timeRoomIdsPair2.Key.GetDescription()}\"")));
        validationMessages.AddRange(roomMessages);

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var teacherPreferences = mergedTimeAvailabilities
            .Select(x => new TeacherPreference
            {
                ScheduleId = teacherPreferenceSaveDto.ScheduleId,
                TeacherId = teacherPreferenceSaveDto.TeacherId,
                DayOfWeekTimeInterval = x.DayOfWeekTimeInterval,
                TeacherPreferenceType = x.TeacherPreferenceType,
            })
            .Concat(teacherPreferenceSaveDto.TeacherRoomPreferences
                .Select(x => new TeacherPreference
                {
                    ScheduleId = teacherPreferenceSaveDto.ScheduleId,
                    TeacherId = teacherPreferenceSaveDto.TeacherId,
                    TeacherPreferenceType = x.TeacherPreferenceType,
                    RoomId = x.RoomId,
                }))
            .Concat(string.IsNullOrEmpty(teacherPreferenceSaveDto.Comment)
                ? []
                : [new TeacherPreference
                {
                    ScheduleId = teacherPreferenceSaveDto.ScheduleId,
                    TeacherId = teacherPreferenceSaveDto.TeacherId,
                    Comment = teacherPreferenceSaveDto.Comment,
                }]);

        var previousPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = teacherPreferenceSaveDto.ScheduleId,
            TeacherIds = [teacherPreferenceSaveDto.TeacherId],
        });

        await teacherPreferenceRepository.DeleteAsync(previousPreferences.Select(x => x.Id!.Value).ToArray());
        var teacherPreferencesArray = teacherPreferences.ToArray();
        if (teacherPreferencesArray.Length > 0)
        {
            await lessonService.RecalculateConflictsForNewTeacherPreferences(teacherPreferencesArray);
            await teacherPreferenceRepository.SaveAllAsync(teacherPreferencesArray);
        }
    }

    public async Task DeleteAsync(Guid scheduleId, Guid teacherId)
    {
        var teacherPreferences = await teacherPreferenceRepository.SearchAsync(
            new TeacherPreferenceSearchModel
            {
                ScheduleId = scheduleId,
                TeacherIds = [teacherId],
            });
        await teacherRepository.DeleteAsync(teacherPreferences.Select(x => x.Id!.Value).ToArray());
    }
}