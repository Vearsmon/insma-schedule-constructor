using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonPolicyViolations;

public interface ILessonPolicyViolationRepository : IRepository<LessonPolicyViolation>
{
    Task<LessonPolicyViolation[]> SearchAsync(LessonPolicyViolationSearchModel searchModel);
}