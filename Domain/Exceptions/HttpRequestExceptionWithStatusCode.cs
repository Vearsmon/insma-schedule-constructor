using System.Net;

namespace Domain.Exceptions;

public class HttpRequestExceptionWithStatusCode(
    string message,
    Exception? inner,
    HttpStatusCode statusCode,
    Guid? errorId,
    string? title = null)
    : HttpRequestException(message, inner)
{
    public Guid? ErrorId { get; } = errorId;

    public string? Title { get; } = title;

    public new HttpStatusCode StatusCode { get; } = statusCode;
}
