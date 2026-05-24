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
    /// Сведения о созданных через академическую дисциплину наборах занятий
    /// </summary>
    public LessonBatchInfo[] LessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }

    public LessonBatchInfo[] GetBatchInfosByType(AcademicDisciplineType type) =>
        LessonBatchInfos.Where(x => x.Type == type).ToArray();
}