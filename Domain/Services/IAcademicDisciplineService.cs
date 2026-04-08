using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface IAcademicDisciplineService
{
    Task<AcademicDisciplineShortDto[]> SearchShortAsync(Guid scheduleId);

    Task<RegistryDto<AcademicDisciplineRegistryItemDto>> SearchAsync(AcademicDisciplineRegistrySearchModel searchModel);

    Task<AcademicDisciplineViewDto> GetViewAsync(Guid academicDisciplineId);

    Task SaveAsync(SaveAcademicDisciplineDto saveAcademicDisciplineDto);

    Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Guid academicDisciplineId,
        AcademicDisciplineType academicDisciplineType);

    Task<string[]> SearchCyphersAsync(Guid scheduleId);

    Task DeleteAsync(Guid academicDisciplineId);
}