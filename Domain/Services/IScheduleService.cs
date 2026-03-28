using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface IScheduleService
{
    Task<RegistryDto<ScheduleRegistryItemDto>> SearchAsync(ScheduleRegistrySearchModel searchModel);

    Task<Guid> SaveAsync(SaveScheduleDto saveScheduleDto);
}