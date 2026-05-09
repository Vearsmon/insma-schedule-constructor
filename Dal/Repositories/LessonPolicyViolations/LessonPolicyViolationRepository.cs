using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonPolicyViolations;

public class LessonPolicyViolationRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbLessonPolicyViolation, LessonPolicyViolation> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbLessonPolicyViolation, LessonPolicyViolationSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbLessonPolicyViolation, LessonPolicyViolation>(context, mapper, transactionalService), ILessonPolicyViolationRepository
{
    public async Task<LessonPolicyViolation[]> SearchAsync(LessonPolicyViolationSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}