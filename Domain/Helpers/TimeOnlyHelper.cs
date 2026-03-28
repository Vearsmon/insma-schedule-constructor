using Domain.Models.Common;

namespace Domain.Helpers;

public static class TimeOnlyHelper
{
    public static bool HasIntersection(this TimeInterval first, TimeInterval second)
    {
        return first.TimeFrom <= second.TimeTo && first.TimeTo >= second.TimeFrom;
    }
}
