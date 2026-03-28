using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class AcademicDisciplineLessonBatchInfoDto
{
    public Guid? Id { get; set; }
    public Guid StudentGroupId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? RoomId { get; set; }
    public DayOfWeekTimeInterval[] DayOfWeekTimeIntervals { get; set; } = [];
    public DisciplineLessonRepeatType RepeatType { get; set; }
    public DateInterval DateInterval { get; set; } = null!;
    public int HoursCost { get; set; }
}