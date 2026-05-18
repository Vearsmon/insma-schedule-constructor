using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class RoomDtoMappingRegister
{
    [MapperIgnoreSource(nameof(Room.Campus))]
    [MapperIgnoreTarget(nameof(RoomViewDto.CampusName))]
    public static partial RoomViewDto? MapModelToViewDto(Room? model);

    [MapperIgnoreTarget(nameof(Room.Campus))]
    public static partial Room? MapSaveDtoToModel(RoomSaveDto? dto);

    [MapperIgnoreTarget(nameof(Room.Campus))]
    public static partial void UpdateModelWithSaveDto(RoomSaveDto? dto, Room? model);

    public static partial RoomRegistryItemDto? MapItemToItemDto(RoomRegistryItem? item);

    [MapperIgnoreSource(nameof(Room.Campus))]
    [MapperIgnoreTarget(nameof(RoomShortDto.CampusName))]
    public static partial RoomShortDto? MapModelToShortDto(Room? model);
}