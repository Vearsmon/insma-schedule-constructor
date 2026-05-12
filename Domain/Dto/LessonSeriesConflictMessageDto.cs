using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class LessonSeriesConflictMessageDto
{
    public TimeInterval? TimeInterval { get; set; }
    public string Message { get; set; } = null!;
    public LessonValidationErrorType ErrorType { get; set; }
}