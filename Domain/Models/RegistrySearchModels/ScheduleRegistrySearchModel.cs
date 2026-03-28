using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class ScheduleRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
}