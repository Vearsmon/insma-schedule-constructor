using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

public class LessonBatchInfo : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Академические группы
    /// </summary>
    public StudentGroup[] StudentGroups { get; set; } = [];

    /// <summary>
    /// Преподаватели
    /// </summary>
    public Teacher[] Teachers { get; set; } = [];

    /// <summary>
    /// Аудитории
    /// </summary>
    public Room[] Rooms { get; set; } = [];

    /// <summary>
    /// Отрезки времени занятий по дням недели
    /// </summary>
    public DayOfWeekTimeInterval[] DayOfWeekTimeIntervals { get; set; } = [];

    /// <summary>
    /// Вид повторения занятий
    /// </summary>
    public DisciplineLessonRepeatType RepeatType { get; set; }

    /// <summary>
    /// Отрезок дат начала и конца занятий
    /// </summary>
    public DateInterval DateInterval { get; set; } = null!;

    /// <summary>
    /// Занятие допускает совмещение
    /// </summary>
    public bool AllowCombining { get; set; }

    /// <summary>
    /// Вес для всех занятий в часах
    /// </summary>
    public int? HoursCost { get; set; }

    /// <summary>
    /// Требуемое количество часов освоения занятий указанного вида
    /// </summary>
    public int? TotalHoursCount { get; set; }
}