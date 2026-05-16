using Dal.Entities;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class RoomMappingRegister
{
    [MapperIgnoreSource(nameof(DbRoom.Campus))]
    [MapperIgnoreTarget(nameof(Room.Campus))]
    public static partial Room? MapEntityToModel(DbRoom? entity);

    [MapperIgnoreSource(nameof(Room.Campus))]
    [MapperIgnoreTarget(nameof(DbRoom.Campus))]
    public static partial DbRoom? MapModelToEntity(Room? model);

    [MapperIgnoreSource(nameof(Room.Campus))]
    [MapperIgnoreTarget(nameof(DbRoom.Campus))]
    public static partial void UpdateEntityWithModel(Room? model, DbRoom? entity);

    [MapProperty(nameof(DbRoom.Campus), nameof(RoomRegistryItem.CampusName), Use = nameof(MapCampusName))]
    public static partial RoomRegistryItem? MapEntityToRegistryItem(DbRoom? entity);

    private static string MapCampusName(DbCampus campus) => campus.Name;
}