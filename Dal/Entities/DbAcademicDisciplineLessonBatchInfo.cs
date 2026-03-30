using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Common;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Информация о созданных через академическую дисциплину занятиях
/// </summary>
public class DbAcademicDisciplineLessonBatchInfo : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Академическая группа
    /// </summary>
    public Guid StudentGroupId { get; set; }

    /// <summary>
    /// Академическая группа
    /// </summary>
    public DbStudentGroup StudentGroup { get; set; } = null!;

    /// <summary>
    /// Преподаватель
    /// </summary>
    public Guid? TeacherId { get; set; }

    /// <summary>
    /// Преподаватель
    /// </summary>
    public DbTeacher? Teacher { get; set; }

    /// <summary>
    /// Аудитория
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// Аудитория
    /// </summary>
    public DbRoom? Room { get; set; }

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
    public int HoursCost { get; set; }
}