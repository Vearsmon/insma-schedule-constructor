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
    public async Task DeleteViolationLinksAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var violations = await SelectAsync(ids, cancellationToken);
        var violationLinks = violations.Select(x => new DbLessonPolicyViolationLink
        {
            LessonId = x.LessonId,
            LessonPolicyViolationId = x.Id!.Value,
        });
        Context.Set<DbLessonPolicyViolationLink>().RemoveRange(violationLinks);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<LessonPolicyViolation[]> SearchAsync(LessonPolicyViolationSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}