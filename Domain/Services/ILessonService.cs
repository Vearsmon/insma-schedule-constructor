using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface ILessonService
{
    Task<LessonShortDto[]> SearchWeekAsync(Guid scheduleId, DateOnly dateFrom, DateOnly dateTo);

    Task<RegistryDto<LessonRegistryItemDto>> SearchAsync(LessonRegistrySearchModel searchModel);

    Task<LessonViewDto> GetViewAsync(Guid lessonId);

    Task SaveAsync(SaveLessonDto saveLessonDto);

    Task RecalculateConflictsForUpdatedAcademicDiscipline(AcademicDiscipline academicDiscipline);

    Task UpdateAcademicDisciplineLessons(AcademicDiscipline academicDiscipline);

    Task RecalculateConflictsForNewTeacherPreferences(TeacherPreference[] teacherPreferences);

    Task RecalculateConflictsForNewStudentGroup(StudentGroup studentGroup);

    Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(
        Guid academicDisciplineId,
        AcademicDisciplineLessonBatchInfo lessonBatchInfo,
        AcademicDisciplineType academicDisciplineType, Guid scheduleId);

    Task DeleteAsync(Guid scheduleId, Guid lessonId);
}