using Domain.Dto.ErrorDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/lesson")]
public class LessonController(ILessonService lessonService) : ApiController
{
    /// <summary>
    /// Получить список данных занятий на неделю
    /// </summary>
    /// <param name="scheduleId">Идентификатор проекта расписания</param>
    /// <param name="dateFrom">Начало интервала дат поиска</param>
    /// <param name="dateTo">Конец интервала дат поиска</param>
    /// <returns>Список кратких моделей занятий</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(LessonShortDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("search-week")]
    public async Task<LessonShortDto[]> SearchWeek(Guid scheduleId, DateOnly dateFrom, DateOnly dateTo) =>
        await lessonService.SearchWeekAsync(scheduleId, dateFrom, dateTo);

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
    public async Task<LessonViewDto> View(Guid lessonId) => await lessonService.GetViewAsync(lessonId);

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
    /// Удалить данные занятия
    /// </summary>
    /// <param name="scheduleId">Идентификатор проекта расписания</param>
    /// <param name="lessonId">Идентификатор занятия</param>
    /// <response code="200">Удаление данных выполнилось успешно</response>
    /// <response code="400">Удаление данных завершилось с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Удаление данных завершилось с ошибкой валидации прав доступа</response>
    /// <response code="500">Удаление данных завершилось с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpDelete("delete")]
    public async Task Delete(Guid scheduleId, Guid lessonId)
    {
        await lessonService.DeleteAsync(scheduleId, lessonId);
    }
}