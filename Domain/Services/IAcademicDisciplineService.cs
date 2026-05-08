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

    Task SaveAsync(AcademicDisciplineSaveDto academicDisciplineSaveDto);

    Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Guid academicDisciplineId,
        AcademicDisciplineType academicDisciplineType, Guid lessonBatchInfo);

    Task DeleteAsync(Guid academicDisciplineId);
}