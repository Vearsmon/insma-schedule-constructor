using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class AcademicDisciplineRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
}