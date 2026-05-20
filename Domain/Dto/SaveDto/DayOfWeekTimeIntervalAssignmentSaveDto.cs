using Domain.Models.Common;

namespace Domain.Dto.SaveDto;

public class DayOfWeekTimeIntervalAssignmentSaveDto
{
    public Guid? Id { get; set; }
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
}