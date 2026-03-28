using System.ComponentModel.DataAnnotations;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Предпочтение преподавателя
/// </summary>
public class DbTeacherPreference : IDbEntityWithId
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
    /// Преподаватель
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Преподаватель
    /// </summary>
    public DbTeacher Teacher { get; set; } = null!;

    /// <summary>
    /// Аудитория
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// Аудитория
    /// </summary>
    public DbRoom? Room { get; set; }

    /// <summary>
    /// День недели
    /// </summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>
    /// Отрезок времени, отображающий предпочтение, начало
    /// </summary>
    public TimeOnly? TimeFrom { get; set; }

    /// <summary>
    /// Отрезок времени, отображающий предпочтение, конец
    /// </summary>
    public TimeOnly? TimeTo { get; set; }

    /// <summary>
    /// Вид предпочтения преподавателя
    /// </summary>
    public TeacherPreferenceType? TeacherPreferenceType { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    [MaxLength(255)]
    public string? Comment { get; set; }
}