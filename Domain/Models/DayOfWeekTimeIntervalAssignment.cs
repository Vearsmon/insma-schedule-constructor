using Domain.Models.Common;

namespace Domain.Models;

/// <summary>
/// Назначение дня недели с отрезком времени
/// </summary>
public class DayOfWeekTimeIntervalAssignment : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Сведения о созданных занятиях, к которым относится данное назначение
    /// </summary>
    public Guid LessonBatchInfoId { get; set; }

    /// <summary>
    /// День недели с отрезком времени
    /// </summary>
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
}