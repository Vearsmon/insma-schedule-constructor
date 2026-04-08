using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;

namespace Domain.Services;

public interface IRoomService
{
    Task<RoomTreeDto[]> SearchTreeAsync();

    Task<RegistryDto<RoomRegistryItemDto>> SearchAsync(RoomRegistrySearchModel searchModel);

    Task<RoomViewDto> GetViewAsync(Guid roomId);

    Task<RoomTreeDto[]> GetRoomTreeAsync(RoomSearchModel searchModel);

    Task SaveAsync(SaveRoomDto saveRoomDto);

    Task DeleteAsync(Guid roomId);
}