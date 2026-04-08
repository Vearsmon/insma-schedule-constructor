using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface ITeacherService
{
    Task<TeacherShortDto[]> SearchShortAsync();
    Task<RegistryDto<TeacherRegistryItemDto>> SearchAsync(TeacherRegistrySearchModel searchModel);
    Task<TeacherViewDto> GetViewAsync(Guid teacherId);
    Task SaveAsync(SaveTeacherDto saveTeacherDto);
    Task DeleteAsync(Guid teacherId);
}