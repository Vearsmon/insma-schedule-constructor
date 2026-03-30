using AutoFixture;
using Dal.RegistryRepositories.Campus;
using Dal.Repositories.Campuses;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Moq;
using Services;

namespace Tests;

public class CampusServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<ICampusRepository> _campusRepositoryMock = new();
    private readonly Mock<ICampusRegistryRepository> _campusRegistryRepositoryMock = new();

    private CampusService CreateService() => new(
        _campusRepositoryMock.Object,
        _campusRegistryRepositoryMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var campusToSave = _fixture.Build<SaveCampusDto>()
            .With(x => x.Id, Guid.NewGuid())
            .Without(x => x.Name)
            .Create();

        _campusRepositoryMock.Setup(r => r.ExistsAsync(campusToSave.Id!.Value))
            .ReturnsAsync(false);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(campusToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(2, actualException.ValidationMessages.Length);
    }
}