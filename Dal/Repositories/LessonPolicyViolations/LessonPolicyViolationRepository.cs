using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.LessonPolicyViolations;

public class LessonPolicyViolationRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbLessonPolicyViolation, LessonPolicyViolation> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbLessonPolicyViolation, LessonPolicyViolationSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbLessonPolicyViolation, LessonPolicyViolation>(context, mapper, transactionalService), ILessonPolicyViolationRepository
{
    public async Task DeleteViolationLinksAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        await Context.Set<DbLessonPolicyViolation>().Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<LessonPolicyViolation[]> SearchAsync(LessonPolicyViolationSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}