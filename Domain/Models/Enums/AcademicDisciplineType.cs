using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.Attributes;

namespace Domain.Models.Enums;

/// <summary>
/// Вид занятий, проводимых по дисциплине
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AcademicDisciplineType
{
    /// <summary>
    /// Лекция
    /// </summary>
    [Description("Лекция")]
    [SortEnumOrder(1)]
    Lecture = 1,

    /// <summary>
    /// Практическое занятие
    /// </summary>
    [Description("Практическое занятие")]
    [SortEnumOrder(2)]
    Practice = 2,

    /// <summary>
    /// Лабораторное занятие
    /// </summary>
    [Description("Лабораторное занятие")]
    [SortEnumOrder(3)]
    Lab = 3,

    /// <summary>
    /// Экзамен
    /// </summary>
    [Description("Экзамен")]
    [SortEnumOrder(4)]
    Exam = 4,

    /// <summary>
    /// Зачет
    /// </summary>
    [Description("Зачет")]
    [SortEnumOrder(5)]
    Test = 5,
}