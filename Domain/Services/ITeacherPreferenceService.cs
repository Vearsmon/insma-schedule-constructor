using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface ITeacherPreferenceService
{
    Task<RegistryDto<TeacherPreferenceRegistryItemDto>> SearchAsync(TeacherPreferenceRegistrySearchModel searchModel);
    Task<TeacherPreferenceViewDto> GetViewAsync(Guid teacherId, Guid scheduleId);
    Task SaveAsync(SaveTeacherPreferenceDto saveTeacherPreferenceDto);
    Task DeleteAsync(Guid scheduleId, Guid teacherId);
}