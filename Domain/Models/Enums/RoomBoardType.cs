using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Тип доски в аудитории
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomBoardType
{
    /// <summary>
    /// Меловая
    /// </summary>
    [Description("Меловая")]
    [SortEnumOrder(1)]
    Chalk = 1,

    /// <summary>
    /// Маркерная
    /// </summary>
    [Description("Маркерная")]
    [SortEnumOrder(2)]
    Marker = 2,
}