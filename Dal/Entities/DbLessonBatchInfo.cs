using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Common;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Информация о созданных через академическую дисциплину занятиях
/// </summary>
public class DbLessonBatchInfo : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Академические группы
    /// </summary>
    public ICollection<DbLessonBatchInfoStudentGroup> StudentGroups { get; set; } = [];

    /// <summary>
    /// Преподаватели
    /// </summary>
    public ICollection<DbLessonBatchInfoTeacher> Teachers { get; set; } = [];

    /// <summary>
    /// Аудитории
    /// </summary>
    public ICollection<DbLessonBatchInfoRoom> Rooms { get; set; } = [];

    /// <summary>
    /// Отрезки времени занятий по дням недели
    /// </summary>
    public DayOfWeekTimeInterval[] DayOfWeekTimeIntervals { get; set; } = [];

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
    /// Вес для всех занятий в часах
    /// </summary>
    public int? HoursCost { get; set; }
}