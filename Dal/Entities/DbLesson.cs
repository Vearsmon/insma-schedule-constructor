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
    public int HoursCost { get; set; }

    /// <summary>
    /// Занятие допускает совмещение
    /// </summary>
    public bool AllowCombining { get; set; }

    /// <summary>
    /// Занятие создано через определение дисциплины
    /// </summary>
    public bool CreatedFromDiscipline { get; set; }

    /// <summary>
    /// Сообщения валидации
    /// </summary>
    public ICollection<DbLessonValidationMessage> ValidationMessages { get; set; } = [];
}