using Dal.RegistryRepositories.Campus;
using Dal.Repositories.Campuses;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class CampusService(
    ICampusRepository campusRepository,
    ICampusRegistryRepository campusRegistryRepository) : ICampusService
{
    public async Task<RegistryDto<CampusRegistryItemDto>> SearchAsync(CampusRegistrySearchModel searchModel)
    {
        var registryEntries = await campusRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<CampusRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<Guid> SaveAsync(SaveCampusDto saveCampusDto)
    {
        var campus = DtoMappingRegister.Map(saveCampusDto)!;
        return await campusRepository.SaveAsync(campus);
    }
}