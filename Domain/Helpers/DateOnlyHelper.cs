using Domain.Models.Common;
using Domain.Models.Enums;

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

    public static bool HasIntersection(this DateInterval interval, DateOnly date)
    {
        return date >= interval.DateFrom && date <= interval.DateTo;
    }

    public static DateOnly[] GetDatesInIntervalByDaysOfWeek(DateInterval dateInterval,
        DayOfWeek[] daysOfWeek,
        DisciplineLessonRepeatType repeatType)
    {
        var result = new List<DateOnly>();
        var skipUntilDate = (DateOnly?)null;

        var dates = Enumerable.Range(0, (dateInterval.DateTo.Day - dateInterval.DateFrom.Day) + 1)
            .Select(offset => dateInterval.DateFrom.AddDays(offset));
        foreach (var date in dates)
        {
            if (date < skipUntilDate)
            {
                continue;
            }

            if (daysOfWeek.Contains(date.DayOfWeek))
            {
                result.Add(date);
            }

            if (date.DayOfWeek == DayOfWeek.Sunday && repeatType == DisciplineLessonRepeatType.EveryTwoWeeks)
            {
                skipUntilDate = date.AddDays(8);
            }
        }

        return result.ToArray();
    }

    public static DateInterval GetWeekDatesRangeByDate(this DateOnly date)
    {
        var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        return new DateInterval(startOfWeek, endOfWeek);
    }
}
