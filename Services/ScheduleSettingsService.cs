using Dal.Repositories.ScheduleSettings;
using Domain.Services;

namespace Services;

public class ScheduleSettingsService(IScheduleSettingsRepository scheduleSettingsRepository) : IScheduleSettingsService
{
}