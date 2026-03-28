using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Schedules;

public interface IScheduleRepository : IRepository<Schedule>
{
    Task<Schedule[]> SearchAsync(ScheduleSearchModel searchModel);

    Task<bool> ExistsAsync(Guid id);
}