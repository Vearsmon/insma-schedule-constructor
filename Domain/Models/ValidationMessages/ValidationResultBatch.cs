namespace Domain.Models.ValidationMessages;

public sealed class ValidationResultBatch
{
    private ValidationResultBatch(bool isValid, ValidationMessage[] errorMessages)
    {
        IsValid = isValid;
        ErrorMessages = errorMessages;
    }

    public bool IsValid { get; }

    public ValidationMessage[] ErrorMessages { get; }

    public static ValidationResultBatch Success()
    {
        return new ValidationResultBatch(true, Array.Empty<ValidationMessage>());
    }

    public static ValidationResultBatch Errors(params ValidationMessage[] errors)
    {
        return new ValidationResultBatch(false, errors);
    }
}
