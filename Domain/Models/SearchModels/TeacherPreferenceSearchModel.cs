using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models.SearchModels;

public class TeacherPreferenceSearchModel
{
    public Guid? ScheduleId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid[] RoomIds { get; set; } = [];
    public DayOfWeek[] DaysOfWeek { get; set; } = [];
    public TimeInterval? TimeInterval { get; set; }
    public TeacherPreferenceType[] TeacherPreferenceTypes { get; set; } = [];
}