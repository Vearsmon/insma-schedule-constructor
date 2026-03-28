using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class StudentGroupRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
}