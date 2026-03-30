using AutoFixture;
using Dal.Repositories.Campuses;
using Dal.Repositories.Rooms;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Domain.Models.Enums;
using Moq;
using Services;

namespace Tests;

public class RoomServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<ICampusRepository> _campusRepositoryMock = new();

    private RoomService CreateService() => new(
        _roomRepositoryMock.Object,
        _campusRepositoryMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var roomToSave = _fixture.Build<SaveRoomDto>()
            .With(x => x.Id, Guid.NewGuid())
            .Without(x => x.Name)
            .With(x => x.CampusId, Guid.NewGuid())
            .With(x => x.RoomType, RoomType.Amphitheater)
            .Create();

        _roomRepositoryMock.Setup(r => r.ExistsAsync(roomToSave.Id!.Value)).ReturnsAsync(false);

        _campusRepositoryMock.Setup(r => r.ExistsAsync(roomToSave.CampusId)).ReturnsAsync(false);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(roomToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(3, actualException.ValidationMessages.Length);
    }
}