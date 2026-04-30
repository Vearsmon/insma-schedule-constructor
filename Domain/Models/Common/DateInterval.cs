namespace Domain.Models.Common;

public class DateInterval
{
    public DateOnly DateFrom { get; init; }

    public DateOnly DateTo { get; init; }

    public override bool Equals(object? obj)
        => obj is DateInterval dateInterval
           && DateFrom == dateInterval.DateFrom
           && DateTo == dateInterval.DateTo;

    public override int GetHashCode() => HashCode.Combine(DateFrom, DateTo);
}
