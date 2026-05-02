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
    /// Поддерживает назначение лекционных занятий
    /// </summary>
    public bool IsLectureLessonsAllowed { get; set; }

    /// <summary>
    /// Требуемое количество часов освоения лекционных занятий
    /// </summary>
    public int? LectureTotalHoursCount { get; set; }

    /// <summary>
    /// Сведения о созданных через академическую дисциплину лекционных занятиях
    /// </summary>
    public ICollection<DbLessonBatchInfo> AcademicDisciplineLectureLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Поддерживает назначение практических занятий
    /// </summary>
    public bool IsPracticeLessonsAllowed { get; set; }

    /// <summary>
    /// Требуемое количество часов освоения практических занятий
    /// </summary>
    public int? PracticeTotalHoursCount { get; set; }

    /// <summary>
    /// Сведения о созданных через академическую дисциплину практических занятиях
    /// </summary>
    public ICollection<DbLessonBatchInfo> AcademicDisciplinePracticeLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Поддерживает назначение лабораторных занятий
    /// </summary>
    public bool IsLabLessonsAllowed { get; set; }

    /// <summary>
    /// Требуемое количество часов освоения лабораторных занятий
    /// </summary>
    public int? LabTotalHoursCount { get; set; }

    /// <summary>
    /// Сведения о созданных через академическую дисциплину лабораторных занятиях
    /// </summary>
    public ICollection<DbLessonBatchInfo> AcademicDisciplineLabLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Поддерживает назначение экзаменов
    /// </summary>
    public bool IsExamLessonsAllowed { get; set; }

    /// <summary>
    /// Сведения о созданных через академическую дисциплину экзаменах
    /// </summary>
    public ICollection<DbLessonBatchInfo> AcademicDisciplineExamLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Поддерживает назначение зачетов
    /// </summary>
    public bool IsTestLessonsAllowed { get; set; }

    /// <summary>
    /// Сведения о созданных через академическую дисциплину зачетах
    /// </summary>
    public ICollection<DbLessonBatchInfo> AcademicDisciplineTestLessonBatchInfos { get; set; } = [];

    /// <summary>
    /// Комментарий
    /// </summary>
    [MaxLength(255)]
    public string? Comment { get; set; }
}