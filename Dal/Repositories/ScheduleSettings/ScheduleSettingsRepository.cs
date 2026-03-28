using Dal.Entities;
using Dal.Transactions;
using Domain.Models.SearchModels;

namespace Dal.Repositories.ScheduleSettings;

public class ScheduleSettingsRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbScheduleSettings, Domain.Models.ScheduleSettings> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbScheduleSettings, ScheduleSettingsSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbScheduleSettings, Domain.Models.ScheduleSettings>(context, mapper, transactionalService), IScheduleSettingsRepository
{
    public async Task<Domain.Models.ScheduleSettings[]> SearchAsync(ScheduleSettingsSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}