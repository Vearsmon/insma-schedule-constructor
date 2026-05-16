namespace Domain.Models.Common;

public class DateWithTimeInterval
{
    public DateOnly Date { get; init; }
    public TimeInterval TimeInterval { get; init; } = null!;

    public override bool Equals(object? obj)
        => obj is DateWithTimeInterval dateWithTimeInterval
           && Date == dateWithTimeInterval.Date
           && TimeInterval.Equals(dateWithTimeInterval.TimeInterval)
           || obj is DayOfWeekTimeInterval dayOfWeekTimeInterval
           && Date.DayOfWeek == dayOfWeekTimeInterval.DayOfWeek
           && TimeInterval.Equals(dayOfWeekTimeInterval.TimeInterval);

    public override int GetHashCode() => HashCode.Combine(Date, TimeInterval);
}