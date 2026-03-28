using Domain.Models.Common;

namespace Domain.Dto;

public class LessonWeekConflictDto
{
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
    public string Message { get; set; } = null!;
}