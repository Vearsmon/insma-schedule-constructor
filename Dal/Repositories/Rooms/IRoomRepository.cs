using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Rooms;

public interface IRoomRepository : IRepository<Room>
{
    Task<Room[]> SearchAsync(RoomSearchModel searchModel);

    Task<bool> ExistsAsync(Guid id);
}