using Dal.Entities;
using Dal.Transactions;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonBatchInfo;

public class LessonBatchInfoRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbLessonBatchInfo, Domain.Models.LessonBatchInfo> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbLessonBatchInfo, LessonBatchInfoSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbLessonBatchInfo, Domain.Models.LessonBatchInfo>(context, mapper, transactionalService), ILessonBatchInfoRepository
{
    public async Task<Domain.Models.LessonBatchInfo[]> SearchAsync(LessonBatchInfoSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}