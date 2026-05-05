using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class LessonSaveDto
{
    public Guid? Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid? AcademicDisciplineId { get; set; }
    public AcademicDisciplineType? AcademicDisciplineType { get; set; }
    public Guid[] StudentGroupIds { get; set; } = [];
    public Guid[] TeacherIds { get; set; } = [];
    public Guid[] RoomIds { get; set; } = [];
    public DateWithTimeInterval? DateWithTimeInterval { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public bool AllowCombining { get; set; }
    public int HoursCost { get; set; }
}