using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Подвижность занятия
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LessonFlexibilityType
{
    /// <summary>
    /// Строго закреплено
    /// </summary>
    [Description("Строго закреплено")]
    [SortEnumOrder(1)]
    Fixed = 1,

    /// <summary>
    /// Может быть перемещено
    /// </summary>
    [Description("Может быть перемещено")]
    [SortEnumOrder(2)]
    Flexible = 2,
}