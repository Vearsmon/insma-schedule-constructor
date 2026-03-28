using System.Net;
using Domain.Helpers;
using Domain.Models.Enums;
using Domain.Models.ValidationMessages;

namespace Domain.Exceptions;

[Serializable]
public class ServiceException : Exception
{
    public ServiceException(ServiceExceptionTypes exceptionType,
                            string? title = null,
                            Exception? innerException = null,
                            HttpStatusCode? statusCode = null,
                            Guid? errorId = null,
                            params string[] errorList) : base(exceptionType.Description(), innerException)
    {
        ExceptionType = exceptionType;
        ErrorList = errorList;
        StatusCode = statusCode;
        Title = title;
        ErrorId = errorId;
    }

    public ServiceException(string title,
                            string message,
                            ServiceExceptionTypes exceptionType,
                            params string[] errorList) : base(message)
    {
        Title = title;
        ExceptionType = exceptionType;
        ErrorList = errorList;
    }

    public ServiceException(string title,
                            string message,
                            ServiceExceptionTypes exceptionType,
                            Exception innerException,
                            params string[] errorList) : base(message, innerException)
    {
        Title = title;
        ExceptionType = exceptionType;
        ErrorList = errorList;
    }

    public ServiceException(string title,
                            string message,
                            ServiceExceptionTypes exceptionType,
                            Exception innerException) : base(message, innerException)
    {
        Title = title;
        ExceptionType = exceptionType;
    }

    public ServiceException(string message, ServiceExceptionTypes exceptionType, params string[] errorList) : base(string.IsNullOrEmpty(message) ? exceptionType.Description() : message)
    {
        ExceptionType = exceptionType;
        ErrorList = errorList;
    }

    public ServiceException(string title, string message, ServiceExceptionTypes exceptionType, string data) : base(message)
    {
        Title = title;
        ExceptionType = exceptionType;
        Data = data;
    }

    public ServiceException(string message, ServiceExceptionTypes exceptionType, string data) : base(message)
    {
        ExceptionType = exceptionType;
        Data = data;
    }

    public ServiceException(HttpStatusCode statusCode, ServiceExceptionTypes exceptionType) : base(exceptionType.Description())
    {
        StatusCode = statusCode;
        ExceptionType = exceptionType;
    }

    public ServiceException(HttpStatusCode statusCode, ServiceExceptionTypes exceptionType, string title) : base(exceptionType.Description())
    {
        StatusCode = statusCode;
        ExceptionType = exceptionType;
        Title = title;
    }

    public ServiceException(HttpStatusCode statusCode, ServiceExceptionTypes exceptionType, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        ExceptionType = exceptionType;
        Title = title;
    }

    public ServiceException(params ValidationMessage[] validationMessages) : base("Ошибка валидации")
    {
        ExceptionType = ServiceExceptionTypes.ValidationError;
        ValidationMessages = validationMessages;
    }

    public ServiceException(string? title, params ValidationMessage[] validationMessages) : base("Ошибка валидации")
    {
        ExceptionType = ServiceExceptionTypes.ValidationError;
        ValidationMessages = validationMessages;
        Title = title;
    }

    public ServiceExceptionTypes ExceptionType { get; }

    public HttpStatusCode? StatusCode { get; }

    public string[] ErrorList { get; } = [];

    public ValidationMessage[] ValidationMessages { get; } = [];

    public string? Title { get; }

    public new string? Data { get; }

    public Guid? ErrorId { get; set; }
}
