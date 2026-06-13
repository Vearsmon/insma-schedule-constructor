using Domain.Dto;
using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Services;

public interface ILessonBatchValidationService
{
    Task<LessonPolicyViolation[]> ValidateBatchAsync(LessonBatchInfo lessonBatchInfo);

    void BuildPolicyViolations(List<LessonPolicyViolation> lessonPolicyViolations,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
        Dictionary<Guid, List<Guid>> studentGroupHierarchyIdsByStudentGroupId,
        Lesson[] conflictingLessons,
        LessonBatchInfo[] conflictingBatches,
        LessonBatchInfo batch,
        Guid[] teacherIds,
        Guid[] roomIds,
        TeacherPreference[] conflictingTeacherPreferences,
        Schedule schedule,
        bool includeTiming = false);

    Task<LessonSeriesConflictDto[]> FillValidationMessages(LessonBatchInfo[] batches);

    public void ValidateAcademicDisciplineStudentGroupMatch(LessonBatchInfo? batch,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        StudentGroup[] studentGroups);

    public void ValidateAcademicDisciplineTypeMatch(LessonBatchInfo? batch,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        AcademicDisciplineType lessonAcademicDisciplineType);

    void ValidateConflictByGroup(
        LessonBatchInfo batch,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
        Lesson[] conflictingByGroupLessons,
        LessonBatchInfo[] conflictingByGroupBatches,
        List<LessonPolicyViolation> violations,
        Guid[] hierarchyIds,
        Schedule schedule,
        bool includeTiming = false);

    void ValidateTeacherPreferenceConflict(
        LessonBatchInfo batch,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
        TeacherPreference[] conflictingTeacherPreferences,
        List<LessonPolicyViolation> violations,
        Schedule schedule,
        bool includeTiming = false);
}