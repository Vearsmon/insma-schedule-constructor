using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Предпочтение преподавателя
/// </summary>
public class TeacherPreference : IModelWithId
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
    /// Преподаватель
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Преподаватель
    /// </summary>
    public Teacher Teacher { get; set; } = null!;

    /// <summary>
    /// Аудитория
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// Аудитория
    /// </summary>
    public Room? Room { get; set; }

    /// <summary>
    /// Интервал времени в день недели
    /// </summary>
    public DayOfWeekTimeInterval? DayOfWeekTimeInterval { get; set; }

    /// <summary>
    /// Вид предпочтения преподавателя
    /// </summary>
    public TeacherPreferenceType? TeacherPreferenceType { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }
}