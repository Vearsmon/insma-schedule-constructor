using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Данные для идентификации оказавшей влияние на появление нарушения сущности
/// </summary>
public record LessonPolicyViolationTargetIdentity(Guid? TargetId, LessonPolicyViolationTargetType TargetType)
{
    /// <summary>
    /// Оказавшая влияние сущность
    /// </summary>
    public Guid? TargetId { get; init; } = TargetId;

    /// <summary>
    /// Тип оказавшей влияние сущности
    /// </summary>
    public LessonPolicyViolationTargetType TargetType { get; init; } = TargetType;
}