using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.ShortDto;

public class LessonShortDto
{
    public Guid? Id { get; set; }
    public Guid? AcademicDisciplineId { get; set; }
    public string? AcademicDisciplineName { get; set; }
    public AcademicDisciplineType? AcademicDisciplineType { get; set; }
    public StudentGroupShortDto[] StudentGroups { get; set; } = [];
    public TeacherShortDto[] Teachers { get; set; } = [];
    public RoomShortDto[] Rooms { get; set; } = [];
    public Guid? DayOfWeekTimeIntervalAssignmentId { get; set; }
    public DateWithTimeInterval? DateWithTimeInterval { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public bool AllowCombining { get; set; }
    public string? LessonPolicyViolationDescription { get; set; }
    public LessonValidationErrorType? CurrentErrorsMaxLevel { get; set; }
    public string? Comment { get; set; }
}