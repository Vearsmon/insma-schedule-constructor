using Domain.Models.Common;

namespace Domain.Models.RegistryItemModels;

public class RoomRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
}