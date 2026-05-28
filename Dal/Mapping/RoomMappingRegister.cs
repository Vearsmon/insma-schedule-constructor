using Dal.Entities;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class RoomMappingRegister
{
    public static partial Room? MapEntityToModel(DbRoom? entity);

    public static partial DbRoom? MapModelToEntity(Room? model);

    public static partial void UpdateEntityWithModel(Room? model, DbRoom? entity);

    [MapProperty(nameof(DbRoom.Campus), nameof(RoomRegistryItem.CampusName), Use = nameof(MapCampusName))]
    public static partial RoomRegistryItem? MapEntityToRegistryItem(DbRoom? entity);

    private static string MapCampusName(DbCampus campus) => campus.Name;
}