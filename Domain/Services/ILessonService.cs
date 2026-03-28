using Domain.Dto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.Common;

namespace Domain.Services;

public interface ILessonService
{
    Task<LessonViewDto> GetViewAsync(Guid lessonId);

    Task SaveAsync(SaveLessonDto saveLessonDto);

    Task RecalculateConflictsForUpdatedAcademicDiscipline(AcademicDiscipline academicDiscipline);

    Task UpdateAcademicDisciplineLessons(AcademicDiscipline academicDiscipline);

    Task RecalculateConflictsForNewTeacherPreferences(TeacherPreference[] teacherPreferences);

    Task RecalculateConflictsForNewStudentGroup(StudentGroup studentGroup);

    Task<LessonWeekConflictDto[]> GetLessonWeekConflictsAsync(Guid lessonId, DateInterval dateInterval);
}