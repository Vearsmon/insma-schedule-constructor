using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class SaveRoomDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CampusId { get; set; }
    public RoomType RoomType { get; set; }
}