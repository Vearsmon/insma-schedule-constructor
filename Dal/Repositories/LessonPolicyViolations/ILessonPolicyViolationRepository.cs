using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonPolicyViolations;

public interface ILessonPolicyViolationRepository : IRepository<LessonPolicyViolation>
{
    Task DeleteViolationLinksAsync(Guid[] ids, CancellationToken cancellationToken = default);

    Task<LessonPolicyViolation[]> SearchAsync(LessonPolicyViolationSearchModel searchModel);
}