using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

public class LessonPolicyViolation : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public Guid? LessonId { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public Lesson? Lesson { get; set; }

    /// <summary>
    /// Серия занятий
    /// </summary>
    public Guid? LessonBatchInfoId { get; set; }

    /// <summary>
    /// Серия занятий
    /// </summary>
    public LessonBatchInfo? LessonBatchInfo { get; set; }

    /// <summary>
    /// Тип ошибки
    /// </summary>
    public LessonValidationErrorType ErrorType { get; set; }

    /// <summary>
    /// Код ошибки
    /// </summary>
    public LessonPolicyViolationCode Code { get; set; }

    /// <summary>
    /// Оказавшие влияние сущности
    /// </summary>
    public LessonPolicyViolationTarget[] Targets { get; set; } = [];

    /// <summary>
    /// День недели и отрезок времени, оказавшие влияние
    /// </summary>
    public DayOfWeekTimeInterval? DayOfWeekTimeInterval { get; set; }

    /// <summary>
    /// Дата и отрезок времени, в которых произошло нарушение
    /// </summary>
    public DateWithTimeInterval? Timestamp { get; set; }
}