using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Campuses;

public class CampusRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbCampus, Campus> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbCampus, CampusSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbCampus, Campus>(context, mapper, transactionalService), ICampusRepository
{
    public async Task<Campus[]> SearchAsync(CampusSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await base.SelectAsync([id])).Length == 1;
    }
}