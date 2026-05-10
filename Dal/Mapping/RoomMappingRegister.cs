using Dal.Entities;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class RoomMappingRegister
{
    [MapperIgnoreSource(nameof(DbRoom.Campus))]
    public static partial Room? MapEntityToModel(DbRoom? entity);

    [MapperIgnoreTarget(nameof(DbRoom.Campus))]
    public static partial DbRoom? MapModelToEntity(Room? model);

    [MapperIgnoreTarget(nameof(DbRoom.Campus))]
    public static partial void UpdateEntityWithModel(Room? model, DbRoom? entity);

    [MapperIgnoreSource(nameof(DbRoom.Campus))]
    public static partial void UpdateModelWithEntity(DbRoom? entity, Room? model);

    [MapperIgnoreSource(nameof(DbRoom.Campus))]
    [MapperIgnoreTarget(nameof(RoomRegistryItem.CampusName))]
    public static partial RoomRegistryItem? MapEntityToRegistryItem(DbRoom? entity);
}