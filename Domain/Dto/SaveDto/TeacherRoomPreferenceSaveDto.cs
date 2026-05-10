using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class TeacherRoomPreferenceSaveDto
{
    public TeacherPreferenceType TeacherPreferenceType { get; set; }
    public Guid RoomId { get; set; }
}