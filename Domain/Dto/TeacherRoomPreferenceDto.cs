using Domain.Models.Enums;

namespace Domain.Dto;

public class TeacherRoomPreferenceDto
{
    public Guid RoomId { get; set; }

    public TeacherPreferenceType TeacherPreferenceType { get; set; }
}