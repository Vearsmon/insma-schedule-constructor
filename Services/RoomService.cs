using Dal.RegistryRepositories.Room;
using Dal.Repositories.Campuses;
using Dal.Repositories.Rooms;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class RoomService(
    IRoomRepository roomRepository,
    IRoomRegistryRepository roomRegistryRepository,
    ICampusRepository campusRepository) : IRoomService
{
    public async Task<RoomTreeDto[]> SearchTreeAsync()
    {
        var items = await roomRepository.SelectAllAsync();
        return items.GroupBy(x => x.CampusId)
            .Select(x => new RoomTreeDto
            {
                CampusId = x.Key,
                CampusName = x.First().Campus.Name,
                ChildRooms = x.Select(y => new RoomShortDto
                {
                    Id = y.Id!.Value,
                    Name = y.Name,
                }).ToArray(),
            })
            .ToArray();
    }

    public async Task<RegistryDto<RoomRegistryItemDto>> SearchAsync(RoomRegistrySearchModel searchModel)
    {
        var registryEntries = await roomRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<RoomRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

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

    public async Task DeleteAsync(Guid roomId)
    {
        await roomRepository.DeleteAsync(roomId);
    }
}