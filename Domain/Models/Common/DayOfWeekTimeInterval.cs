namespace Domain.Models.Common;

public class DayOfWeekTimeInterval
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeInterval TimeInterval { get; set; } = null!;
}