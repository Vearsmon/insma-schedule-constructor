namespace Dal.Entities;

/// <summary>
/// Назначение дня недели с отрезком времени
/// </summary>
public class DbDayOfWeekTimeIntervalAssignment : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Сведения о созданных занятиях, к которым относится данное назначение
    /// </summary>
    public Guid LessonBatchInfoId { get; set; }

    /// <summary>
    /// День недели
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Начало отрезка времени
    /// </summary>
    public TimeOnly TimeFrom { get; set; }

    /// <summary>
    /// Конец отрезка времени
    /// </summary>
    public TimeOnly TimeTo { get; set; }
}