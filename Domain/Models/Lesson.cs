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
    /// Академические группы
    /// </summary>
    public StudentGroup[] StudentGroups { get; set; } = [];

    /// <summary>
    /// Преподаватель
    /// </summary>
    public Teacher[] Teachers { get; set; } = [];

    /// <summary>
    /// Аудитория
    /// </summary>
    public Room[] Rooms { get; set; } = [];

    /// <summary>
    /// Назначение дня недели с отрезком времени
    /// </summary>
    public Guid? DayOfWeekTimeIntervalAssignmentId { get; set; }

    /// <summary>
    /// Назначение дня недели с отрезком времени
    /// </summary>
    public DayOfWeekTimeIntervalAssignment? DayOfWeekTimeIntervalAssignment { get; set; }

    /// <summary>
    /// Дата с временным отрезком проведения занятия
    /// </summary>
    public DateWithTimeInterval? DateWithTimeInterval { get; set; }

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
    public Guid LessonBatchInfoId { get; set; }

    /// <summary>
    /// Сведения о созданных занятиях, к которым относится данное
    /// </summary>
    public LessonBatchInfo LessonBatchInfo { get; set; } = null!;

    /// <summary>
    /// Сообщения валидации
    /// </summary>
    public LessonPolicyViolation[] Violations { get; set; } = [];
}