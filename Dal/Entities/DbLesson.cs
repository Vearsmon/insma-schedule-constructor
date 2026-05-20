using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Учебное занятие
/// </summary>
public class DbLesson : IDbEntityWithId
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
    /// Дисциплина в учебном плане
    /// </summary>
    public Guid? AcademicDisciplineId { get; set; }

    /// <summary>
    /// Дисциплина в учебном плане
    /// </summary>
    public DbAcademicDiscipline? AcademicDiscipline { get; set; }

    /// <summary>
    /// Вид занятия, проводимого по дисциплине в учебном плане
    /// </summary>
    public AcademicDisciplineType? AcademicDisciplineType { get; set; }

    /// <summary>
    /// Академические группы
    /// </summary>
    public ICollection<DbStudentGroup> StudentGroups { get; set; } = [];

    /// <summary>
    /// Преподаватели
    /// </summary>
    public ICollection<DbTeacher> Teachers { get; set; } = [];

    /// <summary>
    /// Аудитории
    /// </summary>
    public ICollection<DbRoom> Rooms { get; set; } = [];

    /// <summary>
    /// Назначение дня недели с отрезком времени
    /// </summary>
    public Guid? DayOfWeekTimeIntervalAssignmentId { get; set; }

    /// <summary>
    /// Назначение дня недели с отрезком времени
    /// </summary>
    public DbDayOfWeekTimeIntervalAssignment? DayOfWeekTimeIntervalAssignment { get; set; }

    /// <summary>
    /// Дата проведения занятия
    /// </summary>
    [Column(TypeName = DbDataTypes.Date)]
    public DateOnly? Date { get; set; }

    /// <summary>
    /// Время проведения занятия, с
    /// </summary>
    [Column(TypeName = DbDataTypes.Time)]
    public TimeOnly? TimeFrom { get; set; }

    /// <summary>
    /// Время проведения занятия, по
    /// </summary>
    [Column(TypeName = DbDataTypes.Time)]
    public TimeOnly? TimeTo { get; set; }

    /// <summary>
    /// Подвижность занятия
    /// </summary>
    public LessonFlexibilityType FlexibilityType { get; set; }

    /// <summary>
    /// Вес занятия в часах
    /// </summary>
    public int? HoursCost { get; set; }

    /// <summary>
    /// Занятие допускает совмещение
    /// </summary>
    public bool AllowCombining { get; set; }

    /// <summary>
    /// Занятие откреплено от своего шаблона
    /// </summary>
    public bool DetachedFromBatch { get; set; }

    /// <summary>
    /// Сведения о созданных занятиях, к которым относится данное
    /// </summary>
    public Guid? LessonBatchInfoId { get; set; }

    /// <summary>
    /// Сведения о созданных занятиях, к которым относится данное
    /// </summary>
    public DbLessonBatchInfo? LessonBatchInfo { get; set; }

    /// <summary>
    /// Сообщения валидации
    /// </summary>
    public ICollection<DbLessonPolicyViolation> Violations { get; set; } = [];
}