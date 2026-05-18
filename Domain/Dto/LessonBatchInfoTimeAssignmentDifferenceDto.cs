using Domain.Models.Common;

namespace Domain.Dto;

public class LessonBatchInfoTimeAssignmentDifferenceDto
{
    public Guid LessonBatchInfoId { get; set; }
    public DayOfWeekTimeInterval[] PreviousAssignments { get; set; } = [];
    public DayOfWeekTimeInterval[] CurrentAssignments { get; set; } = [];
}