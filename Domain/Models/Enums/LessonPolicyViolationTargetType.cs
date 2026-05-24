using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Models.Enums;

/// <summary>
/// Тип оказавшей влияние на появление ошибки сущности
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LessonPolicyViolationTargetType
{
    /// <summary>
    /// Академическая дисциплина
    /// </summary>
    [Description("Академическая дисциплина")]
    AcademicDiscipline = 1,

    /// <summary>
    /// Занятие
    /// </summary>
    [Description("Занятие")]
    Lesson = 2,

    /// <summary>
    /// Академическая группа
    /// </summary>
    [Description("Академическая группа")]
    StudentGroup = 3,

    /// <summary>
    /// Преподаватель
    /// </summary>
    [Description("Преподаватель")]
    Teacher = 4,

    /// <summary>
    /// Аудитория
    /// </summary>
    [Description("Аудитория")]
    Room = 5,

    /// <summary>
    /// Пожелание преподавателя
    /// </summary>
    [Description("Пожелание преподавателя")]
    TeacherPreference = 6,

    /// <summary>
    /// Набор занятий
    /// </summary>
    [Description("Набор занятий")]
    LessonBatch = 7,
}