using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Rooms;

public class RoomMapper : IRepositoryMapper<DbRoom, Room>
{
    [return: NotNullIfNotNull("entity")]
    public Room? Map(DbRoom? entity) => RoomMappingRegister.MapEntityToModel(entity);

    public void Update(DbRoom entity, Room model)
    {
        RoomMappingRegister.UpdateEntityWithModel(model, entity);
    }
}