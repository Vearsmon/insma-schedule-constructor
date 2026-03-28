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
    /// Каждые две недели
    /// </summary>
    [Description("Каждые две недели")]
    [SortEnumOrder(2)]
    EveryTwoWeeks = 2,

    /// <summary>
    /// Единожды
    /// </summary>
    [Description("Единожды")]
    [SortEnumOrder(3)]
    Once = 3,

    /// <summary>
    /// Произвольный выбор
    /// </summary>
    [Description("Произвольный выбор")]
    [SortEnumOrder(4)]
    Custom = 4,
}