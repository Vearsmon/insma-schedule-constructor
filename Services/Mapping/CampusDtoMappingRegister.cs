using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class CampusDtoMappingRegister
{
    public static partial Campus? MapSaveDtoToModel(CampusSaveDto? dto);
    public static partial CampusRegistryItemDto? MapItemToItemDto(CampusRegistryItem? item);
    public static partial CampusShortDto? MapModelToShortDto(Campus? model);
}