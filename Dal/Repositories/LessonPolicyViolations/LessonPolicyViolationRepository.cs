using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonPolicyViolations;

public class LessonPolicyViolationRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbPolicyViolation, LessonPolicyViolation> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbPolicyViolation, LessonPolicyViolationSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbPolicyViolation, LessonPolicyViolation>(context, mapper, transactionalService), ILessonPolicyViolationRepository
{
    public async Task<LessonPolicyViolation[]> SearchAsync(LessonPolicyViolationSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}