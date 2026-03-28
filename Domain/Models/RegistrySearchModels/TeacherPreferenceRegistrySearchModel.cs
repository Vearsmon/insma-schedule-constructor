using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class TeacherPreferenceRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
}