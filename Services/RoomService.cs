using Dal.Repositories.Campuses;
using Dal.Repositories.Rooms;
using Domain.Dto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models.SearchModels;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class RoomService(
    IRoomRepository roomRepository,
    ICampusRepository campusRepository) : IRoomService
{
    public async Task<RoomViewDto> GetViewAsync(Guid roomId)
    {
        var room = await roomRepository.GetAsync(roomId);
        return DtoMappingRegister.Map(room)!;
    }

    public async Task<RoomTreeDto[]> GetRoomTreeAsync(RoomSearchModel searchModel)
    {
        var rooms = await roomRepository.SearchAsync(searchModel);
        return rooms
            .GroupBy(room => room.CampusId)
            .Select(group => new RoomTreeDto
            {
                CampusId = group.Key,
                CampusName = group.First().Name,
                ChildRooms = group.Select(room => new RoomShortDto
                {
                    Id = room.Id!.Value,
                    Name = room.Name,
                }).ToArray()
            }).ToArray();
    }

    public async Task<Guid> SaveAsync(SaveRoomDto saveRoomDto)
    {
        if (!(await campusRepository.ExistsAsync(saveRoomDto.CampusId)))
        {
            throw new NotImplementedException();
        }

        var room = DtoMappingRegister.Map(saveRoomDto)!;
        return await roomRepository.SaveAsync(room);
    }
}