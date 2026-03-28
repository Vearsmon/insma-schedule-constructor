using Domain.Models.Common;

namespace Domain.Models.RegistryItemModels;

public class CampusRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
}