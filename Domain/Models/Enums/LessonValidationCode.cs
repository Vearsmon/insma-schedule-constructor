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
    RestrictedTimeTeacherPreferenceTypeConflict = 8,
    UndesirableTimeTeacherPreferenceTypeConflict = 9,
    RestrictedRoomTeacherPreferenceTypeConflict = 10,
    UndesirableRoomTeacherPreferenceTypeConflict = 11,
    FixedLessonTypeConflictByRoom = 12,
    FlexibleLessonTypeConflictByRoom = 13,
    MismatchedAcademicDisciplineTypeTotalHoursCount = 14,
    MismatchedAcademicDisciplineTypeLessonPerWeekCount = 15,
    MismatchedAcademicDisciplineTypeStudyWeeksCount = 16,
}