using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Запись в учебном плане
/// </summary>
public class AcademicDiscipline : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public Schedule Schedule { get; set; } = null!;

    /// <summary>
    /// Название дисциплины
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Названия, ассоциируемые с академической дисциплиной
    /// </summary>
    public string[] AssociatedNames { get; set; } = [];

    /// <summary>
    /// Номер семестра
    /// </summary>
    public int? SemesterNumber { get; set; }

    /// <summary>
    /// Вид дисциплины
    /// </summary>
    public AcademicDisciplineTargetType AcademicDisciplineTargetType { get; set; }

    /// <summary>
    /// Поддерживаемые виды занятий
    /// </summary>
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];

    /// <summary>
    /// Нагрузка по лекционным занятиям
    /// </summary>
    public AcademicDisciplinePayload? LecturePayload { get; set; }

    /// <summary>
    /// Нагрузка по практическим занятиям
    /// </summary>
    public AcademicDisciplinePayload? PracticePayload { get; set; }

    /// <summary>
    /// Нагрузка по лабораторным занятиям
    /// </summary>
    public AcademicDisciplinePayload? LabPayload { get; set; }

    /// <summary>
    /// Нагрузка по экзаменам
    /// </summary>
    public AcademicDisciplinePayload? ExamPayload { get; set; }

    /// <summary>
    /// Нагрузка по зачетам
    /// </summary>
    public AcademicDisciplinePayload? TestPayload { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }

    public AcademicDisciplinePayload? GetPayloadByType(AcademicDisciplineType type) => type switch
    {
        AcademicDisciplineType.Lecture => LecturePayload,
        AcademicDisciplineType.Practice => PracticePayload,
        AcademicDisciplineType.Lab => LabPayload,
        AcademicDisciplineType.Exam => ExamPayload,
        AcademicDisciplineType.Test => TestPayload,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}