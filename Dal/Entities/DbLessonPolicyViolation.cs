using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Сообщения валидации
/// </summary>
public class DbLessonPolicyViolation : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public Guid LessonId { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public DbLesson Lesson { get; set; } = null!;

    /// <summary>
    /// Тип ошибки
    /// </summary>
    public LessonValidationErrorType ErrorType { get; set; }

    /// <summary>
    /// Код ошибки
    /// </summary>
    public LessonPolicyViolationCode Code { get; set; }

    /// <summary>
    /// Оказавшая влияние академическая дисциплина
    /// </summary>
    public Guid? AffectedByAcademicDisciplineId { get; set; }

    /// <summary>
    /// Оказавшая влияние академическая дисциплина
    /// </summary>
    public DbAcademicDiscipline? AffectedByAcademicDiscipline { get; set; }

    /// <summary>
    /// Оказавший влияние вид занятий академической дисциплины
    /// </summary>
    public AcademicDisciplineType? AffectedByAcademicDisciplineType { get; set; }

    /// <summary>
    /// Оказавшее влияние занятие
    /// </summary>
    public Guid? AffectedByLessonId { get; set; }

    /// <summary>
    /// Оказавшее влияние занятие
    /// </summary>
    public DbLesson? AffectedByLesson { get; set; }

    /// <summary>
    /// Оказавшая влияние академическая группа
    /// </summary>
    public Guid? AffectedByStudentGroupId { get; set; }

    /// <summary>
    /// Оказавшая влияние академическая группа
    /// </summary>
    public DbStudentGroup? AffectedByStudentGroup { get; set; }

    /// <summary>
    /// Оказавший влияние преподаватель
    /// </summary>
    public Guid? AffectedByTeacherId { get; set; }

    /// <summary>
    /// Оказавший влияние преподаватель
    /// </summary>
    public DbTeacher? AffectedByTeacher { get; set; }

    /// <summary>
    /// Оказавшая влияние аудитория
    /// </summary>
    public Guid? AffectedByRoomId { get; set; }

    /// <summary>
    /// Оказавшая влияние аудитория
    /// </summary>
    public DbRoom? AffectedByRoom { get; set; }

    /// <summary>
    /// Оказавшее влияние пожелание преподавателя
    /// </summary>
    public Guid? AffectedByTeacherPreferenceId { get; set; }

    /// <summary>
    /// Оказавшее влияние пожелание преподавателя
    /// </summary>
    public DbTeacherPreference? AffectedByTeacherPreference { get; set; }
}