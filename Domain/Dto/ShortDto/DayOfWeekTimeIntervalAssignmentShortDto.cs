using Domain.Models.Common;

namespace Domain.Dto.ShortDto;

public class DayOfWeekTimeIntervalAssignmentShortDto
{
    public Guid Id { get; set; }
    public Guid LessonBatchInfoId { get; set; }
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
}