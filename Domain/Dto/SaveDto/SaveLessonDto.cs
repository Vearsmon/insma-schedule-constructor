using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class SaveLessonDto
{
    public Guid? Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid? AcademicDisciplineId { get; set; }
    public AcademicDisciplineType? AcademicDisciplineType { get; set; }
    public Guid StudentGroupId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? RoomId { get; set; }
    public DateWithTimeInterval? DateWithTimeInterval { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public bool AllowCombining { get; set; }
    public int HoursCost { get; set; }
}