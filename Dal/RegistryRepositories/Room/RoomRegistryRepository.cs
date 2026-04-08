using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Room;

internal class RoomRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbRoom, RoomRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbRoom, RoomRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbRoom, RoomRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbRoom, RoomRegistryItem,
            RoomRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        IRoomRegistryRepository
{
}