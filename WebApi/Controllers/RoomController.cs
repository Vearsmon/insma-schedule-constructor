using Domain.Dto;
using Domain.Dto.ErrorDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.SearchModels;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/room")]
public class RoomController(IRoomService roomService) : ApiController
{
    /// <summary>
    /// Получить данные аудитории
    /// </summary>
    /// <param name="roomId">Идентификатор аудиторию</param>
    /// <returns>Модель просмотра данных аудитории</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(RoomViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("view")]
    public async Task<RoomViewDto> View(Guid roomId) => await roomService.GetViewAsync(roomId);

    /// <summary>
    /// Сохранить данные аудитории
    /// </summary>
    /// <param name="saveRoomDto">Модель для сохранения новой аудитории</param>
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
    public async Task Save([FromBody] SaveRoomDto saveRoomDto)
    {
        await roomService.SaveAsync(saveRoomDto);
    }

    /// <summary>
    /// Получить дерево аудиторий
    /// </summary>
    /// <returns>Модель данных дерева аудиторий</returns>
    /// <response code="200">Поиск данных выполнился успешно</response>
    /// <response code="400">Поиск данных завершился с ошибкой валидации входных данных</response>
    /// <response code="401">Не удалось выполнить авторизацию</response>
    /// <response code="403">Поиск данных завершился с ошибкой валидации прав доступа</response>
    /// <response code="500">Поиск данных завершился с ошибкой</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(RoomTreeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [HttpGet("tree")]
    public async Task<RoomTreeDto[]> GetRoomTree() => await roomService.GetRoomTreeAsync(new RoomSearchModel());
}