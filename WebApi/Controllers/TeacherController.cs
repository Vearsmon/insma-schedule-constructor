using Domain.Dto.ErrorDto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/teacher")]
public class TeacherController(ITeacherService teacherService) : ApiController
{
    /// <summary>
    /// Получить реестр данных преподавателей
    /// </summary>
    /// <param name="searchModel">Поисковая модель реестра данных преподавателей</param>
    /// <returns>Модель ресстра данных преподавателей</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistryDto<TeacherRegistryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpPost("search")]
    public async Task<RegistryDto<TeacherRegistryItemDto>> Search([FromBody] TeacherRegistrySearchModel searchModel) =>
        await teacherService.SearchAsync(searchModel);

    /// <summary>
    /// Получить данные преподавателя
    /// </summary>
    /// <param name="teacherId">Идентификатор преподавателя</param>
    /// <returns>Модель просмотра данных преподавателя</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(TeacherViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("view")]
    public async Task<TeacherViewDto> View(Guid teacherId) => await teacherService.GetViewAsync(teacherId);

    /// <summary>
    /// Сохранить данные преподавателя
    /// </summary>
    /// <param name="saveTeacherDto">Модель сохранения данных преподавателя</param>
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
    public async Task Save([FromBody] SaveTeacherDto saveTeacherDto)
    {
        await teacherService.SaveAsync(saveTeacherDto);
    }
}