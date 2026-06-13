using Domain.Models.Enums;

namespace Domain.Dto.ShortDto;

public class LessonBatchInfoShortDto
{
    public Guid? Id { get; set; }
    public Guid? AcademicDisciplineId { get; set; }
    public string? AcademicDisciplineName { get; set; }
    public AcademicDisciplineType? Type { get; set; }
    public StudentGroupShortDto[] StudentGroups { get; set; } = [];
    public TeacherShortDto[] Teachers { get; set; } = [];
    public RoomShortDto[] Rooms { get; set; } = [];
    public int LessonsPerWeekCount { get; set; }
    public DayOfWeekTimeIntervalAssignmentShortDto[] DayOfWeekTimeIntervals { get; set; } = [];
    public bool AllowCombining { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public int? HoursCost { get; set; }
    public int? TotalHoursCount { get; set; }
    public string? LessonPolicyViolationDescription { get; set; }
    public LessonValidationErrorType? CurrentErrorsMaxLevel { get; set; }
    public string? Comment { get; set; }
}