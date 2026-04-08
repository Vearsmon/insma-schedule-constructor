namespace Domain.Models;

public class AcademicDisciplinePayload
{
    /// <summary>
    /// Требуемое количество часов освоения занятий указанного вида
    /// </summary>
    public int TotalHoursCount { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину занятиях
    /// </summary>
    public AcademicDisciplineLessonBatchInfo? LessonBatchInfo { get; set; }
}