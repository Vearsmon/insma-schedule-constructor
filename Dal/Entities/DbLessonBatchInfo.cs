using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Информация о созданных через академическую дисциплину занятиях
/// </summary>
public class DbLessonBatchInfo : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Академическая дисциплина
    /// </summary>
    public Guid AcademicDisciplineId { get; set; }

    /// <summary>
    /// Академическая дисциплина
    /// </summary>
    public DbAcademicDiscipline AcademicDiscipline { get; set; } = null!;

    /// <summary>
    /// Вид дисциплины
    /// </summary>
    public AcademicDisciplineType Type { get; set; }

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
    /// Количество занятий в неделю
    /// </summary>
    public int LessonsPerWeekCount { get; set; }

    /// <summary>
    /// Отрезки времени занятий по дням недели
    /// </summary>
    public ICollection<DbDayOfWeekTimeIntervalAssignment> DayOfWeekTimeIntervals { get; set; } = [];

    /// <summary>
    /// Вид повторения занятий
    /// </summary>
    public DisciplineLessonRepeatType RepeatType { get; set; }

    /// <summary>
    /// Дата начала занятий
    /// </summary>
    [Column(TypeName = DbDataTypes.Date)]
    public DateOnly DateFrom { get; set; }

    /// <summary>
    /// Дата завершения занятий
    /// </summary>
    [Column(TypeName = DbDataTypes.Date)]
    public DateOnly DateTo { get; set; }

    /// <summary>
    /// Занятие допускает совмещение
    /// </summary>
    public bool AllowCombining { get; set; }

    /// <summary>
    /// Подвижность занятия
    /// </summary>
    public LessonFlexibilityType FlexibilityType { get; set; }

    /// <summary>
    /// Вес для всех занятий в часах
    /// </summary>
    public int? HoursCost { get; set; }

    /// <summary>
    /// Требуемое количество часов освоения занятий
    /// </summary>
    public int? TotalHoursCount { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Сообщения валидации
    /// </summary>
    public ICollection<DbPolicyViolation> Violations { get; set; } = [];
}