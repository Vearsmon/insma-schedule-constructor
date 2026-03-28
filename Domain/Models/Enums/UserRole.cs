using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Роль пользователя
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    /// <summary>
    /// Студент
    /// </summary>
    [Description("Студент")]
    [SortEnumOrder(1)]
    Student = 1,

    /// <summary>
    /// Составитель расписания
    /// </summary>
    [Description("Составитель расписания")]
    [SortEnumOrder(2)]
    ScheduleCreator = 2,

    /// <summary>
    /// Преподаватель
    /// </summary>
    [Description("Преподаватель")]
    [SortEnumOrder(3)]
    Teacher = 3,

    /// <summary>
    /// Сотрудник
    /// </summary>
    [Description("Сотрудник")]
    [SortEnumOrder(4)]
    Employee = 4,
}