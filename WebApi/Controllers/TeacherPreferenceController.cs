using Domain.Dto.ErrorDto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/teacher-preference")]
public class TeacherPreferenceController(ITeacherPreferenceService teacherPreferenceService) : ApiController
{
    /// <summary>
    /// Получить реестр данных пожеланий преподавателей
    /// </summary>
    /// <param name="searchModel">Поисковая модель реестра данных пожеланий преподавателей</param>
    /// <returns>Модель ресстра данных пожеланий преподавателей</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistryDto<TeacherPreferenceRegistryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpPost("search")]
    public async Task<RegistryDto<TeacherPreferenceRegistryItemDto>> Search([FromBody] TeacherPreferenceRegistrySearchModel searchModel) =>
        await teacherPreferenceService.SearchAsync(searchModel);

    /// <summary>
    /// Получить данные пожеланий преподавателя
    /// </summary>
    /// <param name="teacherId">Идентификатор преподавателя</param>
    /// <param name="scheduleId">Идентификатор проекта расписания</param>
    /// <returns>Модель просмотра данных пожеланий преподавателя</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(TeacherPreferenceViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("view")]
    public async Task<TeacherPreferenceViewDto> View(Guid teacherId, Guid scheduleId) =>
        await teacherPreferenceService.GetViewAsync(teacherId, scheduleId);

    /// <summary>
    /// Сохранить данные пожелания преподавателя
    /// </summary>
    /// <param name="saveTeacherPreferenceDto">Модель сохранения данных пожеланий преподавателя</param>
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
    public async Task Save([FromBody] SaveTeacherPreferenceDto saveTeacherPreferenceDto)
    {
        await teacherPreferenceService.SaveAsync(saveTeacherPreferenceDto);
    }
}