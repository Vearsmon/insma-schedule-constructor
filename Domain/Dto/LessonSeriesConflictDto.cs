using Domain.Models.Common;

namespace Domain.Dto;

public class LessonSeriesConflictDto
{
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
    public string Message { get; set; } = null!;
}