namespace Domain.Dto;

public class LessonValidationMessageBatchDto
{
    public Guid LessonId { get; set; }
    public string[] Messages { get; set; } = [];
}