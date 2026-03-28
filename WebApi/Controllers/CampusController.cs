using Domain.Dto.ErrorDto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/campus")]
public class CampusController(ICampusService campusService) : ApiController
{
    /// <summary>
    /// Получить реестр данных учебных корпусов
    /// </summary>
    /// <param name="searchModel">Поисковая модель реестра данных учебных корпусов</param>
    /// <returns>Модель ресстра данных учебных корпусов</returns>
    /// <response code="200">Поиск реестра выполнился успешно</response>
    /// <response code="400">Поиск реестра завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск реестра завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск реестра завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistryDto<CampusRegistryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpPost("search")]
    public async Task<RegistryDto<CampusRegistryItemDto>> Search([FromBody] CampusRegistrySearchModel searchModel) =>
        await campusService.SearchAsync(searchModel);

    /// <summary>
    /// Сохранить данные учебного корпуса
    /// </summary>
    /// <param name="saveCampusDto">Модель сохранения данных учебного корпуса</param>
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
    public async Task Save([FromBody] SaveCampusDto saveCampusDto)
    {
        await campusService.SaveAsync(saveCampusDto);
    }
}