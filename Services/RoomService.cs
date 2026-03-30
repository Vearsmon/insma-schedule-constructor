using Dal.Repositories.Campuses;
using Dal.Repositories.Rooms;
using Domain.Dto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
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

    public async Task SaveAsync(SaveRoomDto saveRoomDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (saveRoomDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }
        if (saveRoomDto.Id.HasValue && !(await roomRepository.ExistsAsync(saveRoomDto.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найдена аудитория для обновления"));
        }
        if (!(await campusRepository.ExistsAsync(saveRoomDto.CampusId)))
        {
            validationMessages.Add(new ValidationMessage("Не найден учебный корпус для сохранения аудитории"));
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var room = DtoMappingRegister.Map(saveRoomDto)!;
        await roomRepository.SaveAsync(room);
    }
}