using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.RegistrySearchModels;

namespace Domain.Services;

public interface ILessonService
{
    Task<LessonShortDto[]> SearchWeekAsync(Guid scheduleId, DateOnly dateFrom, DateOnly dateTo);

    Task<RegistryDto<LessonRegistryItemDto>> SearchAsync(LessonRegistrySearchModel searchModel);

    Task<LessonViewDto> GetViewAsync(Guid lessonId);

    Task SaveAsync(LessonSaveDto lessonSaveDto);

    Task RecalculateConflictsForUpdatedAcademicDiscipline(AcademicDiscipline academicDiscipline);

    Task UpdateLessonsByBatches(Guid scheduleId,
        LessonBatchInfo[] lessonBatchInfos,
        Guid[] newLessonBatchInfoIds,
        LessonBatchInfoTimeAssignmentDifferenceDto[] dayOfWeekTimeIntervalDifferences);

    Task RecalculateConflictsForNewTeacherPreferences(TeacherPreference[] teacherPreferences);

    Task RecalculateConflictsForNewStudentGroup(StudentGroup studentGroup);

    Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Lesson lesson);

    Task DeleteAsync(Guid lessonId);
}