using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.Room;

public class RoomRegistryMapper : IReadonlyRepositoryMapper<DbRoom, RoomRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public RoomRegistryItem? Map(DbRoom? entity) => MappingRegister.MapRegistryItem(entity);
}