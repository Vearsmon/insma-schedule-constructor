using Domain.Dto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.SearchModels;

namespace Domain.Services;

public interface IRoomService
{
    Task<RoomViewDto> GetViewAsync(Guid roomId);

    Task<RoomTreeDto[]> GetRoomTreeAsync(RoomSearchModel searchModel);

    Task SaveAsync(SaveRoomDto saveRoomDto);
}