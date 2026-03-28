namespace Domain.Dto.ViewDto;

public class TeacherPreferenceViewDto
{
    public TeacherTimeAvailabilityDto[] TeacherTimeAvailabilities { get; set; } = [];

    public TeacherRoomPreferenceDto[] TeacherRoomPreferences { get; set; } = [];

    public string? Comment { get; set; }
}