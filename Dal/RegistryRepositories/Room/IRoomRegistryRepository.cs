using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Room;

public interface IRoomRegistryRepository
    : IRegistryRepository<RoomRegistryItem, RoomRegistryInternalSearchModel>
{
}