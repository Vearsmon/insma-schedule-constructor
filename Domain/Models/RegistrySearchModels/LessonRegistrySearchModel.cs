using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class LessonRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
}