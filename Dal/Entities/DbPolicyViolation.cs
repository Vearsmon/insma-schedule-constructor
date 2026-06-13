using Domain.Models.Common;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Нарушение валидации
/// </summary>
public class DbPolicyViolation : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public Guid? LessonId { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public DbLesson? Lesson { get; set; } = null!;

    /// <summary>
    /// Серия занятий
    /// </summary>
    public Guid? LessonBatchInfoId { get; set; }

    /// <summary>
    /// Серия занятий
    /// </summary>
    public DbLessonBatchInfo? LessonBatchInfo { get; set; } = null!;

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
    public ICollection<DbPolicyViolationTarget> Targets { get; set; } = [];

    /// <summary>
    /// День недели и отрезок времени, оказавшие влияние
    /// </summary>
    public DayOfWeekTimeInterval? DayOfWeekTimeInterval { get; set; }

    /// <summary>
    /// Дата и отрезок времени, в которых произошло нарушение
    /// </summary>
    public DateWithTimeInterval? Timestamp { get; set; }
}