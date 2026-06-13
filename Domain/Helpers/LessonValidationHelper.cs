using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Helpers;

public static class LessonValidationHelper
{
    public static void AddError(this List<LessonPolicyViolation> violations,
        LessonPolicyViolationTargetIdentity[] targetIdentities,
        LessonPolicyViolationCode code,
        Guid? lessonId = null,
        Guid? lessonBatchId = null,
        DateWithTimeInterval? violationTimestamp = null,
        DayOfWeekTimeInterval? conflictingDayOfWeekTimeInterval = null)
    {
        violations.AddErrorIf(true, targetIdentities, code, lessonId, lessonBatchId, violationTimestamp, conflictingDayOfWeekTimeInterval);
    }

    public static void AddErrorIf(this List<LessonPolicyViolation> violations,
        bool condition,
        LessonPolicyViolationTargetIdentity[] targetIdentities,
        LessonPolicyViolationCode code,
        Guid? lessonId = null,
        Guid? lessonBatchId = null,
        DateWithTimeInterval? violationTimestamp = null,
        DayOfWeekTimeInterval? conflictingDayOfWeekTimeInterval = null)
    {
        violations.AddValidationMessageIf(condition, targetIdentities, LessonValidationErrorType.Error, code, lessonId,
            lessonBatchId, violationTimestamp, conflictingDayOfWeekTimeInterval);
    }

    public static void AddWarning(this List<LessonPolicyViolation> violations,
        LessonPolicyViolationTargetIdentity[] targetIdentities,
        LessonPolicyViolationCode code,
        Guid? lessonId = null,
        Guid? lessonBatchId = null,
        DateWithTimeInterval? violationTimestamp = null,
        DayOfWeekTimeInterval? conflictingDayOfWeekTimeInterval = null)
    {
        violations.AddWarningIf(true, targetIdentities, code, lessonId, lessonBatchId, violationTimestamp,
            conflictingDayOfWeekTimeInterval);
    }

    public static void AddWarningIf(this List<LessonPolicyViolation> violations,
        bool condition,
        LessonPolicyViolationTargetIdentity[] targetIdentities,
        LessonPolicyViolationCode code,
        Guid? lessonId = null,
        Guid? lessonBatchId = null,
        DateWithTimeInterval? violationTimestamp = null,
        DayOfWeekTimeInterval? conflictingDayOfWeekTimeInterval = null)
    {
        violations.AddValidationMessageIf(condition, targetIdentities, LessonValidationErrorType.Warning, code,
            lessonId, lessonBatchId, violationTimestamp, conflictingDayOfWeekTimeInterval);
    }

    private static void AddValidationMessageIf(this List<LessonPolicyViolation> violations,
        bool condition,
        LessonPolicyViolationTargetIdentity[] targetIdentities,
        LessonValidationErrorType type,
        LessonPolicyViolationCode code,
        Guid? lessonId = null,
        Guid? lessonBatchId = null,
        DateWithTimeInterval? violationTimestamp = null,
        DayOfWeekTimeInterval? conflictingDayOfWeekTimeInterval = null)
    {
        if (condition)
        {
            violations.Add(new LessonPolicyViolation
            {
                LessonId = lessonId,
                LessonBatchInfoId = lessonBatchId,
                ErrorType = type,
                Code = code,
                Targets = targetIdentities
                    .Where(x => x.TargetId.HasValue)
                    .Select(x => new LessonPolicyViolationTarget
                    {
                        TargetId = x.TargetId!.Value,
                        TargetType = x.TargetType,
                    }).ToArray(),
                Timestamp = violationTimestamp,
                DayOfWeekTimeInterval = conflictingDayOfWeekTimeInterval,
            });
        }
    }
}