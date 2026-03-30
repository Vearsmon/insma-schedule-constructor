using AutoFixture;
using Dal.RegistryRepositories.StudentGroup;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Services;
using Moq;
using Services;

namespace Tests;

public class StudentGroupServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IStudentGroupRepository> _studentGroupRepositoryMock = new();
    private readonly Mock<IStudentGroupRegistryRepository> _studentGroupRegistryRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<ILessonService> _lessonServiceMock = new();

    private StudentGroupService CreateService() => new(
        _studentGroupRepositoryMock.Object,
        _studentGroupRegistryRepositoryMock.Object,
        _scheduleRepositoryMock.Object,
        _lessonServiceMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data_Thread()
    {
        // Arrange
        var studentGroupToSave = _fixture.Build<SaveStudentGroupDto>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Name)
            .With(x => x.SemesterNumber, 0)
            .With(x => x.StudentGroupType, StudentGroupType.Thread)
            .Without(x => x.Cypher)
            .With(x => x.ParentId, Guid.NewGuid())
            .With(x => x.ChildIds, [Guid.NewGuid()])
            .Create();

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(studentGroupToSave.ScheduleId))
            .ReturnsAsync(false);

        _studentGroupRepositoryMock.Setup(r => r.ExistsAsync(studentGroupToSave.ParentId!.Value))
            .ReturnsAsync(false);

        _studentGroupRepositoryMock.Setup(r => r.GetAsync(studentGroupToSave.Id!.Value, CancellationToken.None))
            .ReturnsAsync(new StudentGroup { ScheduleId = Guid.NewGuid() });

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(studentGroupToSave.ChildIds, CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(studentGroupToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(8, actualException.ValidationMessages.Length);
    }

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data_Semigroup()
    {
        // Arrange
        var studentGroupToSave = _fixture.Build<SaveStudentGroupDto>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Name)
            .With(x => x.SemesterNumber, 0)
            .With(x => x.StudentGroupType, StudentGroupType.SemiGroup)
            .Without(x => x.Cypher)
            .With(x => x.ParentId, Guid.NewGuid())
            .With(x => x.ChildIds, [Guid.NewGuid()])
            .Create();

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(studentGroupToSave.ScheduleId))
            .ReturnsAsync(false);

        _studentGroupRepositoryMock.Setup(r => r.ExistsAsync(studentGroupToSave.ParentId!.Value))
            .ReturnsAsync(false);

        _studentGroupRepositoryMock.Setup(r => r.GetAsync(studentGroupToSave.Id!.Value, CancellationToken.None))
            .ReturnsAsync(new StudentGroup { ScheduleId = Guid.NewGuid() });

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(studentGroupToSave.ChildIds, CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(studentGroupToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(8, actualException.ValidationMessages.Length);
    }
}