using Domain.Dto.ShortDto;
using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class LessonBatchInfoDto
{
    public Guid? Id { get; set; }
    public StudentGroupShortDto[] StudentGroups { get; set; } = [];
    public Guid[] TeacherIds { get; set; } = [];
    public Guid[] RoomIds { get; set; } = [];
    public int LessonsPerWeekCount { get; set; }
    public DayOfWeekTimeIntervalAssignmentShortDto[] DayOfWeekTimeIntervals { get; set; } = [];
    public DisciplineLessonRepeatType RepeatType { get; set; }
    public DateInterval DateInterval { get; set; } = null!;
    public bool AllowCombining { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public int? HoursCost { get; set; }
    public int? TotalHoursCount { get; set; }
    public string? Comment { get; set; }
}