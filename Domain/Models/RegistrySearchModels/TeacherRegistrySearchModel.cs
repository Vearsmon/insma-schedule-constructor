using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class TeacherRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
    public string? Name { get; set; }
}