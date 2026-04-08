using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Вид повторения занятий
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisciplineLessonRepeatType
{
    /// <summary>
    /// Еженедельно
    /// </summary>
    [Description("Еженедельно")]
    [SortEnumOrder(1)]
    Weekly = 1,

    /// <summary>
    /// Четные недели
    /// </summary>
    [Description("Четные недели")]
    [SortEnumOrder(2)]
    EvenWeeks = 2,

    /// <summary>
    /// Нечетные недели
    /// </summary>
    [Description("Нечетные недели")]
    [SortEnumOrder(3)]
    OddWeeks = 3,

    /// <summary>
    /// Единожды
    /// </summary>
    [Description("Единожды")]
    [SortEnumOrder(4)]
    Once = 4,
}