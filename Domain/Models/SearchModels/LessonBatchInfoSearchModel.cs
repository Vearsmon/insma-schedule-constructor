namespace Domain.Models.SearchModels;

public class LessonBatchInfoSearchModel
{
    public Guid? ScheduleId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Guid? AcademicDisciplineId { get; set; }
}