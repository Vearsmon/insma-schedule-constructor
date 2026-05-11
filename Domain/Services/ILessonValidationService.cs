using Domain.Dto;
using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Services;

public interface ILessonValidationService
{
    Task<LessonPolicyViolation[]> ValidateAsync(Lesson[] lessons);

    Task DeleteViolationLinksAsync(Guid[] ids);

    void BuildPolicyViolations(List<LessonPolicyViolation> lessonPolicyViolations,
        Dictionary<Guid, List<Guid>> studentGroupHierarchyIdsByStudentGroupId,
        Lesson[] conflictingLessons,
        Lesson? lesson,
        Guid[] teacherIds,
        Guid[] roomIds,
        TeacherPreference[] conflictingTeacherPreferences,
        bool includeTiming = false);

    Task<LessonSeriesConflictDto[]> FillValidationMessages(Lesson[] lessons);

    public void ValidateAcademicDisciplineStudentGroupMatch(Lesson? lesson,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        StudentGroup[] studentGroups);

    public void ValidateAcademicDisciplineTypeMatch(Lesson? lesson,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        AcademicDisciplineType lessonAcademicDisciplineType);

    void ValidateLessonConflictByGroup(Lesson? lesson,
        Lesson[] conflictingByGroupLessons,
        List<LessonPolicyViolation> violations,
        Guid[] hierarchyIds,
        bool includeTiming = false);

    void ValidateLessonConflictByTeacher(
        Lesson? lesson,
        Guid[] teacherIds,
        Lesson[] conflictingByTeacherLessons,
        List<LessonPolicyViolation> violations,
        bool includeTiming = false);

    void ValidateTeacherPreferenceConflict(
        Lesson? lesson,
        TeacherPreference[] conflictingTeacherPreferences,
        List<LessonPolicyViolation> violations,
        bool includeTiming = false);

    void ValidateLessonConflictByRoom(Lesson? lesson,
        Guid[] roomIds,
        Lesson[] conflictingByRoomLessons,
        List<LessonPolicyViolation> violations,
        bool includeTiming = false);

    Task<LessonValidationMessageBatchDto[]> GetValidationResultMessageAsync(LessonPolicyViolation[] violations,
        Dictionary<Guid, int>? currentBatchLessonsTotalHoursByLessonId = null);

    Task RemovePolicyViolations(Guid[] lessonIds, LessonPolicyViolationCode[] validationCodes);

    Task RemovePolicyViolations(Guid academicDisciplineId);
}