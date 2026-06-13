namespace Domain.Dto;

public class LessonValidationMessageBatchDto
{
    public Guid? LessonId { get; set; }
    public Guid? LessonBatchInfoId { get; set; }
    public Dictionary<Guid, string> MessagesByViolationId { get; set; } = [];
}