using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class LessonSeriesConflictDto
{
    public Guid[] LessonIds { get; set; } = [];
    public Guid[] LessonBatchInfoIds { get; set; } = [];
    public DayOfWeekTimeInterval? DayOfWeekTimeInterval { get; set; }
    public LessonSeriesConflictMessageDto[] Messages { get; set; } = [];
    public LessonValidationErrorType MaxErrorType { get; set; }
}