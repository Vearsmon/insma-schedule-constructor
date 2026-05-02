using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class LessonSeriesConflictDto
{
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
    public string[] Messages { get; set; } = [];
    public LessonValidationErrorType ErrorType { get; set; }
}