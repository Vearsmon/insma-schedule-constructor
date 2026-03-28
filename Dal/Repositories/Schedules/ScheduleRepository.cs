using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Schedules;

public class ScheduleRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbSchedule, Schedule> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbSchedule, ScheduleSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbSchedule, Schedule>(context, mapper, transactionalService), IScheduleRepository
{
    public async Task<Schedule[]> SearchAsync(ScheduleSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await base.SelectAsync([id])).Length == 1;
    }
}