using Dal.RegistryRepositories.Schedule;
using Dal.Repositories.Schedules;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class ScheduleService(
    IScheduleRepository scheduleRepository,
    IScheduleRegistryRepository scheduleRegistryRepository) : IScheduleService
{
    public async Task<RegistryDto<ScheduleRegistryItemDto>> SearchAsync(ScheduleRegistrySearchModel searchModel)
    {
        var registryEntries = await scheduleRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<ScheduleRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<Guid> SaveAsync(SaveScheduleDto saveScheduleDto)
    {
        var schedule = DtoMappingRegister.Map(saveScheduleDto)!;
        return await scheduleRepository.SaveAsync(schedule);
    }
}