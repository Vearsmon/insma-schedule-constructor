using Domain.Dto;
using Domain.Dto.ErrorDto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/academic-discipline")]
public class AcademicDisciplineController(IAcademicDisciplineService academicDisciplineService) : ApiController
{
    /// <summary>
    /// Получить список корневых академических дисциплин
    /// </summary>
    /// <param name="scheduleId">Идентификатор проекта расписания</param>
    /// <returns>Список моделей-ссылок на корневые академические дисциплины</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(AcademicDisciplineShortDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("search-short")]
    public async Task<AcademicDisciplineShortDto[]> SearchShort(Guid scheduleId) =>
        await academicDisciplineService.SearchShortAsync(scheduleId);

    /// <summary>
    /// Получить реестр данных академических дисциплин
    /// </summary>
    /// <param name="searchModel">Поисковая модель реестра данных академических дисциплин</param>
    /// <returns>Модель ресстра данных академических дисциплин</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistryDto<AcademicDisciplineRegistryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpPost("search")]
    public async Task<RegistryDto<AcademicDisciplineRegistryItemDto>> Search([FromBody] AcademicDisciplineRegistrySearchModel searchModel) =>
        await academicDisciplineService.SearchAsync(searchModel);

    /// <summary>
    /// Получить данные академической дисциплины
    /// </summary>
    /// <param name="academicDisciplineId">Идентификатор академической дисциплины</param>
    /// <returns>Модель просмотра данных академической дисциплины</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(AcademicDisciplineViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("view")]
    public async Task<AcademicDisciplineViewDto> View(Guid academicDisciplineId) =>
        await academicDisciplineService.GetViewAsync(academicDisciplineId);

    /// <summary>
    /// Сохранить данные академической дисциплины
    /// </summary>
    /// <param name="saveAcademicDisciplineDto">Модель сохранения данных академической дисциплины</param>
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
    public async Task Save([FromBody] SaveAcademicDisciplineDto saveAcademicDisciplineDto)
    {
        await academicDisciplineService.SaveAsync(saveAcademicDisciplineDto);
    }

    /// <summary>
    /// Получить список временных отрезков по дням недели, в которых возникнет конфликт при установке серии занятий
    /// по академической дисциплине
    /// </summary>
    /// <param name="academicDisciplineId">Идентификатор академической дисциплины</param>
    /// <param name="academicDisciplineType">Вид занятий, проводимых по академической дисциплине</param>
    /// <returns>Модель списка временных отрезков по дням недели с конфликтами</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(LessonSeriesConflictDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("week-conflicts")]
    public async Task<LessonSeriesConflictDto[]> GetLessonConflicts(Guid academicDisciplineId,
        AcademicDisciplineType academicDisciplineType) =>
        await academicDisciplineService.GetLessonSeriesConflictsAsync(academicDisciplineId, academicDisciplineType);

    /// <summary>
    /// Получить список шифров академических дисциплин
    /// </summary>
    /// <param name="scheduleId">Идентификатор проекта расписания</param>
    /// <returns>Список шифров академических дисциплин</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("search-cyphers")]
    public async Task<string[]> SearchCyphers(Guid scheduleId) =>
        await academicDisciplineService.SearchCyphersAsync(scheduleId);

    /// <summary>
    /// Удалить данные академической дисциплины
    /// </summary>
    /// <param name="academicDisciplineId">Идентификатор академичекой дисциплины</param>
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
    public async Task Delete(Guid academicDisciplineId)
    {
        await academicDisciplineService.DeleteAsync(academicDisciplineId);
    }
}