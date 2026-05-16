namespace Domain.Constants;

public static class LessonPolicyViolationTemplates
{
    public const string MismatchedSemesterNumberTemplate =
        "Номер семетра группы {0} ({1}) отличается от номера семестра дисциплины \"{2}\" ({3}) в учебном плане).";

    public const string MismatchedAcademicDisciplineTypeTemplate =
        "Занятие имеет вид \"{0}\", не поддерживаемый дисциплиной \"{1}\".";

    public const string FixedLessonTypeConflictByGroupTemplate =
        "Пересечение по времени с другим занятием {0}со статусом \"Закреплено\" у группы {1}, {2}.";

    public const string FlexibleLessonTypeConflictByGroupTemplate =
        "Пересечение по времени с другим занятием {0}со статусом \"Может быть перемещено\" у группы {1}, {2}.";

    public const string FixedLessonTypeConflictByTeacherTemplate =
        "Пересечение по времени с другим занятием {0}со статусом \"Закреплено\" с таким же преподавателем {1}.";

    public const string FlexibleLessonTypeConflictByTeacherTemplate =
        "Пересечение по времени с другим занятием {0}со статусом \"Может быть перемещено\" с таким же преподавателем {1}.";

    public const string FixedLessonTypeConflictByRoomTemplate =
        "Пересечение по времени с другим занятием {0}со статусом \"Закреплено\" в этой же аудитории \"{1}\".";

    public const string FlexibleLessonTypeConflictByRoomTemplate =
        "Пересечение по времени с другим занятием {0}со статусом \"Может быть перемещено\" в этой же аудитории \"{1}\".";

    public const string RestrictedTimeTeacherPreferenceTypeConflictTemplate =
        "Отмеченное время идет вразрез с пожеланием преподавателя {0} \"Невозможно провести занятие в это время\".";

    public const string UndesirableTimeTeacherPreferenceTypeConflictTemplate =
        "Отмеченное время идет вразрез с пожеланием преподавателя {0} \"Нежелательное время\".";

    public const string RestrictedRoomTeacherPreferenceTypeConflictTemplate =
        "Отмеченная аудитория идет вразрез с пожеланием преподавателя {0} \"Невозможно провести занятие в этой аудитории\".";

    public const string UndesirableRoomTeacherPreferenceTypeConflictTemplate =
        "Отмеченная аудитория идет вразрез с пожеланием преподавателя {0} \"Нежелательная аудитория\".";

    public const string MismatchedAcademicDisciplineTypeTotalHoursCountTemplate =
        "Общее количество часов по дисциплине \"{0} ({1})\" ({2}ч) не совпадает с необходимым ({3}ч) для группы {4}.";

    public const string LessonPolicyViolationDefaultTemplate = "Количество ошибок занятия: {0}";
}