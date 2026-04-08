using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface ICampusService
{
    Task<CampusShortDto[]> SearchShortAsync();

    Task<RegistryDto<CampusRegistryItemDto>> SearchAsync(CampusRegistrySearchModel searchModel);

    Task SaveAsync(SaveCampusDto saveCampusDto);
}