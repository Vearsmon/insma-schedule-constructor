using Dal.RegistryRepositories.Campus;
using Dal.Repositories.Campuses;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Exceptions;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class CampusService(
    ICampusRepository campusRepository,
    ICampusRegistryRepository campusRegistryRepository) : ICampusService
{
    public async Task<CampusShortDto[]> SearchShortAsync()
    {
        var items = await campusRepository.SelectAllAsync();
        return items.Select(CampusDtoMappingRegister.MapModelToShortDto).ToArray()!;
    }

    public async Task<RegistryDto<CampusRegistryItemDto>> SearchAsync(CampusRegistrySearchModel searchModel)
    {
        var registryEntries = await campusRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<CampusRegistryItemDto>
        {
            Items = registryEntries.Items.Select(CampusDtoMappingRegister.MapItemToItemDto).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task SaveAsync(CampusSaveDto campusSaveDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (campusSaveDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }
        if (campusSaveDto.Id.HasValue && !await campusRepository.ExistsAsync(campusSaveDto.Id!.Value))
        {
            validationMessages.Add(new ValidationMessage("Не найден учебный корпус для обновления"));
        }

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var campus = CampusDtoMappingRegister.MapSaveDtoToModel(campusSaveDto)!;
        await campusRepository.SaveAsync(campus);
    }
}