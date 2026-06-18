using Domain.Dto.RegistryDto;

namespace Domain.Models.RegistrySearchModels;

public class RoomRegistrySearchModel : IWithSearchParameters
{
    public SearchParametersDto SearchParameters { get; set; } = null!;
    public string? Name { get; set; }
    public Guid? CampusId { get; set; }
}