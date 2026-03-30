namespace Domain.Models.ValidationMessages;

public class ValidationMessage(
    string message,
    ValidationLevel validationLevel = ValidationLevel.Error,
    string? field = null,
    string? errorCode = null,
    int? sortOrder = null)
{
    public ValidationLevel ValidationLevel { get; set; } = validationLevel;

    public string Message { get; set; } = message;

    public string? Field { get; set; } = field;

    [System.Text.Json.Serialization.JsonIgnore]
    public string? ErrorCode { get; set; } = errorCode;

    [System.Text.Json.Serialization.JsonIgnore]
    public int? SortOrder { get; set; } = sortOrder;
}