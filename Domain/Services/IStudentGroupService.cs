using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface IStudentGroupService
{
    Task<RegistryDto<StudentGroupRegistryItemDto>> SearchAsync(StudentGroupRegistrySearchModel searchModel);
    Task<StudentGroupViewDto> GetViewAsync(Guid studentGroupId);
    Task<Guid> SaveAsync(SaveStudentGroupDto saveStudentGroupDto);
}