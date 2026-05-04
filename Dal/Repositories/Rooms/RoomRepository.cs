using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.Rooms;

public class RoomRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbRoom, Room> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbRoom, RoomSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbRoom, Room>(context, mapper, transactionalService), IRoomRepository
{
    public override async Task<Room> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(id, BaseQuery(), cancellationToken);
        return MapperReadonly.Map(entity);
    }

    public async Task<Room[]> SearchAsync(RoomSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await ExistAsync(predicateBuilder, new RoomSearchModel { Id = id });
    }

    protected override IQueryable<DbRoom> Query()
    {
        return BaseQuery()
            .Include(x => x.Campus);
    }
}