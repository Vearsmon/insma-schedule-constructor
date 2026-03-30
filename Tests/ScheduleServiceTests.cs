using AutoFixture;
using Dal.RegistryRepositories.Schedule;
using Dal.Repositories.Schedules;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Moq;
using Services;

namespace Tests;

public class ScheduleServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IScheduleRegistryRepository> _scheduleRegistryRepositoryMock = new();

    private ScheduleService CreateService() => new(
        _scheduleRepositoryMock.Object,
        _scheduleRegistryRepositoryMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var scheduleToSave = _fixture.Build<SaveScheduleDto>()
            .With(x => x.Id, Guid.NewGuid())
            .Without(x => x.Name)
            .Create();

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(scheduleToSave.Id!.Value))
            .ReturnsAsync(false);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(scheduleToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(2, actualException.ValidationMessages.Length);
    }
}