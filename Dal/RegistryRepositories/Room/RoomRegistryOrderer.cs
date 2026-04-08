using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Room;

public class RoomRegistryOrderer
    : IRegistryRepositoryOrderer<DbRoom, RoomRegistryInternalSearchModel>
{
    public IQueryable<DbRoom> Order(IQueryable<DbRoom> queryable,
        RoomRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}