using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Тип аудитории
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomType
{
    /// <summary>
    /// Стандартная
    /// </summary>
    [Description("Стандартная")]
    [SortEnumOrder(1)]
    Standard = 1,

    /// <summary>
    /// Мультимедийная
    /// </summary>
    [Description("Мультимедийная")]
    [SortEnumOrder(2)]
    Multimedia = 2,

    /// <summary>
    /// Лабораторная
    /// </summary>
    [Description("Лабораторная")]
    [SortEnumOrder(3)]
    Laboratory = 3,

    /// <summary>
    /// Амфитеатр
    /// </summary>
    [Description("Амфитеатр")]
    [SortEnumOrder(4)]
    Amphitheater = 4,
}