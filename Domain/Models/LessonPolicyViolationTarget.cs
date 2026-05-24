using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Оказавшая влияние на появление нарушения сущность
/// </summary>
public class LessonPolicyViolationTarget : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Нарушение валидации
    /// </summary>
    public LessonPolicyViolation Violation { get; set; } = null!;

    /// <summary>
    /// Оказавшая влияние сущность
    /// </summary>
    public Guid TargetId { get; set; }

    /// <summary>
    /// Тип оказавшей влияние сущности
    /// </summary>
    public LessonPolicyViolationTargetType TargetType { get; set; }
}