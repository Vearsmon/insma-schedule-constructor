using System.Text;
using Domain.Exceptions;
using Domain.Models.ValidationMessages;

namespace Domain.Helpers;

public static class ExceptionExtensions
{
    public static string FlattenException(this Exception? exception, Guid id)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"error-id: ${id} ");
        stringBuilder.Append(exception.FlattenException());
        return stringBuilder.ToString();
    }

    public static string FlattenException(this Exception? exception)
    {
        var stringBuilder = new StringBuilder();
        while (exception != null)
        {
            stringBuilder.Append(exception);
            exception = exception.InnerException;
        }

        return stringBuilder.ToString();
    }

    public static IEnumerable<string> ExpandMessages(this Exception exception)
    {
        var current = exception;
        while (current != null)
        {
            yield return current.Message;
            current = current.InnerException;
        }
    }

    public static string GetValidationError(this ServiceException exception)
    {
        var validationErrors = exception.ValidationMessages.Where(x => x.ValidationLevel == ValidationLevel.Error).ToArray();
        if (validationErrors.Length > 0)
        {
            return string.Join(Environment.NewLine, validationErrors.Select(x => x.Message).ToArray());
        }

        if (exception.ErrorList.Length > 0)
        {
            return string.Join(Environment.NewLine, exception.ErrorList);
        }

        return string.IsNullOrEmpty(exception.Title) ? $"{exception.Message}" : $"{exception.Message}. {exception.Title}";
    }

    public static string GetValidationError(this IEnumerable<ValidationMessage> validationMessages)
    {
        var validationErrors = validationMessages.Where(x => x.ValidationLevel == ValidationLevel.Error).ToArray();
        return validationErrors.Length > 0
            ? string.Join(Environment.NewLine, validationErrors.Select(x => x.Message).ToArray())
            : string.Empty;
    }
}
