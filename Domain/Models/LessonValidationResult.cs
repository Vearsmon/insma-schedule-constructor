namespace Domain.Models;

public class LessonValidationResult
{
    public LessonValidationMessage[] Messages { get; set; } = [];
    public Dictionary<Guid, Lesson> LessonsWithConflictById { get; set; } = [];
}