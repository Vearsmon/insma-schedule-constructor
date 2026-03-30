namespace Domain.Models.Common;

public class DayOfWeekTimeInterval
{
    public DayOfWeekTimeInterval()
    {
    }

    public DayOfWeekTimeInterval(DayOfWeek dayOfWeek, TimeInterval timeInterval)
    {
        DayOfWeek = dayOfWeek;
        TimeInterval = timeInterval;
    }

    public DayOfWeek DayOfWeek { get; }
    public TimeInterval TimeInterval { get; } = null!;

    public override bool Equals(object? obj)
        => obj is DayOfWeekTimeInterval dayOfWeekTimeInterval
           && DayOfWeek == dayOfWeekTimeInterval.DayOfWeek
           && TimeInterval.Equals(dayOfWeekTimeInterval.TimeInterval);

    public override int GetHashCode() => HashCode.Combine(DayOfWeek, TimeInterval);
}