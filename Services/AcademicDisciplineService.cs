using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.Repositories.AcademicDisciplines;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class AcademicDisciplineService(
    IAcademicDisciplineRepository academicDisciplineRepository,
    IAcademicDisciplineRegistryRepository academicDisciplineRegistryRepository,
    ILessonService lessonService) : IAcademicDisciplineService
{
    public async Task<RegistryDto<AcademicDisciplineRegistryItemDto>> SearchAsync(AcademicDisciplineRegistrySearchModel searchModel)
    {
        var registryEntries = await academicDisciplineRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<AcademicDisciplineRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<AcademicDisciplineViewDto> GetViewAsync(Guid academicDisciplineId)
    {
        var academicDiscipline = await academicDisciplineRepository.GetAsync(academicDisciplineId);
        return DtoMappingRegister.Map(academicDiscipline)!;
    }

    public async Task<Guid> SaveAsync(SaveAcademicDisciplineDto saveAcademicDisciplineDto)
    {
        var academicDiscipline = DtoMappingRegister.Map(saveAcademicDisciplineDto)!;
        if (saveAcademicDisciplineDto.Id.HasValue)
        {
            await lessonService.UpdateAcademicDisciplineLessons(academicDiscipline);
        }

        var id = await academicDisciplineRepository.SaveAsync(academicDiscipline);
        if (saveAcademicDisciplineDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForUpdatedAcademicDiscipline(academicDiscipline);
        }
        return id;
    }
}