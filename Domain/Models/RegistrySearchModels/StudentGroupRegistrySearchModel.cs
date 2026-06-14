using Domain.Dto.RegistryDto;
using Domain.Models.Enums;

namespace Domain.Models.RegistrySearchModels;

public class StudentGroupRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
    public StudentGroupType? StudentGroupType { get; set; }
}