using Domain.Models.Common;

namespace Domain.Dto;

public class LessonSeriesConflictMessageDto
{
    public TimeInterval? TimeInterval { get; set; }

    public string Message { get; set; } = null!;
}