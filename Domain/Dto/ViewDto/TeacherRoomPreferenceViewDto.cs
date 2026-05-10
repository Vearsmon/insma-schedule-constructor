using Domain.Models.Enums;

namespace Domain.Dto.ViewDto;

public class TeacherRoomPreferenceViewDto
{
    public TeacherPreferenceType TeacherPreferenceType { get; set; }
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = null!;
}