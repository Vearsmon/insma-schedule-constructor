namespace Domain.Dto.ViewDto;

public class TeacherPreferenceViewDto
{
    public TeacherTimePreferenceViewDto[] TeacherTimePreferences { get; set; } = [];
    public TeacherRoomPreferenceViewDto[] TeacherRoomPreferences { get; set; } = [];
    public string? Comment { get; set; }
}