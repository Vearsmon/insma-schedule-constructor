using Domain.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Dal.Helpers;

public static class PaginationExtensions
{
    public const int PageNumberDefault = 1;
    public const int PageSizeDefault = 25;

    public static async Task<T[]> GetPagedAsync<T>(
        this IQueryable<T> query,
        int pageNumber = 1,
        int pageSize = PageSizeDefault,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await query.Paged(pageNumber, pageSize)
                          .ToArrayAsync(cancellationToken);
    }

    public static IQueryable<T> Paged<T>(this IQueryable<T> query, int pageNumber = 1,
        int pageSize = PageSizeDefault, bool checkPageLimits = true)
        where T : class
    {
        PaginationHelper.CheckPageLimits(checkPageLimits, pageSize);
        var skip = (pageNumber - 1) * pageSize;

        return query.Skip(skip)
                    .Take(pageSize);
    }

    public static IEnumerable<T> Paged<T>(this IEnumerable<T> query, int pageNumber = 1,
        int pageSize = PageSizeDefault, bool checkPageLimits = true)
        where T : class
    {
        PaginationHelper.CheckPageLimits(checkPageLimits, pageSize);
        pageNumber = Math.Max(PageNumberDefault, pageNumber);
        var skip = (pageNumber - 1) * pageSize;

        return query.Skip(skip)
            .Take(pageSize);
    }
}
