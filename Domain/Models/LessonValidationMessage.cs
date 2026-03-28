using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

public class LessonValidationMessage : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public Guid LessonId { get; set; }

    /// <summary>
    /// Занятие
    /// </summary>
    public Lesson Lesson { get; set; } = null!;

    /// <summary>
    /// Тип ошибки
    /// </summary>
    public LessonValidationErrorType ErrorType { get; set; }

    /// <summary>
    /// Код ошибки
    /// </summary>
    public LessonValidationCode Code { get; set; }

    /// <summary>
    /// Оказавшие влияние данные
    /// </summary>
    public LessonValidationPayload Payload { get; set; } = null!;

    /// <summary>
    /// Сообщение валидации
    /// </summary>
    public string Message { get; set; } = null!;
}