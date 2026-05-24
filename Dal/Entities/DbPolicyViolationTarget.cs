using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Оказавшая влияние на появление нарушения сущность
/// </summary>
public class DbPolicyViolationTarget : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Нарушение валидации
    /// </summary>
    public Guid ViolationId { get; set; }

    /// <summary>
    /// Нарушение валидации
    /// </summary>
    public DbPolicyViolation Violation { get; set; } = null!;

    /// <summary>
    /// Оказавшая влияние сущность
    /// </summary>
    public Guid TargetId { get; set; }

    /// <summary>
    /// Тип оказавшей влияние сущности
    /// </summary>
    public LessonPolicyViolationTargetType TargetType { get; set; }
}