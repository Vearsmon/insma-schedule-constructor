using Domain.Helpers;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Dal.Helpers;

public static class QueryableExtensions
{
    public static async Task<TModel[]> TakePageAsync<TModel>(this IQueryable<TModel> query, IAutocompletePaginated pagination,
        bool checkPageLimits = true)
    {
        PaginationHelper.CheckPageLimits(checkPageLimits, pagination.Take);
        return await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToArrayAsync();
    }
}
