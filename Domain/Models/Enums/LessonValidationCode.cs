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
    RestrictedTeacherPreferenceTypeConflict = 6,
    UndesirableTeacherPreferenceTypeConflict = 7,
    FixedLessonTypeConflictByRoom = 8,
    FlexibleLessonTypeConflictByRoom = 9,
    MismatchedAcademicDisciplineTypeTotalHoursCount = 10,
    MismatchedAcademicDisciplineTypeLessonPerWeekCount = 11,
    MismatchedAcademicDisciplineTypeStudyWeeksCount = 12,
}