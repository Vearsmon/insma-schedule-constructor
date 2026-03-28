namespace Domain.Models;

public class AcademicDisciplinePayload
{
    /// <summary>
    /// Требуемое количество часов освоения занятий указанного вида
    /// </summary>
    public int TotalHoursCount { get; set; }

    /// <summary>
    /// Число учебных недель, в течение которых осваиваются занятия указанного вида
    /// </summary>
    public int StudyWeeksCount { get; set; }

    /// <summary>
    /// Требуемое число занятий указанного вида в неделю
    /// </summary>
    public int LessonsPerWeekCount { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину занятиях
    /// </summary>
    public AcademicDisciplineLessonBatchInfo? LessonBatchInfo { get; set; }
}