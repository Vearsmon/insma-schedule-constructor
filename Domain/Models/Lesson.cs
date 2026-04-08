using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Учебное занятие
/// </summary>
public class Lesson : IModelWithId
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
    /// Дисциплина в учебном плане
    /// </summary>
    public Guid? AcademicDisciplineId { get; set; }

    /// <summary>
    /// Дисциплина в учебном плане
    /// </summary>
    public AcademicDiscipline? AcademicDiscipline { get; set; }

    /// <summary>
    /// Вид занятия, проводимого по дисциплине в учебном плане
    /// </summary>
    public AcademicDisciplineType? AcademicDisciplineType { get; set; }

    /// <summary>
    /// Академические группы
    /// </summary>
    public StudentGroup[] StudentGroups { get; set; } = [];

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
    /// Дата с временным отрезком проведения занятия
    /// </summary>
    public DateWithTimeInterval? DateWithTimeInterval { get; set; }

    /// <summary>
    /// Подвижность занятия
    /// </summary>
    public LessonFlexibilityType FlexibilityType { get; set; }

    /// <summary>
    /// Занятие допускает совмещение
    /// </summary>
    public bool AllowCombining { get; set; }

    /// <summary>
    /// Вес занятия в часах
    /// </summary>
    public int HoursCost { get; set; }

    /// <summary>
    /// Сообщения валидации
    /// </summary>
    public LessonValidationMessage[] ValidationMessages { get; set; } = [];
}