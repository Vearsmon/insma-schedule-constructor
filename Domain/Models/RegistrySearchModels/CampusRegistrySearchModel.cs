using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class CampusRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
}