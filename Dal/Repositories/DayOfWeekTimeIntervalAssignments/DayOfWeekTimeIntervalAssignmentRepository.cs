using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.DayOfWeekTimeIntervalAssignments;

public class DayOfWeekTimeIntervalAssignmentRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbDayOfWeekTimeIntervalAssignment, DayOfWeekTimeIntervalAssignment> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbDayOfWeekTimeIntervalAssignment, DayOfWeekTimeIntervalAssignmentSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbDayOfWeekTimeIntervalAssignment, DayOfWeekTimeIntervalAssignment>(context, mapper, transactionalService), IDayOfWeekTimeIntervalAssignmentRepository
{
    public async Task<DayOfWeekTimeIntervalAssignment[]> SearchAsync(DayOfWeekTimeIntervalAssignmentSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}