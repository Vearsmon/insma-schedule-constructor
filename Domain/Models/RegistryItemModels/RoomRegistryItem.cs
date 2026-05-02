using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models.RegistryItemModels;

public class RoomRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CampusId { get; set; }
    public string CampusName { get; set; } = null!;
    public RoomType? RoomType { get; set; }
    public int? Capacity { get; set; }
    public RoomBoardType? RoomBoardType { get; set; }
    public bool? HasProjector { get; set; }
}