using System.Text.Json.Serialization;
using Domain.Helpers;
using Domain.Models.Enums;

namespace Domain.Dto.ErrorDto;

public class ErrorDto
{
    public ErrorDto()
    {
    }

    public ErrorDto(string message)
    {
        Id = Guid.NewGuid();
        Message = message;
    }

    public ErrorDto(ServiceExceptionTypes serviceExceptionType)
    {
        Id = Guid.NewGuid();
        Message = serviceExceptionType.Description();
        Code = serviceExceptionType.GetErrorCode();
    }

    [JsonConstructor]
    protected ErrorDto(Guid id, string code, string message)
    {
        Id = id;
        Code = code;
        Message = message;
    }

    public Guid Id { get; set; }

    public string? Code { get; set; }

    public string Message { get; } = null!;

    public string? Title { get; set; }
}
