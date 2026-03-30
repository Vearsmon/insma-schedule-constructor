using Dal.RegistryRepositories.Teacher;
using Dal.Repositories.Teachers;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
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
    public async Task<RegistryDto<TeacherRegistryItemDto>> SearchAsync(TeacherRegistrySearchModel searchModel)
    {
        var registryEntries = await teacherRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<TeacherRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<TeacherViewDto> GetViewAsync(Guid teacherId)
    {
        var teacher = await teacherRepository.GetAsync(teacherId);
        return DtoMappingRegister.Map(teacher)!;
    }

    public async Task SaveAsync(SaveTeacherDto saveTeacherDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (saveTeacherDto.Fullname == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие имени"));
        }
        if (saveTeacherDto.Id.HasValue && !(await teacherRepository.ExistsAsync(saveTeacherDto.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найден преподаватель для обновления"));
        }

        if (validationMessages.Count != 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var teacher = DtoMappingRegister.Map(saveTeacherDto)!;
        await teacherRepository.SaveAsync(teacher);
    }
}