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
    /// Шифр направления
    /// </summary>
    [StringLength(8)]
    public string Cypher { get; set; } = null!;

    /// <summary>
    /// Номер семестра
    /// </summary>
    public int SemesterNumber { get; set; }

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
    /// Число учебных недель, в течение которых осваиваются лекционные занятия
    /// </summary>
    public int? LectureStudyWeeksCount { get; set; }

    /// <summary>
    /// Требуемое число лекционных занятий в неделю
    /// </summary>
    public int? LectureLessonsPerWeekCount { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину лекционных занятиях
    /// </summary>
    public Guid? AcademicDisciplineLectureLessonBatchInfoId { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину лекционных занятиях
    /// </summary>
    public DbAcademicDisciplineLessonBatchInfo? AcademicDisciplineLectureLessonBatchInfo { get; set; }

    /// <summary>
    /// Поддерживает назначение практических занятий
    /// </summary>
    public bool IsPracticeLessonsAllowed { get; set; }

    /// <summary>
    /// Требуемое количество часов освоения практических занятий
    /// </summary>
    public int? PracticeTotalHoursCount { get; set; }

    /// <summary>
    /// Число учебных недель, в течение которых осваиваются практические занятия
    /// </summary>
    public int? PracticeStudyWeeksCount { get; set; }

    /// <summary>
    /// Требуемое число практических занятий в неделю
    /// </summary>
    public int? PracticeLessonsPerWeekCount { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину практических занятиях
    /// </summary>
    public Guid? AcademicDisciplinePracticeLessonBatchInfoId { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину практических занятиях
    /// </summary>
    public DbAcademicDisciplineLessonBatchInfo? AcademicDisciplinePracticeLessonBatchInfo { get; set; }

    /// <summary>
    /// Поддерживает назначение лабораторных занятий
    /// </summary>
    public bool IsLabLessonsAllowed { get; set; }

    /// <summary>
    /// Требуемое количество часов освоения лабораторных занятий
    /// </summary>
    public int? LabTotalHoursCount { get; set; }

    /// <summary>
    /// Число учебных недель, в течение которых осваиваются лабораторные занятия
    /// </summary>
    public int? LabStudyWeeksCount { get; set; }

    /// <summary>
    /// Требуемое число лабораторных занятий в неделю
    /// </summary>
    public int? LabLessonsPerWeekCount { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину лабораторных занятиях
    /// </summary>
    public Guid? AcademicDisciplineLabLessonBatchInfoId { get; set; }

    /// <summary>
    /// Информация о созданных через академическую дисциплину лабораторных занятиях
    /// </summary>
    public DbAcademicDisciplineLessonBatchInfo? AcademicDisciplineLabLessonBatchInfo { get; set; }

    /// <summary>
    /// Дисциплина предусматривает проведение экзамена
    /// </summary>
    public bool HasExam { get; set; }

    /// <summary>
    /// Дисциплина предусматривает проведение зачета
    /// </summary>
    public bool HasTest { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    [MaxLength(255)]
    public string? Comment { get; set; }
}