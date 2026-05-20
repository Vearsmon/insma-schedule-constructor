using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.DayOfWeekTimeIntervalAssignments;

public interface IDayOfWeekTimeIntervalAssignmentRepository : IRepository<DayOfWeekTimeIntervalAssignment>
{
    Task<DayOfWeekTimeIntervalAssignment[]> SearchAsync(DayOfWeekTimeIntervalAssignmentSearchModel searchModel);
}