using Domain.Models.Enums;

namespace Domain.Models;

public class LessonValidationPayload
{
    /// <summary>
    /// Оказавшая влияние академическая дисциплина
    /// </summary>
    public Guid? AffectedByAcademicDisciplineId { get; set; }

    /// <summary>
    /// Оказавшая влияние академическая дисциплина
    /// </summary>
    public AcademicDiscipline? AffectedByAcademicDiscipline { get; set; }

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
    public Lesson? AffectedByLesson { get; set; }

    /// <summary>
    /// Оказавшая влияние академическая группа
    /// </summary>
    public Guid? AffectedByStudentGroupId { get; set; }

    /// <summary>
    /// Оказавшая влияние академическая группа
    /// </summary>
    public StudentGroup? AffectedByStudentGroup { get; set; }

    /// <summary>
    /// Оказавший влияние преподаватель
    /// </summary>
    public Guid? AffectedByTeacherId { get; set; }

    /// <summary>
    /// Оказавший влияние преподаватель
    /// </summary>
    public Teacher? AffectedByTeacher { get; set; }

    /// <summary>
    /// Оказавшее влияние пожелание преподавателя
    /// </summary>
    public Guid? AffectedByTeacherPreferenceId { get; set; }

    /// <summary>
    /// Оказавшее влияние пожелание преподавателя
    /// </summary>
    public TeacherPreference? AffectedByTeacherPreference { get; set; }
}