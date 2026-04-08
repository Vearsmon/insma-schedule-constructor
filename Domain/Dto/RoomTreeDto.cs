using Domain.Dto.ShortDto;

namespace Domain.Dto;

public class RoomTreeDto
{
    public Guid CampusId { get; set; }

    public string CampusName { get; set; } = null!;

    public RoomShortDto[] ChildRooms { get; set; } = null!;
}