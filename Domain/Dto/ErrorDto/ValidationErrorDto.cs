using Domain.Models.ValidationMessages;

namespace Domain.Dto.ErrorDto;

public class ValidationErrorDto(string message) : ErrorDto(message)
{
    public ValidationMessage[] ValidationMessages { get; set; } = null!;
}
