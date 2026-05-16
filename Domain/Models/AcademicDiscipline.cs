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
    /// Сведения о созданных через академическую дисциплину лекционных занятиях
    /// </summary>
    public LessonBatchInfo[] LectureLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Сведения о созданных через академическую дисциплину практических занятиях
    /// </summary>
    public LessonBatchInfo[] PracticeLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Сведения о созданных через академическую дисциплину лабораторных занятиях
    /// </summary>
    public LessonBatchInfo[] LabLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Сведения о созданных через академическую дисциплину экзаменах
    /// </summary>
    public LessonBatchInfo[] ExamLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Сведения о созданных через академическую дисциплину зачетах
    /// </summary>
    public LessonBatchInfo[] TestLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }

    public LessonBatchInfo[] GetBatchInfosByType(AcademicDisciplineType type) => type switch
    {
        AcademicDisciplineType.Lecture => LectureLessonBatchInfos,
        AcademicDisciplineType.Practice => PracticeLessonBatchInfos,
        AcademicDisciplineType.Lab => LabLessonBatchInfos,
        AcademicDisciplineType.Exam => ExamLessonBatchInfos,
        AcademicDisciplineType.Test => TestLessonBatchInfos,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public LessonBatchInfo[] GetAllBatchInfos() => LectureLessonBatchInfos
        .Concat(PracticeLessonBatchInfos)
        .Concat(LabLessonBatchInfos)
        .Concat(ExamLessonBatchInfos)
        .Concat(TestLessonBatchInfos)
        .ToArray();
}