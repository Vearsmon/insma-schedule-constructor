using Domain.Models.Common;

namespace Domain.Dto.RegistryDto;

public class CampusRegistryItemDto : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
}