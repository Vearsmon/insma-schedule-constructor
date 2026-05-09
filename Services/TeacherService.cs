using Dal.RegistryRepositories.Teacher;
using Dal.Repositories.Teachers;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class TeacherService(
    ITeacherRepository teacherRepository,
    ITeacherRegistryRepository teacherRegistryRepository) : ITeacherService
{
    public async Task<TeacherShortDto[]> SearchShortAsync()
    {
        var items = await teacherRepository.SelectAllAsync();
        return items.Select(TeacherDtoMappingRegister.MapModelToShortDto).ToArray()!;
    }

    public async Task<RegistryDto<TeacherRegistryItemDto>> SearchAsync(TeacherRegistrySearchModel searchModel)
    {
        var registryEntries = await teacherRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<TeacherRegistryItemDto>
        {
            Items = registryEntries.Items.Select(TeacherDtoMappingRegister.MapItemToItemDto).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<TeacherViewDto> GetViewAsync(Guid teacherId)
    {
        var teacher = await teacherRepository.GetAsync(teacherId);
        return TeacherDtoMappingRegister.MapModelToViewDto(teacher)!;
    }

    public async Task SaveAsync(TeacherSaveDto teacherSaveDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (teacherSaveDto.Fullname == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие имени"));
        }
        if (teacherSaveDto.Id.HasValue && !await teacherRepository.ExistsAsync(teacherSaveDto.Id!.Value))
        {
            validationMessages.Add(new ValidationMessage("Не найден преподаватель для обновления"));
        }

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var teacher = TeacherDtoMappingRegister.MapSaveDtoToModel(teacherSaveDto)!;
        await teacherRepository.SaveAsync(teacher);
    }

    public async Task DeleteAsync(Guid teacherId)
    {
        await teacherRepository.DeleteAsync([teacherId]);
    }
}