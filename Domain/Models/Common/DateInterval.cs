namespace Domain.Models.Common;

public class DateInterval
{
    public DateInterval()
    {
    }

    public DateInterval(DateOnly dateFrom, DateOnly dateTo)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
    }

    public DateOnly DateFrom { get; }

    public DateOnly DateTo { get; }

    public override bool Equals(object? obj)
        => obj is DateInterval dateInterval
           && DateFrom == dateInterval.DateFrom
           && DateTo == dateInterval.DateTo;

    public override int GetHashCode() => HashCode.Combine(DateFrom, DateTo);
}
