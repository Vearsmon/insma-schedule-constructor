using Domain.Models.Common;

namespace Domain.Models.SearchModels;

public class LessonBatchInfoConflictsSearchModel
{
    public Guid? ScheduleId { get; set; }
    public Guid[] StudentGroupIds { get; set; } = [];
    public Guid[] TeacherIds { get; set; } = [];
    public Guid[] RoomIds { get; set; } = [];
    public DateWithTimeInterval[] DateWithTimeIntervals { get; set; } = [];
    public Guid[] ExcludeBatchIds { get; set; } = [];
}