using AutoFixture;
using Dal.RegistryRepositories.Teacher;
using Dal.Repositories.Teachers;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Moq;
using Services;

namespace Tests;

public class TeacherServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<ITeacherRepository> _teacherRepositoryMock = new();
    private readonly Mock<ITeacherRegistryRepository> _teacherRegistryRepositoryMock = new();

    private TeacherService CreateService() => new(
        _teacherRepositoryMock.Object,
        _teacherRegistryRepositoryMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var teacherToSave = _fixture.Build<SaveTeacherDto>()
            .With(x => x.Id, Guid.NewGuid())
            .Without(x => x.Fullname)
            .Without(x => x.Contacts)
            .Create();

        _teacherRepositoryMock.Setup(r => r.ExistsAsync(teacherToSave.Id!.Value))
            .ReturnsAsync(false);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(teacherToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(2, actualException.ValidationMessages.Length);
    }
}