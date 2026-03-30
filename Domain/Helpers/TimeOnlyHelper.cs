using Domain.Models.Common;

namespace Domain.Helpers;

public static class TimeOnlyHelper
{
    public static bool HasIntersection(this TimeInterval first, TimeInterval second)
    {
        return first.TimeFrom <= second.TimeTo && first.TimeTo >= second.TimeFrom;
    }

    public static TimeInterval[] MergeIntersections(this TimeInterval[] timeIntervals)
    {
        var sortedIntervals = timeIntervals.OrderBy(x => x.TimeFrom).ToArray();
        var result = new List<TimeInterval>() { sortedIntervals.First() };
        for (var i = 1; i < sortedIntervals.Length; i++)
        {
            var lastInterval = result.Last();
            var current = sortedIntervals[i];

            if (lastInterval.TimeTo >= current.TimeFrom)
            {
                result.Remove(lastInterval);
                result.Add(new TimeInterval(lastInterval.TimeFrom,
                    lastInterval.TimeTo > current.TimeTo ? lastInterval.TimeTo : current.TimeTo));
            }
            else
            {
                result.Add(current);
            }
        }

        return result.ToArray();
    }
}
