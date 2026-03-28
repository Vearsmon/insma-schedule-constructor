using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Helpers;

public static class LessonValidationHelper
{
    public static void AddError(this List<LessonValidationMessage> messages,
        LessonValidationPayload payload, LessonValidationCode code)
    {
        messages.AddErrorIf(true, payload, code);
    }

    public static void AddErrorIf(this List<LessonValidationMessage> messages,
        bool condition, LessonValidationPayload payload, LessonValidationCode code)
    {
        messages.AddValidationMessageIf(condition, payload, LessonValidationErrorType.Error, code);
    }

    public static void AddWarning(this List<LessonValidationMessage> messages,
        LessonValidationPayload payload, LessonValidationCode code)
    {
        messages.AddWarningIf(true, payload, code);
    }

    public static void AddWarningIf(this List<LessonValidationMessage> messages,
        bool condition, LessonValidationPayload payload, LessonValidationCode code)
    {
        messages.AddValidationMessageIf(condition, payload, LessonValidationErrorType.Warning, code);
    }

    private static void AddValidationMessageIf(this List<LessonValidationMessage> messages,
        bool condition, LessonValidationPayload payload, LessonValidationErrorType type, LessonValidationCode code)
    {
        if (condition)
        {
            messages.Add(new LessonValidationMessage
            {
                ErrorType = type,
                Code = code,
                Payload = payload,
            });
        }
    }
}