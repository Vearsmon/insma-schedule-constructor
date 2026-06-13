using Domain.Dto;
using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Helpers;

public static class DateOnlyHelper
{
    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    public static IEnumerable<DateOnly> ToDateSequence(this DateInterval interval)
    {
        for (var dt = interval.DateFrom; dt <= interval.DateTo; dt = dt.AddDays(1))
        {
            yield return dt;
        }
    }

    public static bool HasIntersection(this DateInterval interval, DateOnly date) =>
        date >= interval.DateFrom && date <= interval.DateTo;

    public static bool HasIntersection(this DateInterval first, DateInterval second) =>
        first.DateFrom <= second.DateTo && second.DateFrom <= first.DateTo;

    public static int GetLogicalDayOfWeekNumber(DayOfWeek dayOfWeek) =>
        dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

    public static DateOnly GetWeekStartDate(this DateOnly date) =>
        date.AddDays(1 - GetLogicalDayOfWeekNumber(date.DayOfWeek));

    public static DateOnly GetNextWeekStartDate(this DateOnly date) => date.GetWeekStartDate().AddDays(7);

    public static DateOnly GetPreviousWeekEndDate(this DateOnly date) => date.GetWeekStartDate().AddDays(-1);

    public static bool IntersectsEvenWeek(this DateOnly date, DateInterval dateInterval) =>
        date < dateInterval.DateFrom || date > dateInterval.DateTo
            ? throw new ArgumentOutOfRangeException()
            : (date.DayNumber - dateInterval.DateFrom.DayNumber) / 7 % 2 == 1;

    public static int GetDaysInDateIntervalCount(DateInterval dateInterval,
        int daysPerWeekCount,
        DisciplineLessonRepeatType repeatType,
        DateInterval scheduleDateInterval)
    {
        var isIntervalStartIntersectEvenWeek = dateInterval.DateFrom.IntersectsEvenWeek(scheduleDateInterval);
        var dateFrom = repeatType == DisciplineLessonRepeatType.OddWeeks && isIntervalStartIntersectEvenWeek
                            || repeatType == DisciplineLessonRepeatType.EvenWeeks && !isIntervalStartIntersectEvenWeek
            ? dateInterval.DateFrom.GetNextWeekStartDate()
            : dateInterval.DateFrom;

        var isIntervalEndIntersectEvenWeek = dateInterval.DateTo.IntersectsEvenWeek(scheduleDateInterval);
        var dateTo = repeatType == DisciplineLessonRepeatType.OddWeeks && isIntervalEndIntersectEvenWeek
                       || repeatType == DisciplineLessonRepeatType.EvenWeeks && !isIntervalEndIntersectEvenWeek
            ? dateInterval.DateTo.GetPreviousWeekEndDate()
            : dateInterval.DateTo;

        var totalDays = dateTo.DayNumber - dateFrom.DayNumber + 1;
        var weeksCount = (int)Math.Ceiling(totalDays / 7.0);
        return daysPerWeekCount * weeksCount;
    }

    public static DateOnly[] GetDatesInIntervalByDaysOfWeek(DateInterval dateInterval,
        DayOfWeek[] daysOfWeek,
        DisciplineLessonRepeatType repeatType,
        DateInterval scheduleDateInterval)
    {
        var result = new List<DateOnly>();
        var isIntervalStartIntersectEvenWeek = dateInterval.DateFrom.IntersectsEvenWeek(scheduleDateInterval);
        var skipUntilDate = repeatType == DisciplineLessonRepeatType.OddWeeks && isIntervalStartIntersectEvenWeek
                            || repeatType == DisciplineLessonRepeatType.EvenWeeks && !isIntervalStartIntersectEvenWeek
            ? dateInterval.DateFrom.GetNextWeekStartDate()
            : (DateOnly?)null;

        var dates = Enumerable.Range(0, dateInterval.DateTo.DayNumber - dateInterval.DateFrom.DayNumber + 1)
            .Select(offset => dateInterval.DateFrom.AddDays(offset));
        foreach (var date in dates)
        {
            if (date < skipUntilDate)
            {
                continue;
            }

            if (daysOfWeek.Contains(date.DayOfWeek))
            {
                if (repeatType == DisciplineLessonRepeatType.Once)
                {
                    return [date];
                }

                result.Add(date);
            }

            if (date.DayOfWeek == DayOfWeek.Sunday
                && repeatType is DisciplineLessonRepeatType.EvenWeeks or DisciplineLessonRepeatType.OddWeeks)
            {
                skipUntilDate = date.AddDays(8);
            }
        }

        return result.ToArray();
    }

    public static LessonSeriesConflictDto[] MergeIntersections(this LessonSeriesConflictDto[] seriesConflicts)
    {
        var result = new List<LessonSeriesConflictDto>();
        var seriesConflictsGroupByDayOfWeek = seriesConflicts.GroupBy(x => x.DayOfWeekTimeInterval!.DayOfWeek);
        foreach (var group in seriesConflictsGroupByDayOfWeek)
        {
            var sortedConflicts = group.OrderBy(x => x.DayOfWeekTimeInterval!.TimeInterval.TimeFrom).ToArray();
            var dayOfWeekMergedIntersections = new List<LessonSeriesConflictDto> { sortedConflicts.First() };
            for (var i = 1; i < sortedConflicts.Length; i++)
            {
                var lastConflict = dayOfWeekMergedIntersections.Last();
                var current = sortedConflicts[i];

                if (lastConflict.DayOfWeekTimeInterval!.TimeInterval.TimeTo >= current.DayOfWeekTimeInterval!.TimeInterval.TimeFrom)
                {
                    dayOfWeekMergedIntersections.Remove(lastConflict);
                    dayOfWeekMergedIntersections.Add(new LessonSeriesConflictDto
                    {
                        LessonIds = lastConflict.LessonIds.Concat(current.LessonIds).ToArray(),
                        DayOfWeekTimeInterval = new DayOfWeekTimeInterval
                        {
                            DayOfWeek = current.DayOfWeekTimeInterval.DayOfWeek,
                            TimeInterval = new TimeInterval
                            {
                                TimeFrom = lastConflict.DayOfWeekTimeInterval.TimeInterval.TimeFrom,
                                TimeTo = lastConflict.DayOfWeekTimeInterval.TimeInterval.TimeTo > current.DayOfWeekTimeInterval.TimeInterval.TimeTo
                                    ? lastConflict.DayOfWeekTimeInterval.TimeInterval.TimeTo
                                    : current.DayOfWeekTimeInterval.TimeInterval.TimeTo,
                            },
                        },
                        Messages = lastConflict.Messages.Concat(current.Messages).ToArray(),
                        MaxErrorType = new[] { lastConflict.MaxErrorType, current.MaxErrorType }.Max(),
                    });
                }
                else
                {
                    dayOfWeekMergedIntersections.Add(current);
                }
            }
            result.AddRange(dayOfWeekMergedIntersections);
        }

        return result.ToArray();
    }

    public static DateInterval GetWeekDatesRangeByDate(this DateOnly date)
    {
        var startOfWeek = date.GetWeekStartDate();
        var endOfWeek = startOfWeek.AddDays(6);
        return new DateInterval { DateFrom = startOfWeek, DateTo = endOfWeek };
    }

    public static DayOfWeekTimeInterval ToDayOfWeekTimeInterval(this DateWithTimeInterval dateWithTimeInterval) =>
        new()
        {
            DayOfWeek = dateWithTimeInterval.Date.DayOfWeek,
            TimeInterval = dateWithTimeInterval.TimeInterval,
        };
}