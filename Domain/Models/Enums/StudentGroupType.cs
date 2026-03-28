using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Тип академической группы
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StudentGroupType
{
    /// <summary>
    /// Поток
    /// </summary>
    [Description("Поток")]
    [SortEnumOrder(1)]
    Thread = 1,

    /// <summary>
    /// Группа
    /// </summary>
    [Description("Группа")]
    [SortEnumOrder(2)]
    Group = 2,

    /// <summary>
    /// Подгруппа
    /// </summary>
    [Description("Подгруппа")]
    [SortEnumOrder(3)]
    SemiGroup = 3,
}