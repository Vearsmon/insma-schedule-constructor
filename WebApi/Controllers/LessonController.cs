using Domain.Dto;
using Domain.Dto.ErrorDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.Common;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/lesson")]
public class LessonController(ILessonService lessonService) : ApiController
{
    /// <summary>
    /// Получить данные занятия
    /// </summary>
    /// <param name="lessonId">Идентификатор занятия</param>
    /// <returns>Модель просмотра данных занятия</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(LessonViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("view")]
    public async Task<LessonViewDto> View(Guid lessonId) =>
        await lessonService.GetViewAsync(lessonId);

    /// <summary>
    /// Сохранить данные занятия
    /// </summary>
    /// <param name="saveLessonDto">Модель сохранения данных занятия</param>
    /// <response code="200">Сохранение данных выполнилось успешно</response>
    /// <response code="400">Сохранение данных завершилось с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Сохранение данных завершилось с ошибкой валидации прав доступа</response>
    /// <response code="500">Сохранение данных завершилось с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpPost("save")]
    public async Task Save([FromBody] SaveLessonDto saveLessonDto)
    {
        await lessonService.SaveAsync(saveLessonDto);
    }

    /// <summary>
    /// Получить список временных отрезков по дням недели, в которых возникнет конфликт при установке занятия
    /// </summary>
    /// <param name="lessonId">Идентификатор проекта расписания</param>
    /// <param name="dateInterval">Отрезок дат, для которого выполняется поиск</param>
    /// <returns>Модель списка временных отрезков по дням недели с конфликтами</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(LessonWeekConflictDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("week-conflicts")]
    public async Task<LessonWeekConflictDto[]> GetLessonWeekConflicts(Guid lessonId, DateInterval dateInterval) =>
        await lessonService.GetLessonWeekConflictsAsync(lessonId, dateInterval);
}