using Domain.Dto;
using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Services;

public interface ILessonValidationService
{
    Task<LessonValidationResult> ValidateAsync(Lesson lesson);

    Task<LessonWeekConflictDto[]> FillValidationMessages(Lesson[] lessons);

    Task RemoveValidationMessages(Guid[] lessonIds, LessonValidationCode[] validationCodes);
}