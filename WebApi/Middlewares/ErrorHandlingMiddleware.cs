using System.Collections;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Domain.Dto.ErrorDto;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApi.Middlewares;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IOptions<JsonOptions> jsonOptions)
{
    private readonly JsonOptions _jsonOptions = jsonOptions.Value;

    private readonly ServiceExceptionTypes[] _unauthorizedExceptionTypes =
    [
        ServiceExceptionTypes.AuthorizationError,
        ServiceExceptionTypes.SessionExpired,
        ServiceExceptionTypes.AuthenticationCommonError
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ServiceException serviceException) when (serviceException.StatusCode != null)
        {
            await SendError(context, serviceException, GetErrorDto(serviceException), serviceException.StatusCode!.Value);
        }
        catch (ServiceException serviceException) when (serviceException.ExceptionType == ServiceExceptionTypes.AccessDenied)
        {
            await SendError(context, serviceException, GetErrorDto(serviceException), HttpStatusCode.Forbidden);
        }
        catch (ServiceException serviceException) when (((IList)_unauthorizedExceptionTypes).Contains(serviceException.ExceptionType))
        {
            await SendError(context, serviceException, GetErrorDto(serviceException), HttpStatusCode.Unauthorized);
        }
        catch (ServiceException serviceException) when (serviceException.ExceptionType == ServiceExceptionTypes.ValidationError)
        {
            var error = new ValidationErrorDto(serviceException.Message)
            {
                Title = serviceException.Title,
                Code = serviceException.ExceptionType.GetErrorCode(),
                ValidationMessages = serviceException.ValidationMessages.OrderByDescending(x => x.ValidationLevel).ToArray()
            };
            await SendError(context, serviceException, error, HttpStatusCode.BadRequest);
        }
        catch (ServiceException serviceException) when (serviceException.ExceptionType == ServiceExceptionTypes.EntityNotFound)
        {
            await SendError(context, serviceException, GetErrorDto(serviceException), HttpStatusCode.NotFound);
        }
        catch (ServiceException serviceException)
        {
            var statusCode = serviceException.ExceptionType == ServiceExceptionTypes.GenericError
                ? HttpStatusCode.InternalServerError
                : HttpStatusCode.BadRequest;

            await SendError(context, serviceException, GetErrorDto(serviceException), statusCode);
        }
        catch (Exception exception)
        {
            await SendError(context, exception);
        }
    }

    private static ErrorDto GetErrorDto(ServiceException serviceException)
    {
        if (!string.IsNullOrEmpty(serviceException.Data))
        {
            return new ErrorDataDto(serviceException.Message)
            {
                Id = serviceException.ErrorId ??= Guid.NewGuid(),
                Title = serviceException.Title,
                Code = serviceException.ExceptionType.GetErrorCode(),
                Data = serviceException.Data
            };
        }

        return new ErrorDto(serviceException.Message)
        {
            Id = serviceException.ErrorId ??= Guid.NewGuid(),
            Title = serviceException.Title,
            Code = serviceException.ExceptionType.GetErrorCode()
        };
    }

    private async Task SendError(HttpContext context,
                                 Exception exception,
                                 ErrorDto? error = null,
                                 HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)statusCode;

        error ??= new ErrorDto("Во время работы произошла ошибка");

        var message = exception.FlattenException(error.Id);
        logger.LogError("{Message}", message);

        var responseJson = JsonSerializer.Serialize<object>(error, _jsonOptions.JsonSerializerOptions);
        await context.Response.WriteAsync(responseJson);
    }
}