namespace Domain.Dto.SaveDto;

public class TeacherPreferenceSaveDto
{
    public Guid ScheduleId { get; set; }
    public Guid TeacherId { get; set; }
    public TeacherTimePreferenceSaveDto[] TeacherTimePreferences { get; set; } = [];
    public TeacherRoomPreferenceSaveDto[] TeacherRoomPreferences { get; set; } = [];
    public string? Comment { get; set; }
}