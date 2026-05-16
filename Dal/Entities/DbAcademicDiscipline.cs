using System.ComponentModel.DataAnnotations;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Академическая дисциплина
/// </summary>
public class DbAcademicDiscipline : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public DbSchedule Schedule { get; set; } = null!;

    /// <summary>
    /// Название дисциплины
    /// </summary>
    [MaxLength(255)]
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
    /// Сведения о созданных через академическую дисциплину наборах занятий
    /// </summary>
    public ICollection<DbLessonBatchInfo> LessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Поддерживаемые виды занятий
    /// </summary>
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];

    /// <summary>
    /// Комментарий
    /// </summary>
    [MaxLength(255)]
    public string? Comment { get; set; }
}