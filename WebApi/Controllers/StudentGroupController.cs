using Domain.Dto.ErrorDto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/student-group")]
public class StudentGroupController(IStudentGroupService studentGroupService) : ApiController
{
    /// <summary>
    /// Получить реестр данных академических групп
    /// </summary>
    /// <param name="searchModel">Поисковая модель реестра данных академических групп</param>
    /// <returns>Модель ресстра данных академических групп</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistryDto<StudentGroupRegistryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpPost("search")]
    public async Task<RegistryDto<StudentGroupRegistryItemDto>> Search([FromBody] StudentGroupRegistrySearchModel searchModel) =>
        await studentGroupService.SearchAsync(searchModel);

    /// <summary>
    /// Получить данные академической группы
    /// </summary>
    /// <param name="studentGroupId">Идентификатор академической группы</param>
    /// <returns>Модель просмотра данных академической группы</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(StudentGroupViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("view")]
    public async Task<StudentGroupViewDto> View(Guid studentGroupId) =>
        await studentGroupService.GetViewAsync(studentGroupId);

    /// <summary>
    /// Сохранить данные академической группы
    /// </summary>
    /// <param name="saveStudentGroupDto">Модель сохранения данных академической группы</param>
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
    public async Task Save([FromBody] SaveStudentGroupDto saveStudentGroupDto)
    {
        await studentGroupService.SaveAsync(saveStudentGroupDto);
    }
}