using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models.SearchModels;

public class TeacherPreferenceConflictsSearchModel
{
    public Guid? ScheduleId { get; set; }
    public Guid[] TeacherIds { get; set; } = [];
    public Guid[] RoomIds { get; set; } = [];
    public DayOfWeekTimeInterval[] DayOfWeekTimeIntervals { get; set; } = [];
    public TeacherPreferenceType[] TeacherPreferenceTypes { get; set; } = [];
}