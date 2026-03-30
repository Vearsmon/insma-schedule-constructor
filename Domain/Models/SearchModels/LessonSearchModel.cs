using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models.SearchModels;

public class LessonSearchModel
{
    public Guid? ScheduleId { get; set; }
    public Guid? AcademicDisciplineId { get; set; }
    public AcademicDisciplineType[] Types { get; set; } = [];
    public Guid[] StudentGroupIds { get; set; } = [];
    public Guid[] RoomIds { get; set; } = [];
    public DateOnly? Date { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Guid? TeacherId { get; set; }
    public TimeInterval[] TimeIntervals { get; set; } = [];
    public DayOfWeekTimeInterval[] DayOfWeekTimeIntervals { get; set; } = [];
    public bool ExcludeAllowCombining { get; set; }
}