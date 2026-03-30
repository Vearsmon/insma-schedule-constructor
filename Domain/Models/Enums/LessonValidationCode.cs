using System.Text.Json.Serialization;

namespace Domain.Models.Enums;

/// <summary>
/// Коды валидаций для занятий
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LessonValidationCode
{
    MismatchedCyphers = 1,
    MismatchedSemesterNumber = 2,
    MismatchedAcademicDisciplineType = 3,
    FixedLessonTypeConflictByGroup = 4,
    FlexibleLessonTypeConflictByGroup = 5,
    FixedLessonTypeConflictByTeacher = 6,
    FlexibleLessonTypeConflictByTeacher = 7,
    RestrictedTeacherPreferenceTypeConflict = 8,
    UndesirableTeacherPreferenceTypeConflict = 9,
    FixedLessonTypeConflictByRoom = 10,
    FlexibleLessonTypeConflictByRoom = 11,
    MismatchedAcademicDisciplineTypeTotalHoursCount = 12,
    MismatchedAcademicDisciplineTypeLessonPerWeekCount = 13,
    MismatchedAcademicDisciplineTypeStudyWeeksCount = 14,
}