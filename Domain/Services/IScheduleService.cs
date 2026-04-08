using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface IScheduleService
{
    Task<ScheduleShortDto[]> SearchShortAsync();
    Task<RegistryDto<ScheduleRegistryItemDto>> SearchAsync(ScheduleRegistrySearchModel searchModel);

    Task SaveAsync(SaveScheduleDto saveScheduleDto);
    Task DeleteAsync(Guid scheduleId);
}