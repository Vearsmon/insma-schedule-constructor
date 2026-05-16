namespace Domain.Models.Common;

public class DayOfWeekTimeInterval
{
    public DayOfWeek DayOfWeek { get; init; }
    public TimeInterval TimeInterval { get; init; } = null!;

    public override bool Equals(object? obj)
        => obj is DayOfWeekTimeInterval dayOfWeekTimeInterval
           && DayOfWeek == dayOfWeekTimeInterval.DayOfWeek
           && TimeInterval.Equals(dayOfWeekTimeInterval.TimeInterval)
           || obj is DateWithTimeInterval dateWithTimeInterval
           && DayOfWeek == dateWithTimeInterval.Date.DayOfWeek
           && TimeInterval.Equals(dateWithTimeInterval.TimeInterval);

    public override int GetHashCode() => HashCode.Combine(DayOfWeek, TimeInterval);
}