using Domain.Models.Common;

namespace Domain.Dto.RegistryDto;

public class RoomRegistryItemDto : IModelWithId
{
    public Guid? Id { get; set; }
}