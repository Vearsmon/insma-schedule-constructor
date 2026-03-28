namespace Domain.Dto.ErrorDto;

public class ErrorDataDto(string message) : ErrorDto(message)
{
    public string Data { get; set; } = null!;
}
