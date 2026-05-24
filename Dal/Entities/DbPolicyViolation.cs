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
    public Guid LessonId { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public DbLesson Lesson { get; set; } = null!;

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
}