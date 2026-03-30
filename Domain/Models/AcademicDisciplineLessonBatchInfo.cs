using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

public class AcademicDisciplineLessonBatchInfo : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Академическая группа
    /// </summary>
    public Guid StudentGroupId { get; set; }

    /// <summary>
    /// Академическая группа
    /// </summary>
    public StudentGroup StudentGroup { get; set; } = null!;

    /// <summary>
    /// Преподаватель
    /// </summary>
    public Guid? TeacherId { get; set; }

    /// <summary>
    /// Преподаватель
    /// </summary>
    public Teacher? Teacher { get; set; }

    /// <summary>
    /// Аудитория
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// Аудитория
    /// </summary>
    public Room? Room { get; set; }

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
    public int HoursCost { get; set; }
}