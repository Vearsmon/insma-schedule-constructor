using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Вид дисциплины
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AcademicDisciplineTargetType
{
    /// <summary>
    /// Общего назначения
    /// </summary>
    [Description("Общего назначения")]
    [SortEnumOrder(1)]
    General = 1,

    /// <summary>
    /// Профильная
    /// </summary>
    [Description("Профильная")]
    [SortEnumOrder(2)]
    Specialized = 2,

    /// <summary>
    /// По выбору
    /// </summary>
    [Description("По выбору")]
    [SortEnumOrder(3)]
    ByChoice = 3,

    /// <summary>
    /// Факультативная
    /// </summary>
    [Description("Факультативная")]
    [SortEnumOrder(4)]
    Optional = 4,
}