using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class SaveTeacherPreferenceDto
{
    public Guid ScheduleId { get; set; }
    public Guid TeacherId { get; set; }
    public TeacherTimeAvailabilityDto[] TeacherTimeAvailabilities { get; set; } = [];
    public TeacherRoomPreferenceDto[] TeacherRoomPreferences { get; set; } = [];
    public RoomBoardType RoomBoardTypePreference { get; set; }
    public bool HasProjectorPreference { get; set; }
    public string? Comment { get; set; }
}