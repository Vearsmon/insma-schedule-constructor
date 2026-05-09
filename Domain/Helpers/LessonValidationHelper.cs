using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Helpers;

public static class LessonValidationHelper
{
    public static void AddError(this List<LessonPolicyViolation> violations,
        LessonValidationPayload payload, LessonPolicyViolationCode code, Guid? lessonId = null)
    {
        violations.AddErrorIf(true, payload, code, lessonId);
    }

    public static void AddErrorIf(this List<LessonPolicyViolation> violations,
        bool condition, LessonValidationPayload payload, LessonPolicyViolationCode code, Guid? lessonId = null)
    {
        violations.AddValidationMessageIf(condition, payload, LessonValidationErrorType.Error, code, lessonId);
    }

    public static void AddWarning(this List<LessonPolicyViolation> violations,
        LessonValidationPayload payload, LessonPolicyViolationCode code, Guid? lessonId = null)
    {
        violations.AddWarningIf(true, payload, code, lessonId);
    }

    public static void AddWarningIf(this List<LessonPolicyViolation> violations,
        bool condition, LessonValidationPayload payload, LessonPolicyViolationCode code, Guid? lessonId = null)
    {
        violations.AddValidationMessageIf(condition, payload, LessonValidationErrorType.Warning, code, lessonId);
    }

    private static void AddValidationMessageIf(this List<LessonPolicyViolation> violations,
        bool condition, LessonValidationPayload payload, LessonValidationErrorType type, LessonPolicyViolationCode code, Guid? lessonId = null)
    {
        if (condition)
        {
            violations.Add(new LessonPolicyViolation
            {
                LessonId = lessonId ?? Guid.Empty,
                ErrorType = type,
                Code = code,
                Payload = payload,
            });
        }
    }
}