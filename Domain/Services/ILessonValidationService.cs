using Domain.Dto;
using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Services;

public interface ILessonValidationService
{
    Task<LessonValidationResult> ValidateAsync(Lesson lesson);

    void BuildValidationMessages(List<LessonValidationMessage> lessonValidationMessages,
        Dictionary<Guid, List<Guid>> studentGroupHierarchyIdsByStudentGroupId,
        Lesson[] conflictingLessons,
        Lesson? lesson,
        Guid[] teacherIds,
        Guid[] roomIds,
        TeacherPreference[] conflictingTeacherPreferences,
        Dictionary<Guid, List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
        bool includeTiming = false);

    Task<LessonSeriesConflictDto[]> FillValidationMessages(Lesson[] lessons);

    public void ValidateAcademicDisciplineStudentGroupMatch(List<LessonValidationMessage> validationMessages,
        AcademicDiscipline saveDtoAcademicDiscipline,
        StudentGroup[] saveDtoStudentGroups);

    public void ValidateAcademicDisciplineTypeMatch(List<LessonValidationMessage> validationMessages,
        AcademicDiscipline saveDtoAcademicDiscipline,
        AcademicDisciplineType lessonAcademicDisciplineType);

    void ValidateLessonConflictByGroup(Lesson? lesson,
        Lesson[] conflictingByGroupLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
        Guid[] hierarchyIds,
        bool includeTiming = false);

    void ValidateLessonConflictByTeacher(Guid? lessonId,
        LessonFlexibilityType? lessonFlexibilityType,
        Guid[] teacherIds,
        Lesson[] conflictingByTeacherLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
        bool includeTiming = false);

    void ValidateTeacherPreferenceConflict(
        TeacherPreference[] conflictingTeacherPreferences,
        List<LessonValidationMessage> validationMessages,
        bool includeTiming = false);

    void ValidateLessonConflictByRoom(Lesson? lesson,
        Guid[] roomIds,
        Lesson[] conflictingByRoomLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
        bool includeTiming = false);

    Task<string[]> GetValidationResultMessageAsync(LessonValidationMessage[] validationMessage,
        Lesson? lesson = null, Dictionary<(Guid, Guid, Guid), List<Lesson>>? studentGroupAcademicDisciplineLessonsCache = null);

    Task RemoveValidationMessages(Guid[] lessonIds, LessonValidationCode[] validationCodes);

    Task RemoveValidationMessages(Guid academicDisciplineId);
}