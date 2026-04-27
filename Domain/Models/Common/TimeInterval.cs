namespace Domain.Models.Common;

public class TimeInterval
{
    public TimeOnly TimeFrom { get; init; }
    public TimeOnly TimeTo { get; init; }

    public override bool Equals(object? obj)
        => obj is TimeInterval timeInterval
           && TimeFrom == timeInterval.TimeFrom
           && TimeTo == timeInterval.TimeTo;

    public override int GetHashCode() => HashCode.Combine(TimeFrom, TimeTo);

    public override string ToString() => $"{TimeFrom:HH:mm}-{TimeTo:HH:mm}";
}