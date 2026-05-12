namespace Domain.Dto;

public class LessonValidationMessageBatchDto
{
    public Guid LessonId { get; set; }
    public Dictionary<Guid, string> MessagesByViolationId { get; set; } = [];
}