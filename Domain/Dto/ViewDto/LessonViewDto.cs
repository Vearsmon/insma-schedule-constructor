using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.ViewDto;

public class LessonViewDto
{
    public Guid? Id { get; set; }
    public Guid? AcademicDisciplineId { get; set; }
    public AcademicDisciplineType? AcademicDisciplineType { get; set; }
    public Guid[] StudentGroupIds { get; set; } = [];
    public Guid? TeacherId { get; set; }
    public Guid? RoomId { get; set; }
    public DateWithTimeInterval? DateWithTimeInterval { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public bool AllowCombining { get; set; }
    public int HoursCost { get; set; }
    public LessonValidationMessage[] ValidationMessages { get; set; } = [];
}