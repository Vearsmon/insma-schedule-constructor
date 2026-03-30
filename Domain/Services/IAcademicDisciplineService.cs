using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface IAcademicDisciplineService
{
    Task<RegistryDto<AcademicDisciplineRegistryItemDto>> SearchAsync(AcademicDisciplineRegistrySearchModel searchModel);

    Task<AcademicDisciplineViewDto> GetViewAsync(Guid academicDisciplineId);

    Task SaveAsync(SaveAcademicDisciplineDto saveAcademicDisciplineDto);
}