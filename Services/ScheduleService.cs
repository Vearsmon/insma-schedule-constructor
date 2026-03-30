using Dal.RegistryRepositories.Schedule;
using Dal.Repositories.Schedules;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Models.ValidationMessages;
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

    public async Task SaveAsync(SaveScheduleDto saveScheduleDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (saveScheduleDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }
        if (saveScheduleDto.Id.HasValue && !(await scheduleRepository.ExistsAsync(saveScheduleDto.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найден проект расписания для обновления"));
        }

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var schedule = DtoMappingRegister.Map(saveScheduleDto)!;
        await scheduleRepository.SaveAsync(schedule);
    }
}