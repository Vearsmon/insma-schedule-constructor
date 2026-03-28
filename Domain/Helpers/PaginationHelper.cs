using Domain.Exceptions;
using Domain.Models.Enums;

namespace Domain.Helpers;

public static class PaginationHelper
{
    private const int MaxItemsPerPage = 100;

    public static void CheckPageLimits(bool checkPageLimits, int pageSize, int maxItemsPerPage = MaxItemsPerPage)
    {
        if (checkPageLimits && pageSize > maxItemsPerPage)
        {
            throw new ServiceException(ServiceExceptionTypes.ValidationError,
                "Укажите меньшее количество запрашиваемых данных");
        }
    }
}
