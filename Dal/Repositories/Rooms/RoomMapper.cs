using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Rooms;

public class RoomMapper : IRepositoryMapper<DbRoom, Room>
{
    [return: NotNullIfNotNull("entity")]
    public Room? Map(DbRoom? entity)
    {
        var model = entity == null ? null : new Room();
        MappingRegister.Update(entity, model);
        return model;
    }

    public void Update(DbRoom entity, Room model)
    {
        MappingRegister.Update(model, entity);
    }
}