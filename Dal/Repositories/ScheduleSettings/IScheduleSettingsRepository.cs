using Domain.Models.SearchModels;

namespace Dal.Repositories.ScheduleSettings;

public interface IScheduleSettingsRepository : IRepository<Domain.Models.ScheduleSettings>
{
    Task<Domain.Models.ScheduleSettings[]> SearchAsync(ScheduleSettingsSearchModel searchModel);
}