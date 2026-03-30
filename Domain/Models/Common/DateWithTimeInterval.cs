namespace Domain.Models.Common;

public class DateWithTimeInterval
{
    public DateWithTimeInterval()
    {
    }

    public DateWithTimeInterval(DateOnly date, TimeInterval timeInterval)
    {
        Date = date;
        TimeInterval = timeInterval;
    }

    public DateOnly Date { get; }
    public TimeInterval TimeInterval { get; } = null!;

    public override bool Equals(object? obj)
        => obj is DateWithTimeInterval dateWithTimeInterval
           && Date == dateWithTimeInterval.Date
           && TimeInterval.Equals(dateWithTimeInterval.TimeInterval);

    public override int GetHashCode() => HashCode.Combine(Date, TimeInterval);
}