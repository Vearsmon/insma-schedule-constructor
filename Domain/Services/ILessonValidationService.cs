using Domain.Dto;
using Domain.Models;

namespace Domain.Services;

public interface ILessonValidationService
{
    Task<LessonValidationResult> ValidateAsync(Lesson lesson);

    Task<LessonWeekConflictDto[]> FillValidationMessages(Lesson[] lessons);
}