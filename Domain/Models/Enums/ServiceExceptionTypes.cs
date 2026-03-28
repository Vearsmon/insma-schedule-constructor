using System.ComponentModel;

namespace Domain.Models.Enums;

public enum ServiceExceptionTypes
{
    [Description("Сущность не найдена в хранилище данных")]
    EntityNotFound,

    [Description("Ошибка валидации")]
    ValidationError,

    [Description("Доступ запрещен")]
    AccessDenied,

    [Description("Во время работы произошла ошибка")]
    GenericError,

    [Description("Ошибка авторизации")]
    AuthenticationCommonError,

    [Description("Время сессии истекло")]
    SessionExpired,

    [Description("Неверный логин или пароль")]
    AuthorizationError,
}
