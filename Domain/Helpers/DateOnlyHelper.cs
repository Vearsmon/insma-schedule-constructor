using Domain.Models.Common;

namespace Domain.Helpers;

public static class DateOnlyHelper
{
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    public static IEnumerable<DateOnly> ToDateSequence(this DateInterval interval)
    {
        for (var dt = interval.DateFrom; dt <= interval.DateTo; dt = dt.AddDays(1))
        {
            yield return dt;
        }
    }

    public static DateOnly[] GetDatesInIntervalByDaysOfWeek(DateInterval dateInterval, DayOfWeek[] daysOfWeek)
    {
        return Enumerable.Range(0, (dateInterval.DateTo.Day - dateInterval.DateFrom.Day) + 1)
            .Select(offset => dateInterval.DateFrom.AddDays(offset))
            .Where(d => daysOfWeek.Contains(d.DayOfWeek))
            .ToArray();
    }
}
