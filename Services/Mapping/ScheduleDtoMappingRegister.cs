using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class ScheduleDtoMappingRegister
{
    public static partial Schedule? MapSaveDtoToModel(ScheduleSaveDto? dto);
    public static partial ScheduleRegistryItemDto? MapItemToItemDto(ScheduleRegistryItem? item);
    public static partial ScheduleShortDto? MapModelToShortDto(Schedule? model);
}