using Domain.Dto.RegistryDto;
using Domain.Helpers;

namespace Dal.RegistryRepositories;

public static class SearchHelper
{
    private const int ItemsPerPage = 20;

    public static IQueryable<T> Page<T>(this IQueryable<T> query, SearchParametersDto model, bool checkPageLimits = true)
    {
        return query.Page(model.Page, model.ItemsPerPage, checkPageLimits);
    }

    public static IQueryable<T> Page<T>(this IQueryable<T> obj, int page, int? pageSize,
        bool checkPageLimits = true)
    {
        pageSize ??= ItemsPerPage;

        if (page < 1)
        {
            page = 1;
        }
        PaginationHelper.CheckPageLimits(checkPageLimits, pageSize!.Value);

        var num = (page - 1) * pageSize.Value;
        return obj.Skip(num).Take(pageSize.Value);
    }
}
