using Domain.Models.Enums;

namespace Domain.Dto;

public class TeacherRoomPreferenceDto
{
    public TeacherPreferenceType TeacherPreferenceType { get; set; }
    public Guid RoomId { get; set; }
}