using Dal.RegistryRepositories.Teacher;
using Dal.Repositories.Teachers;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
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

    public async Task<Guid> SaveAsync(SaveTeacherDto saveTeacherDto)
    {
        var teacher = DtoMappingRegister.Map(saveTeacherDto)!;
        return await teacherRepository.SaveAsync(teacher);
    }
}