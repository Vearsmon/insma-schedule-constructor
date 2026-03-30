namespace Domain.Models.Common;

public class TimeInterval
{
    public TimeInterval()
    {
    }

    public TimeInterval(TimeOnly timeFrom, TimeOnly timeTo)
    {
        TimeFrom = timeFrom;
        TimeTo = timeTo;
    }

    public TimeOnly TimeFrom { get; }
    public TimeOnly TimeTo { get; }

    public override bool Equals(object? obj)
        => obj is TimeInterval timeInterval
           && TimeFrom == timeInterval.TimeFrom
           && TimeTo == timeInterval.TimeTo;

    public override int GetHashCode() => HashCode.Combine(TimeFrom, TimeTo);

    public override string ToString()
    {
        return $"{TimeFrom:HH:mm}-{TimeTo:HH:mm}";
    }
}