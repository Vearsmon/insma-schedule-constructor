using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class LessonSeriesConflictDto
{
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
    public string Message { get; set; } = null!;
    public LessonValidationErrorType ErrorType { get; set; }
}