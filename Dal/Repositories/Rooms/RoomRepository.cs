using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Rooms;

public class RoomRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbRoom, Room> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbRoom, RoomSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbRoom, Room>(context, mapper, transactionalService), IRoomRepository
{
    public async Task<Room[]> SearchAsync(RoomSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await base.SelectAsync([id])).Length == 1;
    }
}