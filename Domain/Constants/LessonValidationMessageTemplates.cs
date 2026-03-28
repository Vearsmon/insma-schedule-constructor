namespace Domain.Constants;

public static class LessonValidationMessageTemplates
{
    public const string MismatchedCyphersTemplate =
        "Занятие по дисциплине \"{0}\" не может преподаваться у группы \"{1}\" (шифр группы \"{2}\" отличается от шифра учебного плана дисциплины \"{3}\").";

    public const string MismatchedSemesterNumberTemplate =
        "Занятие по дисциплине \"{0}\" не может преподаваться у группы \"{1}\" (номер семетра группы \"{2}\" отличается от номера семестра дисциплины \"{3}\" в учебном плане).";

    public const string FixedLessonTypeConflictByGroupTemplate =
        "Занятие не может преподаваться в это время у группы, поскольку есть пересечение по времени с другим занятием {0}со статусом \"Закреплено\" у группы {1}, принадлежащей иерархии выбранной для занятия группы.";

    public const string FlexibleLessonTypeConflictByGroupTemplate =
        "Занятие пересекается по времени с другим занятием {0}со статусом \"Может быть перемещено\" у группы {1}, принадлежащей иерархии выбранной для занятия группы.";

    public const string RestrictedTeacherPreferenceTypeConflictTemplate =
        "Занятие не может быть проведено в это время у группы, поскольку отмеченное время идет вразрез с пожеланием преподавателя {0} \"Нет возможности провести занятие\".";

    public const string UndesirableTeacherPreferenceTypeConflictTemplate =
        "Временной отрезок занятия пересекается с временным отрезком в пожелании, отмеченым преподавателем {0} как \"Нежелательный\".";

    public const string FixedLessonTypeConflictByRoomTemplate =
        "Занятие не может преподаваться в это время у группы, поскольку оно пересекается по времени с другим занятием {0}со статусом \"Закреплено\" в этой же аудитории \"{1}\".";

    public const string FlexibleLessonTypeConflictByRoomTemplate =
        "Занятие пересекается по времени с другим занятием {0}со статусом \"Может быть перемещено\" в этой же аудитории \"{1}\".";

    public const string MismatchedAcademicDisciplineTypeTotalHoursCountTemplate =
        "Общее количество часов занятий вида \"{0}\" по дисциплине \"{1}\" ({2}) не совпадает с необходимым ({3}) для группы \"{4}\".";

    public const string MismatchedAcademicDisciplineTypeLessonPerWeekCountTemplate =
        "Общее количество занятий вида \"{0}\" по дисциплине \"{1}\" в неделю ({2}) не совпадает с необходимым ({3}) для группы \"{4}\".";

    public const string MismatchedAcademicDisciplineTypeStudyWeeksCountTemplate =
        "Общее количество недель, в которые проводится занятие вида \"{0}\" по дисциплине \"{1}\", не совпадает с необходимым ({2} из {3}) для группы \"{4}\".";
}