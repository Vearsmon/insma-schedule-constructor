using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Тип ошибки валидации занятий
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LessonValidationErrorType
{
    /// <summary>
    /// Предупреждение
    /// </summary>
    [Description("Предупреждение")]
    [SortEnumOrder(1)]
    Warning = 1,

    /// <summary>
    /// Ошибка
    /// </summary>
    [Description("Ошибка")]
    [SortEnumOrder(2)]
    Error = 2,
}