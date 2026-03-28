using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Вид предпочтения преподавателя
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TeacherPreferenceType
{
    /// <summary>
    /// Запрещено
    /// </summary>
    [Description("Запрещено")]
    [SortEnumOrder(1)]
    Restricted = 1,

    /// <summary>
    /// Нежелательно
    /// </summary>
    [Description("Нежелательно")]
    [SortEnumOrder(2)]
    Undesirable = 2,

    /// <summary>
    /// Предпочтительно
    /// </summary>
    [Description("Предпочтительно")]
    [SortEnumOrder(3)]
    Preferred = 3,
}