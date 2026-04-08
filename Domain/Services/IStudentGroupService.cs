using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface IStudentGroupService
{
    Task<StudentGroupShortDto[]> SearchRootAsync(Guid scheduleId);
    Task<StudentGroupTreeDto[]> SearchTreeAsync(Guid scheduleId);
    Task<RegistryDto<StudentGroupRegistryItemDto>> SearchAsync(StudentGroupRegistrySearchModel searchModel);
    Task<StudentGroupViewDto> GetViewAsync(Guid studentGroupId);
    Task SaveAsync(SaveStudentGroupDto saveStudentGroupDto);
    Task<string[]> SearchCyphersAsync(Guid scheduleId);
    Task DeleteAsync(Guid studentGroupId);
}