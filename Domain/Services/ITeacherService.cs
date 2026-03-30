using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface ITeacherService
{
    Task<RegistryDto<TeacherRegistryItemDto>> SearchAsync(TeacherRegistrySearchModel searchModel);
    Task<TeacherViewDto> GetViewAsync(Guid teacherId);
    Task SaveAsync(SaveTeacherDto saveTeacherDto);
}