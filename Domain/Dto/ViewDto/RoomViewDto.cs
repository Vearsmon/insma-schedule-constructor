using Domain.Models.Enums;

namespace Domain.Dto.ViewDto;

public class RoomViewDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CampusId { get; set; }
    public RoomType RoomType { get; set; }
    public int Capacity { get; set; }
    public RoomBoardType RoomBoardType { get; set; }
    public bool HasProjector { get; set; }
}