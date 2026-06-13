using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class LessonSaveDto
{
    public Guid? Id { get; set; }
    public Guid[] StudentGroupIds { get; set; } = [];
    public Guid[] TeacherIds { get; set; } = [];
    public Guid[] RoomIds { get; set; } = [];
    public DateWithTimeInterval? DateWithTimeInterval { get; set; }
    public LessonFlexibilityType FlexibilityType { get; set; }
    public bool AllowCombining { get; set; }
    public Guid LessonBatchInfoId { get; set; }
    public Guid? DayOfWeekTimeIntervalAssignmentId { get; set; }
    public int HoursCost { get; set; }
    public bool UpdateBatch { get; set; }
}