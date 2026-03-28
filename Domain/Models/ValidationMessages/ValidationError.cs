namespace Domain.Models.ValidationMessages;

public class ValidationError(string code, string message)
{
    public string Code { get; } = code ?? throw new ArgumentNullException(nameof(code));

    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
}
