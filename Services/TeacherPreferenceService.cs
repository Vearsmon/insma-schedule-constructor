using Dal.RegistryRepositories.TeacherPreference;
using Dal.Repositories.TeacherPreferences;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Mapping;
using Domain.Models;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class TeacherPreferenceService(
    ITeacherPreferenceRepository teacherPreferenceRepository,
    ITeacherPreferenceRegistryRepository teacherPreferenceRegistryRepository,
    ILessonService lessonService) : ITeacherPreferenceService
{
    public async Task<RegistryDto<TeacherPreferenceRegistryItemDto>> SearchAsync(
        TeacherPreferenceRegistrySearchModel searchModel)
    {
        var registryEntries = await teacherPreferenceRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
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
        var teacherTimeAvailabilityPreferences = saveTeacherPreferenceDto.TeacherTimeAvailabilities
            .Select(x => new TeacherPreference
            {
                ScheduleId = saveTeacherPreferenceDto.ScheduleId,
                TeacherId = saveTeacherPreferenceDto.TeacherId,
                DayOfWeekTimeInterval = x.DayOfWeekTimeInterval,
                TeacherPreferenceType = x.TeacherPreferenceType,
            });
        var teacherRoomPreferences = saveTeacherPreferenceDto.TeacherRoomPreferences
            .Select(x => new TeacherPreference
            {
                ScheduleId = saveTeacherPreferenceDto.ScheduleId,
                TeacherId = saveTeacherPreferenceDto.TeacherId,
                TeacherPreferenceType = x.TeacherPreferenceType,
                RoomId = x.RoomId,
            });
        var commentPreference = new TeacherPreference
        {
            ScheduleId = saveTeacherPreferenceDto.ScheduleId,
            TeacherId = saveTeacherPreferenceDto.TeacherId,
            Comment = saveTeacherPreferenceDto.Comment,
        };
        var previousPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = saveTeacherPreferenceDto.ScheduleId,
            TeacherId = saveTeacherPreferenceDto.TeacherId,
        });

        await teacherPreferenceRepository.DeleteAsync(previousPreferences.Select(x => x.Id!.Value).ToArray());
        var preferences = teacherTimeAvailabilityPreferences
            .Concat(teacherRoomPreferences)
            .Concat([commentPreference])
            .ToArray();
        if (preferences.Length > 0)
        {
            await lessonService.RecalculateConflictsForNewTeacherPreferences(preferences);
            await teacherPreferenceRepository.SaveAllAsync(preferences);
        }
    }
}