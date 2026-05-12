using Dal.RegistryRepositories.Schedule;
using Dal.Repositories.Schedules;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
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
    public async Task<ScheduleShortDto[]> SearchShortAsync()
    {
        var items = await scheduleRepository.SelectAllAsync();
        return items.Select(ScheduleDtoMappingRegister.MapModelToShortDto).ToArray()!;
    }

    public async Task<RegistryDto<ScheduleRegistryItemDto>> SearchAsync(ScheduleRegistrySearchModel searchModel)
    {
        var registryEntries = await scheduleRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<ScheduleRegistryItemDto>
        {
            Items = registryEntries.Items.Select(ScheduleDtoMappingRegister.MapItemToItemDto).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task SaveAsync(ScheduleSaveDto scheduleSaveDto)
    {
        await ValidateAsync(scheduleSaveDto);

        var schedule = ScheduleDtoMappingRegister.MapSaveDtoToModel(scheduleSaveDto)!;
        await scheduleRepository.SaveAsync(schedule);
    }

    public async Task DeleteAsync(Guid scheduleId)
    {
        await scheduleRepository.DeleteAsync(scheduleId);
    }

    private async Task ValidateAsync(ScheduleSaveDto scheduleSaveDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (scheduleSaveDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }
        if (scheduleSaveDto.Id.HasValue && !await scheduleRepository.ExistsAsync(scheduleSaveDto.Id!.Value))
        {
            validationMessages.Add(new ValidationMessage("Не найден проект расписания для обновления"));
        }

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }
    }
}