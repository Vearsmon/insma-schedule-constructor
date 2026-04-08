using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class AcademicDisciplineLessonBatchInfoDto
{
    public Guid? Id { get; set; }
    public Guid[] StudentGroupIds { get; set; } = [];
    public Guid? TeacherId { get; set; }
    public Guid? RoomId { get; set; }
    public DayOfWeekTimeInterval[] DayOfWeekTimeIntervals { get; set; } = [];
    public DisciplineLessonRepeatType RepeatType { get; set; }
    public DateInterval DateInterval { get; set; } = null!;
    public bool AllowCombining { get; set; }
    public int HoursCost { get; set; }
}