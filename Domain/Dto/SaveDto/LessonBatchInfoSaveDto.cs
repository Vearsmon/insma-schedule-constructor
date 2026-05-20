using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class LessonBatchInfoSaveDto
{
    public Guid? Id { get; set; }
    public Guid[] StudentGroupIds { get; set; } = [];
    public Guid[] TeacherIds { get; set; } = [];
    public Guid[] RoomIds { get; set; } = [];
    public int LessonsPerWeekCount { get; set; }
    public DayOfWeekTimeIntervalAssignmentSaveDto[] DayOfWeekTimeIntervals { get; set; } = [];
    public DisciplineLessonRepeatType RepeatType { get; set; }
    public DateInterval DateInterval { get; set; } = null!;
    public bool AllowCombining { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public int? HoursCost { get; set; }
    public int? TotalHoursCount { get; set; }
    public string? Comment { get; set; }
}